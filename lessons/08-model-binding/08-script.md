# الدرس 8: Model Binding

---

## 1. المقدمة
يصل الطلب إلى الخادم كنصوص: رابط وHeaders وجسم JSON. لكن الـ Action يريد معاملات جاهزة: رقم `int` وكائن `Product`. من يحوّل هذه النصوص إلى معاملات؟ هذا عمل الـ **Model Binding**.

---

## 2. ما هو الـ Model Binding؟

**ما هو؟** عملية تلقائية تأخذ البيانات من الطلب وتحوّلها إلى **معاملات الـ Action** بالأنواع الصحيحة.

**لماذا يوجد؟** بدونه سنقرأ كل قيمة من الطلب بأيدينا ونحوّلها: نقرأ النص `"5"` ثم نحوّله إلى `int`، ونقرأ الجسم ثم نحوّل الـ JSON إلى كائن. الـ Model Binding يفعل هذا كله عنا.

**من ينفّذه ومتى؟** ASP.NET Core، **قبل** تنفيذ الـ Action مباشرة: ينظر في معاملات الـ Action، ويبحث لكل معامل عن قيمته في الطلب، ويحوّلها إلى نوع المعامل، ثم ينفّذ الـ Action.

---

## 3. من أين تأتي البيانات؟

للطلب عدة أجزاء، ولكل جزء Attribute يحدد أن المعامل يؤخذ منه:

| المصدر | الـ Attribute | مثال |
|---|---|---|
| **المسار** (Route) | `[FromRoute]` | `/api/products/5` ← `id = 5` |
| **Query String** | `[FromQuery]` | `/api/products?name=Ali&page=2` |
| **الجسم** (Body) | `[FromBody]` | جسم JSON بالطلب (POST وPUT) |
| **الـ Headers** | `[FromHeader]` | Header اسمه `X-Course` |
| **الفورم** (Form) | `[FromForm]` | بيانات فورم HTML |
| **الخدمات** (Services) | `[FromServices]` | خدمة من الـ IoC Container |

وفي أغلب الحالات **لا نكتب الـ Attribute**، فـ `[ApiController]` يستنتج المصدر (انظر القسم 10).

---

## 4. تجربة 1: من المسار (Route)

```csharp
[HttpGet("route/{id}")]
public string FromRoute(int id)
{
    return $"id = {id}";
}
```
`GET /api/binding/route/5`:
```
id = 5
```
- `{id}` في المسار يطابق المعامل `id`.
- النص `"5"` تحوّل تلقائيًا إلى `int`.

## 5. تجربة 2: من الـ Query String

```csharp
[HttpGet("query")]
public string FromQuery(string name, int page = 1)
{
    return $"name = {name}, page = {page}";
}
```
| الطلب | النتيجة |
|---|---|
| `GET /api/binding/query?name=Ali&page=3` | `name = Ali, page = 3` |
| `GET /api/binding/query?name=Ali` | `name = Ali, page = 1` |

- `page = 1` قيمة افتراضية: إن لم يرسلها العميل تُستخدم.
- أما `name` بلا قيمة افتراضية فهو **مطلوب** (انظر القسم 9).

## 6. تجربة 3: كائن كامل من الـ Query String

```csharp
public class SearchFilter
{
    public string? Name { get; set; }
    public int Page { get; set; } = 1;
}

[HttpGet("filter")]
public string FromFilter([FromQuery] SearchFilter filter)
{
    return $"name = {filter.Name}, page = {filter.Page}";
}
```
| الطلب | النتيجة |
|---|---|
| `GET /api/binding/filter?name=Ali&page=2` | `name = Ali, page = 2` |
| `GET /api/binding/filter` | `name = , page = 1` |

- بدل معاملات كثيرة، نجمعها في كائن واحد.
- كل خاصية تُملأ من الـ Query String بنفس اسمها.
- كتبنا `[FromQuery]` هنا لأن الكائنات المركبة تُؤخذ من الجسم افتراضيًا، فنحتاج أن نطلب الـ Query صراحة.

## 7. تجربة 4: من الجسم (Body)

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

