# الدرس 7: Controllers و Action Results

---

## 1. المقدمة
يمكن كتابة الكود الذي يرد على الطلبات داخل `Program.cs`. هذا يصلح لمثال صغير، لكن مع عشرات الـ endpoints يصبح الملف كبيرًا ومزدحمًا. الحل في ASP.NET Core هو الـ **Controllers**: كلاسات منظمة، كل كلاس مسؤول عن موضوع واحد.

---

## 2. ما هي فكرة الـ Controller؟

### التعريف
**الـ Controller مكوّن برمجي يعمل كنقطة دخول (Entry Point) إلى التطبيق من الخارج.** يستقبل مُدخلًا خارجيًا (في الويب: طلب HTTP)، ويفسّره، ويوجّهه إلى الجزء المسؤول عنه من المنطق، ثم يحوّل النتيجة إلى مُخرَج مناسب (في الويب: رد HTTP).

### الفكرة: فصل المسؤوليات (Separation of Concerns)
في أي تطبيق ويب هناك أمران مختلفان يجب ألا يختلطا:

| الأمر | ماذا يشمل |
|---|---|
| **كيف يصل الطلب ويخرج الرد** | بروتوكول HTTP، والمسارات، وصيغة JSON، والـ Status Codes |
| **منطق التطبيق** | القواعد والعمليات: تسجيل طالب، التحقق من تكرار بريد، منع حذف مادة فيها طلاب |

والـ Controller هو **الطبقة الوسيطة (Mediator) التي تترجم بينهما**:
- كل ما يخص **HTTP** يبقى في الـ Controller.
- وكل ما يخص **القواعد** يبقى في المنطق.
- فيمكن تغيير أحدهما دون أن ينكسر الآخر، مثل أن يتغير شكل الرد بدون المساس بالقواعد.

### وظائفه الثلاث: مُدخل ← توجيه ← مُخرَج
| الوظيفة | ماذا تعني |
|---|---|
| **المُدخل (Input)** | قراءة بيانات الطلب (من المسار أو الـ Query أو الجسم أو الـ Headers) وتحويلها إلى أنواع برمجية |
| **التوجيه (Dispatch)** | اختيار الجزء المناسب من المنطق واستدعاؤه |
| **المُخرَج (Output)** | تحويل النتيجة إلى رد HTTP: البيانات والـ Status Code |

وهو بهذا **منسّق** وليس منفّذًا: لا يحمل القواعد نفسها، بل يربط الطلب بها.
### ليس خاصًا بـ ASP.NET Core
الـ Controller **فكرة من أنماط تصميم البرمجيات** اسمها **MVC (Model-View-Controller)**، ظهرت في أواخر السبعينات، وتفصل التطبيق إلى ثلاثة أدوار:

| الدور | مسؤوليته |
|---|---|
| **Model** | البيانات وقواعدها |
| **View** | ما يراه المستخدم (العرض) |
| **Controller** | يستقبل ما يفعله المستخدم، ويحرّك الـ Model، ويختار ما يُعرض |