[HttpPost("body")]
public string FromBody(Product product)
{
    return $"name = {product.Name}, price = {product.Price}";
}
```
أرسل `POST /api/binding/body` مع Header `Content-Type: application/json` وجسم:
```json
{"name":"Headset","price":60}
```
النتيجة:
```
name = Headset, price = 60
```
- الـ JSON تحوّل إلى كائن `Product` وامتلأت خصائصه.
- أسماء الخصائص في الـ JSON **لا تفرّق بين الحروف الكبيرة والصغيرة**: الجسم `{"Name":"Upper","PRICE":5}` يعمل أيضًا ويعطي `name = Upper, price = 5`.

## 8. تجربة 5: من الـ Header

```csharp
[HttpGet("header")]
public string FromHeader([FromHeader(Name = "X-Course")] string course)
{
    return $"course = {course}";
}
```
أرسل `GET /api/binding/header` مع Header `X-Course: ASP`:
```
course = ASP
```
- اسم الـ Header (`X-Course`) يختلف عن اسم المعامل (`course`)، فنحدده بـ `Name`.

### تجربة 6: خليط من أكثر من مصدر
```csharp
[HttpPut("mix/{id}")]
public string Mix(int id, Product product, bool notify)
{
    return $"id = {id}, name = {product.Name}, notify = {notify}";
}
```
أرسل `PUT /api/binding/mix/7?notify=true` مع جسم `{"name":"Headset","price":60}`:
```
id = 7, name = Headset, notify = True
```
- `id` من **المسار**، و`product` من **الجسم**، و`notify` من **الـ Query String**.
- الـ Binding أخذ كل معامل من مصدره بدون أي Attribute.

---

## 9. عندما يفشل الـ Binding

إذا لم يستطع ASP.NET Core تحويل القيمة أو لم يجدها، فإن `[ApiController]` **يرد `400 Bad Request` تلقائيًا ولا ينفّذ الـ Action**:

| الحالة | النتيجة |
|---|---|
| `GET /api/binding/route/abc` (النص ليس رقمًا) | `400`: `The value 'abc' is not valid.` |
| `GET /api/binding/query` بدون `name` | `400`: `The name field is required.` |
| جسم `{"name":"Bad","price":"abc"}` | `400`: `The JSON value could not be converted to System.Decimal.` |
| `GET /api/binding/header` بدون الـ Header | `400`: `The course field is required.` |

- الرد يأتي بصيغة `ProblemDetails` وفيه قائمة `errors` تخبر العميل أي معامل فشل ولماذا.
- كل معامل من نوع نص **بدون قيمة افتراضية وبدون `?`** يُعدّ **مطلوبًا**.
- لجعل المعامل **اختياريًا**: أعطه قيمة افتراضية (`int page = 1`) أو اجعله يقبل `null` (`string? name`).

---

## 10. كيف يعرف مصدر كل معامل؟

عند وجود `[ApiController]`، يستنتج ASP.NET Core المصدر بهذه القواعد:

| نوع المعامل | المصدر |
|---|---|
| **كائن مركّب** (مثل `Product`) | **الجسم** (`[FromBody]`) |
| **نوع بسيط** (`int` و`string` و`bool`...) واسمه موجود في المسار (`{id}`) | **المسار** |
| **نوع بسيط** غير موجود في المسار | **الـ Query String** |

وكتابة الـ Attribute صراحة تغيّر هذا الاستنتاج، مثل `[FromQuery]` على الكائن، و`[FromHeader]` للـ Headers.

**ملاحظة:** الـ Action الواحد يأخذ **كائنًا واحدًا فقط من الجسم**.

---

## 11. ما الذي يحتاج Attribute وما الذي لا يحتاجه؟

ليست كل المصادر تُربط بنفس الطريقة. نقسّمها إلى ثلاث مجموعات:

| المجموعة | المعنى | أمثلة |
|---|---|---|
| **أ. تحتاج Attribute** | لا يستنتجها ASP.NET Core، فنكتب الـ Attribute | الـ Headers، وكائن مركّب من الـ Query |
| **ب. لا تحتاج Attribute** | يستنتجها تلقائيًا | المسار، والـ Query، والجسم، والخدمات المسجّلة، و`CancellationToken`، و`IFormFile` |
| **ج. لا تُربط أصلًا** | ليس لها Attribute، نقرؤها من الـ `Request` مباشرة | الكوكيز، والجسم الخام |

### أ. ما يحتاج Attribute

| المصدر | الـ Attribute | لماذا |
|---|---|---|
| **Header** | `[FromHeader(Name = "X-Course")]` | لا يُستنتج أبدًا، فبدونه يُعامَل المعامل كأنه من الـ Query |
| **كائن مركّب من الـ Query** | `[FromQuery]` | الكائن المركّب يُؤخذ من الجسم افتراضيًا |
| **بيانات فورم** | `[FromForm]` | الفورم ليس مصدرًا افتراضيًا |

مثال (من القسم 8): `[FromHeader(Name = "X-Course")] string course` يعمل `200`، وبدون الـ Attribute يرجع `400`.

### ب. ما لا يحتاج Attribute

```csharp
public class Greeter
{
    public string Hello() => "Hello from a service";
}

// في Program.cs:
// builder.Services.AddSingleton<Greeter>();

[HttpGet("service")]
public string FromService(Greeter greeter) => greeter.Hello();

[HttpGet("token")]
public string Token(CancellationToken cancellationToken)
    => $"can be cancelled = {cancellationToken.CanBeCanceled}";

[HttpPost("file")]
public string File(IFormFile file)
    => $"file = {file.FileName}, {file.Length} bytes";
```
| الطلب | النتيجة |
|---|---|
| `GET /api/noattr/service` | `Hello from a service` |
| `GET /api/noattr/token` | `can be cancelled = True` |
| `POST /api/noattr/file` (ملف مرفوع) | `file = upload.txt, 12 bytes` |

- **الخدمات:** إذا كان نوع المعامل مسجّلًا في الـ IoC Container، فيُحقن تلقائيًا ولا يحتاج `[FromServices]`. وكتابة `[FromServices]` صراحة تعمل أيضًا لكنها غير لازمة.
- **`CancellationToken`:** يُربط بحالة الاتصال، ويُلغى إذا قطع العميل الطلب. مفيد لإيقاف العمل الطويل.
- **`IFormFile`:** يمثل ملفًا مرفوعًا بصيغة `multipart/form-data`.
- ومعها كل ما ذكرناه في القسم 10: المسار، والـ Query، وكائن مركّب من الجسم.

### ج. ما لا يُربط: الكوكيز

لا يوجد `[FromCookie]`. وإذا كتبت معاملًا باسم الكوكي بدون Attribute، يظنه ASP.NET Core من الـ Query:
```csharp
[HttpGet("cookie-param")]
public string CookieParam(string session) => $"session = {session}";
```
أرسل الطلب مع `Cookie: session=abc123`:
```
400 Bad Request  (The session field is required.)
```
لأن `session` بحث عنه في الـ Query ولم يجده. **الحل: نقرأ الكوكي من الـ `Request`:**
```csharp
[HttpGet("cookie")]
public string Cookie()
{
    if (!Request.Cookies.TryGetValue("session", out string? session))
        return "no session cookie";

    return $"session = {session}";
}
```
| الطلب | النتيجة |
|---|---|
| مع `Cookie: session=abc123` | `session = abc123` |
| بدون كوكي | `no session cookie` |

- `Request.Cookies` من الـ `HttpContext` مباشرة، وهو **ليس** Model Binding.
- ولإرسال كوكي في الرد: `Response.Cookies.Append("session", "abc123");` فيصل Header `Set-Cookie: session=abc123; path=/`.
- ويمكن ربط الـ Header كله بـ `[FromHeader(Name = "Cookie")] string cookie`، لكنك تحصل على **النص كاملًا** (`session=abc123`)، فالقراءة من `Request.Cookies` أنسب.

---

## 12. القراءة المباشرة من `Request`

الـ Model Binding مريح، لكنه لا يغطي كل شيء. حين لا يناسبنا نقرأ من الـ `Request` مباشرة، وهو خاصية جاهزة في الـ Controller:

| ما نقرؤه | الكود | مثال النتيجة |
|---|---|---|
| قيمة من الـ Query | `Request.Query["name"]` | `Ali` (وكل المفاتيح: `Request.Query.Keys`) |
| Header | `Request.Headers["X-Course"]` | `ASP` |
| كوكي | `Request.Cookies["session"]` | `abc123` |
| حقل فورم | `Request.Form["name"]` | `Sara` |
| الجسم الخام | `await new StreamReader(Request.Body).ReadToEndAsync()` | `just text` |

```csharp
[HttpGet("query")]
public string Query()
{
    string? name = Request.Query["name"];
    return $"name = {name ?? "(none)"}, all keys = {string.Join(",", Request.Query.Keys)}";
}