ولهذا تجد الفكرة نفسها في تقنيات كثيرة، أحيانًا بأسماء مختلفة:
- **ASP.NET Core** (C#): `Controller`.
- **Spring** (Java): `@Controller` و`@RestController`.
- **Laravel** (PHP): `Controller`.
- **Ruby on Rails** (Ruby): `Controller`.
- **Django** (Python): الـ `View` يؤدي دور الـ Controller.
- **Express** (JavaScript): دوال معالجة المسارات، ويُنظّمها كثيرون في كلاسات أو ملفات تسمى Controllers.
- وفي تطبيقات الموبايل والـ Desktop أيضًا: Controller بين الواجهة والمنطق.

فإذا فهمت الفكرة هنا، فهمتها في أي تقنية، والذي يتغير هو **الأسلوب** (الـ Attributes والكلاسات)، لا **الدور**.

### أين الـ View في الـ Web API؟
في المواقع التقليدية يعيد الـ Controller صفحة HTML (View). أما في الـ **Web API** فلا توجد صفحة: **الرد نفسه (JSON) هو ما يراه العميل**، فيبقى الـ Controller والـ Model، ويصير الـ View هو شكل البيانات المرجَعة.

---

## 3. ما مسؤولية الـ Controller؟

**ما يفعله:**
1. **يستقبل** الطلب من خلال الـ Action المناسب.
2. **يأخذ البيانات** منه (من المسار أو الـ Query أو الجسم أو الـ Headers).
3. **يستدعي** المنطق الذي ينفّذ المطلوب.
4. **يحوّل النتيجة إلى رد HTTP** مناسب: البيانات والـ Status Code (`200` أو `404` أو `201`...).

**ما لا ينبغي أن يفعله:**
- **منطق العمل المعقد** (حساب الأسعار، وقواعد الأعمال).
- **الوصول المباشر إلى قاعدة البيانات**.
- هذه الأعمال تُوضع في كلاسات أخرى (Services) يستدعيها الـ Controller، فيبقى الـ Controller **صغيرًا وواضحًا**.

في مثالنا التعليمي نكتب المنطق البسيط داخل الـ Controller لتسهيل الشرح، وفي المشاريع الكبيرة ينتقل إلى خدمات مستقلة.

**في ASP.NET Core:** الـ Controller **كلاس عادي** نرثه من `ControllerBase`، ودوالّه التي ترد على الطلبات تسمى **Actions**. وكل Controller مسؤول عن **مورد واحد**، مثل:
- `ProductsController` لكل ما يخص المنتجات.
- `OrdersController` لكل ما يخص الطلبات.

فبدل أن تكون كل الـ endpoints في `Program.cs`، يصبح لكل موضوع كلاس.

---

## 4. كيف يعمل؟ رحلة الطلب

عندما يصل طلب مثل `GET /api/products/2`، تحدث هذه الخطوات بالترتيب:

| # | الخطوة | ماذا يحدث |
|---|---|---|
| 1 | **الاستقبال** | يستقبل Kestrel الطلب وينشئ `HttpContext` |
| 2 | **الـ Pipeline** | يمر الطلب عبر الـ Middlewares |
| 3 | **الـ Routing** | يطابق المسار والـ Verb مع الـ Actions المسجّلة (سجّلها `MapControllers()` عند التشغيل)، ويختار Action واحدًا |
| 4 | **إنشاء الـ Controller** | يُنشأ **كائن جديد** من الـ Controller صاحب هذا الـ Action، وتُمرَّر له ما يطلبه الـ Constructor من خدمات |
| 5 | **الـ Model Binding** | تتحول بيانات الطلب إلى معاملات الـ Action (مثل `id = 2`) |
| 6 | **تنفيذ الـ Action** | يُنفَّذ كود الـ Action |
| 7 | **النتيجة** | يرجع الـ Action قيمة أو Action Result (مثل `Ok(product)`) |
| 8 | **كتابة الرد** | يحوّل ASP.NET Core النتيجة إلى رد: JSON وStatus Code |
| 9 | **التخلص** | يُتخلَّص من كائن الـ Controller |

ملاحظة: خطوة 3 تحدد **أي Action** بالمسار والـ Verb معًا، ولذلك يمكن أن يخدم المسار نفسه Actions مختلفة حسب الـ Verb.

---

## 5. كيف يعثر ASP.NET Core على الـ Controllers؟

عند تشغيل التطبيق يفحص `MapControllers()` كلاسات المشروع، ويعدّ الكلاس Controller إذا كان:
- **`public`** وليس `abstract`،
- و**يرث من `ControllerBase`**، أو **ينتهي اسمه بكلمة `Controller`**، أو عليه `[ApiController]`،
- وليس عليه `[NonController]`.

جرّبنا ست حالات على كلاسات فيها Action مسار:

| الحالة | النتيجة |
|---|---|
| كلاس يرث `ControllerBase`، واسمه لا ينتهي بـ `Controller` | يعمل `200` |
| كلاس عليه `[ApiController]` بدون كلاس أب | يعمل `200` |
| كلاس اسمه ينتهي بـ `Controller` بدون كلاس أب | يعمل `200` |
| كلاس عادي بلا أب وبلا Attribute وبلا اسم `...Controller` | **لا يعمل** `404` |
| كلاس يرث `ControllerBase` وعليه `[NonController]` | **لا يعمل** `404` |
| كلاس يرث `ControllerBase` لكنه `internal` | **لا يعمل** `404` |

**القاعدة العملية:** اجعل الكلاس `public`، واجعله يرث من `ControllerBase`، وسمّه `XxxController`. وهذا ما نفعله دائمًا، وهو الأوضح لأي قارئ.

---

## 6. عمر الـ Controller: كائن جديد لكل طلب

الـ Controller **ليس كائنًا واحدًا يعيش طوال التطبيق**، بل يُنشأ **من جديد مع كل طلب**:
```csharp
[ApiController]
[Route("api/time")]
public class TimeController : ControllerBase
{
    private readonly Guid _instanceId = Guid.NewGuid();
    private static int _created;

    public TimeController()
    {
        _created++;
    }

    [HttpGet]
    public string Get() => $"instance = {_instanceId.ToString()[..8]}, created so far = {_created}";
}
```
افتح `/api/time` ثلاث مرات:
```
instance = a1819a42, created so far = 1
instance = baf7880f, created so far = 2
instance = c3fce1d4, created so far = 3
```
- في كل طلب **رقم مختلف**: كائن جديد.
- وعدّاد الكائنات المنشأة زاد مع كل طلب.

**ماذا يعني هذا؟**
- **لا تحفظ بيانات الطلب في حقل داخل الـ Controller** وتتوقع أن تجدها في الطلب التالي، فكائن الطلب التالي جديد.
- البيانات التي يجب أن تبقى بين الطلبات توضع في **مكان خارج الـ Controller** (قاعدة بيانات، أو خدمة Singleton مثل `InMemoryData` في المثال).

---

## 7. تجهيز المشروع

القالب الفارغ (`dotnet new web`) لا يعرف الـ Controllers، فنضيف سطرين في `Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();      // 1) تسجيل خدمات الـ Controllers

var app = builder.Build();

app.MapControllers();                   // 2) ربط مسارات الـ Controllers

app.Run();
```
- `AddControllers()` قبل `Build()`: يسجّل الخدمات التي تحتاجها الـ Controllers.
- `MapControllers()` بعد `Build()`: يبحث عن الـ Controllers ويجعل مساراتها تعمل.
- **بدون `MapControllers()`** يرجع أي طلب إلى الـ Controller `404 Not Found`.

وننشئ مجلدًا اسمه `Controllers/` لنضع فيه الـ Controllers (الاسم عُرف فقط، والمهم الكلاس نفسه).

---

## 8. أول Controller

أنشئ الملف `Controllers/HelloController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;

namespace ProjectStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public string Get()
    {
        return "Hello from a controller";
    }
}
```
شغّل التطبيق وافتح `http://localhost:5149/api/hello`:
```
Hello from a controller
```

### شرح كل جزء

| الجزء | ما هو |
|---|---|
| `ControllerBase` | الكلاس الأب. يعطينا ما نحتاجه في الـ API، مثل الوصول إلى `HttpContext` (`Request` و`Response` و`User`) |
| `[ApiController]` | يجعل الكلاس Controller لـ API، ويفعّل له سلوكيات تلقائية |
| `[Route("api/[controller]")]` | المسار الذي يستقبل عليه هذا الـ Controller طلباته |
| `[HttpGet]` | هذه الدالة تستجيب لطلبات **GET** |
| `Get()` | الـ **Action**: الدالة التي تُنفَّذ وترد على الطلب |

### من أين جاء `/api/hello`؟
- `[Route("api/[controller]")]` فيه كلمة `[controller]`، وهي **تُستبدل باسم الكلاس بدون كلمة Controller**.
- `HelloController` ← `Hello` ← المسار `api/hello` (المسارات لا تفرّق بين الحروف الكبيرة والصغيرة).
- تسمية الكلاس بـ `...Controller` هي العُرف المتبع، وبها يحصل `[controller]` على اسم المسار (وكيف يتعرف ASP.NET Core على الـ Controllers في القسم 5).

### ما الفرق بين `ControllerBase` و`Controller`؟
- `ControllerBase`: للـ APIs، وهو ما نستخدمه.
- `Controller`: يرث من `ControllerBase` ويضيف دعم صفحات الـ HTML (Views)، ولا نحتاجه في الـ API.

### ماذا يعطينا `[ApiController]`؟
- يجعل تحديد المسار بالـ Attributes **إلزاميًا**.
- يُرجع `400 Bad Request` تلقائيًا إذا كانت البيانات القادمة غير صالحة.
- يستنتج من أين تأتي معاملات الـ Action (من الجسم أو المسار...).

---

## 9. ماذا يرجع الـ Action؟

الـ Action يرجع **أي نوع**، والـ ASP.NET Core يحوّله إلى رد:
- `string` ← نص عادي.
- كائن أو قائمة ← **JSON** تلقائيًا.

مثال بمنتجات:

`Models/Product.cs`:
```csharp
namespace ProjectStructure.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
```
`Controllers/ProductsController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using ProjectStructure.Models;

namespace ProjectStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Keyboard", Price = 25 },
        new Product { Id = 2, Name = "Mouse", Price = 12 },
        new Product { Id = 3, Name = "Monitor", Price = 140 }
    };

    [HttpGet]
    public List<Product> GetAll()
    {
        return _products;
    }

    [HttpGet("{id}")]
    public Product? GetById(int id)
    {
        return _products.FirstOrDefault(p => p.Id == id);
    }
}
```
- `_products` قائمة ثابتة في الذاكرة بدل قاعدة بيانات (للتبسيط).
- `[HttpGet]` على `GetAll`: يستجيب لـ `GET /api/products`.
- `[HttpGet("{id}")]` على `GetById`: يستجيب لـ `GET /api/products/2`، والقيمة `2` تصل إلى المعامل `id`.

جرّب:

`GET /api/products`:
```json
[{"id":1,"name":"Keyboard","price":25},{"id":2,"name":"Mouse","price":12},{"id":3,"name":"Monitor","price":140}]
```
`GET /api/products/2`:
```json
{"id":2,"name":"Mouse","price":12}
```
لاحظ أن القائمة والكائن تحوّلا إلى **JSON** بدون أي كود منا، وأن أسماء الخصائص صارت بحروف صغيرة (`id` و`name`).

---

## 10. أكثر من Action في Controller واحد

كل Action يحدد الـ HTTP Verb الذي يستجيب له:

| الـ Attribute | الـ Verb | يُستخدم عادةً لـ |
|---|---|---|
| `[HttpGet]` | GET | قراءة البيانات |
| `[HttpPost]` | POST | إنشاء عنصر جديد |
| `[HttpPut]` | PUT | تعديل عنصر |
| `[HttpDelete]` | DELETE | حذف عنصر |

أضف إلى `ProductsController`:
```csharp
[HttpPost]
public string Create()
{
    return "Create a product";
}

[HttpPut("{id}")]
public string Update(int id)
{
    return $"Update product {id}";
}

[HttpDelete("{id}")]
public string Delete(int id)
{
    return $"Delete product {id}";
}
```
(هذه أمثلة بسيطة تُظهر فقط أي Action يُنفَّذ.)

جرّب بأداة مثل Postman أو `curl`:
```
POST   /api/products      ← Create a product
PUT    /api/products/2    ← Update product 2
DELETE /api/products/3    ← Delete product 3
```
- نفس المسار `/api/products` يُنفِّذ Action مختلفًا حسب الـ **Verb**.
- إن أرسلت Verb ليس له Action (مثل `DELETE /api/hello`) يرجع `405 Method Not Allowed`.

---

## 11. ما هو الـ Action Result؟

**ما هو؟** كائن يمثل **الرد كاملًا**: الـ Status Code، وجسم الرد (Body) إن وُجد، والـ Headers.

**لماذا يوجد؟** لنتحكم في الرد: ليس فقط "ماذا نرجع" بل أيضًا "بأي Status Code". فمثلًا:
- وجدنا المنتج: نرجعه مع `200 OK`.
- لم نجده: نرجع `404 Not Found`.
- أنشأنا منتجًا جديدًا: نرجع `201 Created`.

**من ينشئه ومتى؟** الـ Action هو الذي ينشئه ويرجعه، عبر دوال جاهزة في `ControllerBase` مثل `Ok()` و`NotFound()`. ثم ينفّذه ASP.NET Core ويكتب الرد الفعلي.

---

## 12. المشكلة: إرجاع القيمة مباشرة

```csharp
[HttpGet("{id}")]
public Product? GetById(int id)
{
    return _products.FirstOrDefault(p => p.Id == id);
}
```
- إذا وُجد المنتج: يرجع JSON مع `200 OK`.
- إذا **لم يوجد**: يرجع الـ Action قيمة `null`، فيكون الرد **`204 No Content`** بدون أي رسالة، وليس `404`.
- لا نستطيع اختيار الـ Status Code، وهذا خطأ في المعنى: `204` تعني "نجح الطلب ولا يوجد محتوى"، بينما المنتج غير موجود أصلًا.

---

## 13. الحل: `ActionResult<T>`

```csharp
[HttpGet("{id}")]
public ActionResult<Product> GetById(int id)
{
    Product? product = _products.FirstOrDefault(p => p.Id == id);

    if (product == null)
        return NotFound();

    return Ok(product);
}
```
النتيجة:

`GET /api/products/2`:
```
200 OK
{"id":2,"name":"Mouse","price":12}
```
`GET /api/products/99`:
```
404 Not Found
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Not Found","status":404,"traceId":"..."}
```
- `NotFound()` يرجع `404`.
- `Ok(product)` يرجع `200` مع المنتج.
- لاحظ أن الـ `404` رجع معه جسم JSON فيه تفاصيل الخطأ (`title` و`status`)، وهذا ما يفعله `[ApiController]` تلقائيًا (يسمى **ProblemDetails**).

---

## 14. `ActionResult<T>` أم `IActionResult`؟

| النوع | ماذا يرجع | متى نستخدمه |
|---|---|---|
| `ActionResult<T>` | إما قيمة من النوع `T`، أو أي Action Result | عندما نعرف نوع البيانات عند النجاح (مثل `Product`) |
| `IActionResult` | أي Action Result فقط | عندما لا نرجع بيانات عند النجاح (مثل الحذف والتعديل) |

- `ActionResult<Product>` يوضّح أن هذا الـ Action يرجع `Product` عند النجاح، وهو أوضح للقارئ وللأدوات التي تولّد توثيق الـ API.
- `IActionResult` يكفي للـ Actions التي ترجع `NoContent()` أو `NotFound()` فقط.

---

## 15. أشهر الـ Action Results

| الدالة | الـ Status Code | متى نستخدمها |
|---|---|---|
| `Ok(data)` | `200 OK` | نجح الطلب ومعه بيانات |
| `CreatedAtAction(...)` | `201 Created` | أنشأنا عنصرًا جديدًا |
| `NoContent()` | `204 No Content` | نجح الطلب ولا بيانات لنرجعها (تعديل وحذف) |
| `BadRequest()` | `400 Bad Request` | الطلب نفسه خاطئ |
| `Unauthorized()` | `401 Unauthorized` | العميل غير مسجّل الدخول |
| `Forbid()` | `403 Forbidden` | مسجّل لكن لا يملك الصلاحية (يعمل بعد إعداد الـ Authentication) |
| `NotFound()` | `404 Not Found` | العنصر المطلوب غير موجود |
| `Conflict()` | `409 Conflict` | تعارض مع الحالة الحالية (مثل عنصر مكرر) |
| `StatusCode(500)` | الرقم الذي تحدده | أي Status Code آخر |

**مجموعات الأرقام:**
| الأرقام | المعنى |
|---|---|
| `2xx` | نجاح |
| `3xx` | إعادة توجيه |
| `4xx` | خطأ من العميل (طلب خاطئ) |
| `5xx` | خطأ من الخادم |

---

## 16. مثال كامل: CRUD مع الـ Action Results

```csharp
using Microsoft.AspNetCore.Mvc;
using ProjectStructure.Models;

namespace ProjectStructure.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Keyboard", Price = 25 },
        new Product { Id = 2, Name = "Mouse", Price = 12 },
        new Product { Id = 3, Name = "Monitor", Price = 140 }
    };

    [HttpGet]
    public ActionResult<List<Product>> GetAll()
    {
        return Ok(_products);
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        Product? product = _products.FirstOrDefault(p => p.Id == id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public ActionResult<Product> Create(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Product updated)
    {
        Product? product = _products.FirstOrDefault(p => p.Id == id);

        if (product == null)
            return NotFound();

        product.Name = updated.Name;
        product.Price = updated.Price;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        Product? product = _products.FirstOrDefault(p => p.Id == id);

        if (product == null)
            return NotFound();

        _products.Remove(product);

        return NoContent();
    }
}
```
- `Product` هو الكلاس الذي فيه `Id` و`Name` و`Price`.
- في `Create` و`Update` يصل `product` من **جسم الطلب** (Body) بصيغة JSON.
- `CreatedAtAction(nameof(GetById), new { id = product.Id }, product)`:
  - يرجع `201 Created`.
  - ويضيف Header اسمه `Location` فيه رابط المنتج الجديد (أي رابط `GetById`).
  - ويرجع المنتج الجديد في الجسم.

---

## 17. التجربة: كل طلب وما يرجعه

للـ POST والـ PUT أرسل Header `Content-Type: application/json` وجسمًا مثل `{"name":"Headset","price":60}` (بـ Postman أو `curl`).

| الطلب | الرد |
|---|---|
| `GET /api/products` | `200 OK` وقائمة المنتجات |
| `GET /api/products/2` | `200 OK` والمنتج |
| `GET /api/products/99` | `404 Not Found` |
| `POST /api/products` بجسم صحيح | `201 Created` + `Location: .../api/Products/4` + المنتج الجديد |
| `GET /api/products/4` | `200 OK` والمنتج الجديد |
| `PUT /api/products/2` بجسم صحيح | `204 No Content` |
| `PUT /api/products/99` | `404 Not Found` |
| `DELETE /api/products/3` | `204 No Content` |
| `DELETE /api/products/99` | `404 Not Found` |
| `POST /api/products` بجسم JSON ناقص | `400 Bad Request` (يرجعه `[ApiController]` تلقائيًا) |

- بعد `PUT /api/products/2` أعد `GET /api/products/2` وسترى الاسم والسعر الجديدين.
- الـ `400` في آخر صف لم نكتبه نحن، وإنما `[ApiController]` هو الذي رفض الطلب لأن الـ JSON غير صالح.

---

## 18. الملخص والواجب

| المفهوم | ما هو |
|---|---|
| **فكرة الـ Controller** | منسّق بين العالم الخارجي ومنطق التطبيق، وهي من نمط MVC ولا تخص تقنية بعينها |
| **مسؤوليته** | يستقبل، ويأخذ البيانات، ويستدعي المنطق، ويرجع الرد (لا يحمل منطقًا معقدًا) |
| **رحلة الطلب** | استقبال ← Pipeline ← Routing ← إنشاء الـ Controller ← Binding ← الـ Action ← الرد |
| **عمر الـ Controller** | كائن جديد لكل طلب، فلا تحفظ فيه بيانات تحتاجها في الطلب التالي |
| **Controller في ASP.NET Core** | كلاس `public` يرث `ControllerBase` ويُسمّى `XxxController`، لكل موضوع واحد |
| **Action** | دالة في الـ Controller ترد على طلب |
| `AddControllers()` و`MapControllers()` | سطرا التفعيل في `Program.cs` |
| `ControllerBase` | الكلاس الأب للـ APIs |
| `[ApiController]` | يجعله Controller لـ API بسلوكيات تلقائية |
| `[Route("api/[controller]")]` | المسار، و`[controller]` تُستبدل باسم الكلاس بدون `Controller` |
| `[HttpGet]` و`[HttpPost]`... | الـ Verb الذي يستجيب له الـ Action |
| **Action Result** | كائن يمثل الرد: الـ Status Code والجسم والـ Headers |
| `ActionResult<T>` | يرجع `T` عند النجاح أو أي Action Result |
| `IActionResult` | أي Action Result فقط |
| `Ok` و`NotFound` و`NoContent` و`BadRequest` | أشهر الدوال، ولكل منها Status Code |
| `CreatedAtAction` | `201` مع `Location` والعنصر الجديد |
| `2xx` و`4xx` و`5xx` | نجاح، وخطأ من العميل، وخطأ من الخادم |

واجب:
1. أنشئ `CustomersController` فيه الـ CRUD الكامل (`GetAll` و`GetById` و`Create` و`Update` و`Delete`) بالـ Action Results المناسبة.
2. اجعل `Create` يرجع `409 Conflict` إذا كان اسم العميل موجودًا من قبل.
3. جرّب كل طلب في جدول التجربة وسجّل الـ Status Code الذي يرجع.
4. احذف `app.MapControllers()` وجرّب أي رابط، ماذا يظهر؟
5. غيّر `[Route("api/[controller]")]` إلى `[Route("shop/[controller]")]` وجرّب الرابط الجديد.