[HttpPost("raw")]
public async Task<string> Raw()
{
    using StreamReader reader = new StreamReader(Request.Body);
    string body = await reader.ReadToEndAsync();
    return $"raw body = {body}";
}
```
- `GET /api/direct/query?name=Ali&page=2` ← `name = Ali, all keys = name,page`.
- `POST /api/direct/raw` بنص `just text` ← `raw body = just text`.

**ملاحظة:** لا تضع `HttpContext` أو `HttpRequest` معاملًا في الـ Action داخل Controller:
```csharp
public string Context(HttpContext context) => ...   // 500 Internal Server Error
```
الرد `500` والرسالة `Could not create an instance of type 'HttpContext'`. في الـ Controller استخدم الخاصيتين الجاهزتين `HttpContext` و`Request`.

### متى نستخدم هذا وذاك؟
| الحالة | الأنسب |
|---|---|
| قيمة بنوع محدد (`int` و`Product`) تريدها جاهزة، ومعها `400` تلقائي عند الخطأ | **Model Binding** |
| كوكي | **`Request.Cookies`** |
| مفاتيح لا تعرف أسماءها مسبقًا (كل الـ Query أو كل الـ Headers) | **`Request.Query` و`Request.Headers`** |
| الجسم نص خام أو ليس JSON | **`Request.Body`** |

---

## 13. الملخص والواجب

| المفهوم | ما هو |
|---|---|
| **Model Binding** | تحويل بيانات الطلب إلى معاملات الـ Action تلقائيًا |
| `[FromRoute]` | من المسار |
| `[FromQuery]` | من الـ Query String |
| `[FromBody]` | من جسم الطلب (JSON) |
| `[FromHeader]` | من الـ Headers |
| قيمة افتراضية أو `?` | تجعل المعامل اختياريًا |
| فشل الـ Binding | `400 Bad Request` تلقائيًا بدون تنفيذ الـ Action |
| القواعد الافتراضية | كائن مركّب ← الجسم، ونوع بسيط ← المسار أو الـ Query |
| بدون Attribute | المسار والـ Query والجسم المركّب والخدمات المسجّلة وCancellationToken وIFormFile |
| تحتاج Attribute | الـ Headers ([FromHeader])، وكائن مركّب من الـ Query ([FromQuery])، والفورم ([FromForm]) |
| الكوكيز | لا يوجد [FromCookie]: نقرأ Request.Cookies |
| القراءة المباشرة | Request.Query وRequest.Headers وRequest.Form وRequest.Body |

واجب:
1. اكتب Action `GET /api/binding/sum?a=2&b=3` يرجع مجموع العددين.
2. اكتب Action يستقبل `SearchFilter` فيه `Name` و`Page` و`PageSize` من الـ Query، ويرجع النص `name = ..., page = ..., size = ...`.
3. جرّب إرسال `a=abc` في تمرين 1، وسجّل الرد الذي يظهر.
4. اكتب Action `PUT` يأخذ `id` من المسار و`Product` من الجسم و`discount` من الـ Query، وجرّبه.
5. اكتب Action يقرأ كوكيًا اسمه 	heme من Request.Cookies، ويرجع dark إن لم يجده.
6. اكتب Action يرسل كوكي isited بـ Response.Cookies.Append، وانظر الـ Header Set-Cookie في الرد.
7. اكتب Action يقبل خدمة مسجّلة كمعامل بدون [FromServices].
