# الدرس 9: Swagger / OpenAPI

---

## 1. المقدمة
أنشأت API فيه عشرات المسارات. كيف يعرف مبرمج آخر (أو أنت بعد شهر) ما هي المسارات؟ وأي Verb يستخدم؟ وما شكل البيانات المطلوبة والراجعة؟ كتابة ملف توثيق يدويًا تتقادم بسرعة. الحل: أن **يُولَّد التوثيق من الكود نفسه** فيبقى دقيقًا دائمًا. وهذا ما يقدمه **OpenAPI**.

---

## 2. ما هو الـ OpenAPI؟

**ما هو؟** **مواصفة (Specification) قياسية لوصف الـ HTTP APIs** بصيغة JSON أو YAML تقرؤها الأدوات والبشر. الوثيقة تصف:
- **المسارات** (Paths) والـ **Verbs** التي يقبلها كل مسار.
- **المعاملات** (من المسار أو الـ Query أو الجسم).
- **النماذج** (Schemas) أي شكل البيانات المرسلة والراجعة.
- **الردود** الممكنة بأرقام الـ Status Codes.

**ما الفرق بين OpenAPI وSwagger؟**
| الاسم | ما هو |
|---|---|
| **OpenAPI** | **المواصفة** نفسها، أي صيغة الوثيقة |
| **Swagger** | كان اسم المواصفة في البداية، وأُعيدت تسميتها OpenAPI عام 2016. واليوم يُقصد بـ Swagger **الأدوات** المبنية عليها، مثل **Swagger UI** |

- **الوثيقة (OpenAPI):** ملف `openapi/v1.json` فيه وصف الـ API.
- **الواجهة (Swagger UI):** صفحة ويب تقرأ هذا الملف وتعرضه بشكل مرتب، وتتيح **تجربة الـ API من المتصفح**.

**لماذا نحتاجه؟**
1. توثيق **دائم التحديث** لأنه يُولَّد من الكود.
2. **تجربة** كل Endpoint من المتصفح بدون Postman.
3. أي أداة تفهم OpenAPI تقرأ الوثيقة وتستفيد منها.

---

## 3. من يولّد الوثيقة؟

ASP.NET Core **يقرأ الـ Controllers والـ Actions** (المسارات والـ Verbs والمعاملات وأنواع الإرجاع) **ويبني الوثيقة منها**. فلا نكتب الوثيقة بأيدينا، بل نفعّل التوليد ونوجّهه بمعلومات إضافية.

---

## 4. الخطوة 1: إضافة الحزمة

الوثيقة يولّدها الحزمة `Microsoft.AspNetCore.OpenApi`، وهي **ليست** جزءًا من القالب الفارغ (`dotnet new web`):
```powershell
dotnet add package Microsoft.AspNetCore.OpenApi --version "10.*"
```
**انتبه إلى رقم الإصدار:** أضف الإصدار المطابق لـ .NET الذي تستخدمه (هنا `10.*`). إذا كتبت الأمر بدون `--version` قد يُضاف الإصدار الأحدث (`11.0.0` عند تجربتي) وهو لا يناسب مشروع `net10.0`، فيظهر الخطأ:
```
error CS1061: 'IServiceCollection' does not contain a definition for 'AddOpenApi'
```

## 5. الخطوة 2: التفعيل في `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();              // 1) تسجيل خدمة توليد الوثيقة

var app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                       // 2) نشر الوثيقة على مسار
}

app.Run();
```
- `AddOpenApi()` قبل `Build()`: يسجّل الخدمة التي تولّد الوثيقة.
- `MapOpenApi()` بعد `Build()`: ينشر الوثيقة على المسار `/openapi/v1.json`.
- وضعناه داخل `IsDevelopment()` لتظهر الوثيقة أثناء التطوير فقط (انظر القسم 10).

افتح `http://localhost:5149/openapi/v1.json` وستجد وثيقة JSON. أهم مفاتيحها:

| المفتاح | ما فيه |
|---|---|
| `openapi` | نسخة المواصفة، وكانت `3.1.1` عند تجربتي |
| `info` | عنوان الـ API ونسخته، وافتراضيًا `اسم المشروع \| v1` |
| `paths` | كل المسارات والـ Verbs |
| `components` | النماذج (`schemas`)، مثل `Product` |

مقتطف مختصر من وصف `GET /api/Products/{id}` (حذفت منه الصيغ المكررة):
```json
"/api/Products/{id}": {
  "get": {
    "tags": [ "Products" ],
    "parameters": [
      { "name": "id", "in": "path", "required": true,
        "schema": { "type": [ "integer", "string" ], "format": "int32" } }
    ],
    "responses": {
      "200": { "description": "OK", "content": {
        "application/json": { "schema": { "$ref": "#/components/schemas/Product" } } } }
    }
  }
}
```
- `tags`: اسم الـ Controller.
- `parameters`: المعامل `id` ومصدره `in: path`.
- `responses`: الردود الممكنة، ومعها شكل البيانات عبر `$ref` إلى `Product`.

وهذا ما تقرؤه الواجهات والأدوات.

---

## 6. الخطوة 3: واجهة Swagger UI

الوثيقة JSON ليست مريحة للقراءة. نضيف **Swagger UI** لتعرضها:
```powershell
dotnet add package Swashbuckle.AspNetCore.SwaggerUI --version "10.*"
```
وفي `Program.cs` داخل `IsDevelopment()`:
```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Shop API v1");
    });
}
```
- `SwaggerEndpoint(...)`: يخبر الواجهة **أين الوثيقة** ويعطيها اسمًا في القائمة.
- هذه الحزمة تعرض الواجهة فقط، أما **الوثيقة** فتولّدها `Microsoft.AspNetCore.OpenApi`.

افتح `http://localhost:5149/swagger`، وستجد:
- عنوان الـ API ونسخته (`Shop API v1`) وصيغة المواصفة (`OAS 3.1`).
- قائمة **الـ Endpoints** مجمّعة باسم الـ Controller (`Products`)، ولكل واحد لون حسب الـ Verb.
- قسم **Schemas** فيه النماذج (`Product`).
- عند فتح أي Endpoint تجد زر **Try it out** لتجربته وإرسال الطلب وعرض الرد.

### بديل: Scalar
واجهة أخرى بنفس الغرض:
```powershell
dotnet add package Scalar.AspNetCore
```
```csharp
using Scalar.AspNetCore;

app.MapOpenApi();
app.MapScalarApiReference();      // الواجهة على /scalar/v1
```
تعمل بنفس الوثيقة (`/openapi/v1.json`)، وتغيّر شكل العرض فقط. ويمكن استخدام إحدى الواجهتين.

---

### مشكلة شائعة: زر Try it out يرفض المعامل

**الأعراض:** تفتح Endpoint فيه `{id}` مثل `GET /api/courses/{id}`، وتضغط **Try it out**، وتكتب `1`، ثم **Execute**، فيظهر خطأ أحمر ولا يُرسل الطلب:
```
Please correct the following validation errors and try again.
For 'id': Required field is not provided.
```
ويظهر تحت اسم المعامل النوع `integer | string($int32)`. مع أن الـ API نفسه سليم: الطلب بـ `curl` أو Postman يعمل `200`.

**السبب:** الوثيقة تصف الرقم بنوعين: `"type": ["integer", "string"]` مع `pattern`، لأن إعدادات JSON الافتراضية تسمح بقراءة الأرقام من نصوص (`"7"` بدل `7`). ويبدو أن Swagger UI الذي جرّبته لا يتعامل جيدًا مع هذا النوع المتعدد في معامل المسار، فيعدّ الحقل فارغًا. (هذا ما استنتجته من التجربة: عندما غيّرت المخطط إلى نوع واحد، اختفى الخطأ.)

**الحل:** نجعل الأرقام **صارمة (Strict)** في خيارات JSON التي يقرأ منها مولّد الوثيقة، في `Program.cs`:
```csharp
using System.Text.Json.Serialization;

builder.Services.AddControllers();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});
```
**النتيجة:**
| | قبل الحل | بعد الحل |
|---|---|---|
| مخطط `id` في الوثيقة | `{"type":["integer","string"],"pattern":"...","format":"int32"}` | `{"type":"integer","format":"int32"}` |
| النوع في Swagger UI | `integer \| string($int32)` | `integer($int32)` |
| Try it out ثم Execute | خطأ `Required field is not provided` | `200` ويعرض الرد |

**ما لم يتغير:** سلوك الـ API نفسه. جرّبت طلبات `UniversityApi` الـ22 قبل الحل وبعده وأعطت نفس الرموز، وإرسال رقم كنص (`{"id":"7"}`) ما زال يُقبل (`201`) في الحالتين. فالإعداد يغيّر **وصف الوثيقة** ولا يغيّر طريقة قراءة الطلبات في الـ Controllers.

**ملاحظة:** جرّبت هذا الحل في المتصفح على مشروعين (`UniversityApi` و`SwaggerDemo`)، وخطأ المعامل اختفى في Endpoints الـ `{id}` كلها التي جرّبتها. أما Scalar فلم أجرّب Try it out فيه.

---

## 7. تحسين الوثيقة (1): معلومات الـ API

عنوان الوثيقة الافتراضي `اسم المشروع | v1`. نغيّره بـ **Document Transformer**:
```csharp
using Microsoft.OpenApi;

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Shop API",
            Version = "v1",
            Description = "A small API for managing products."
        };
        return Task.CompletedTask;
    });
});
```
النتيجة في الوثيقة:
```json
"info": { "title": "Shop API", "description": "A small API for managing products.", "version": "v1" }
```
وتظهر في Swagger UI: العنوان `Shop API` والوصف تحته.

## 8. تحسين الوثيقة (2): توثيق الردود

الوثيقة تعرف نوع الرد الناجح من نوع الإرجاع (`ActionResult<Product>`)، لكنها **لا تعرف الردود الأخرى** التي نرجعها بـ `NotFound()` و`CreatedAtAction()`. فقبل التحسين:

| الـ Action | الردود في الوثيقة |
|---|---|
| `GET /api/Products/{id}` | `200` فقط |
| `POST /api/Products` | `200` فقط (والحقيقة `201`) |

نوثّقها بـ `[ProducesResponseType]`:
```csharp
[HttpGet("{id}")]
[ProducesResponseType<Product>(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public ActionResult<Product> GetById(int id)
{
    Product? product = _products.FirstOrDefault(p => p.Id == id);

    if (product == null)
        return NotFound();

    return Ok(product);
}

[HttpPost]
[ProducesResponseType<Product>(StatusCodes.Status201Created)]
public ActionResult<Product> Create(Product product)
{
    product.Id = _products.Max(p => p.Id) + 1;
    _products.Add(product);

    return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
}
```
بعد التحسين:

| الـ Action | الردود في الوثيقة |
|---|---|
| `GET /api/Products/{id}` | `200` و`404` |
| `POST /api/Products` | `201` |

- `[ProducesResponseType<Product>(200)]`: الرد الناجح، ومعه نوع البيانات.
- `[ProducesResponseType(404)]`: رد بدون بيانات محددة (يظهر معه `ProblemDetails` في الـ Schemas).
- هذه الـ Attributes **توثّق فقط**، ولا تغيّر ما يرجعه الكود فعلًا.

---

## 9. الـ Attributes التي نوثّق بها الـ Endpoint

لا نكتب الوثيقة بأيدينا، لكن نوجّهها بـ Attributes نضعها على الـ Action أو الـ Controller. هذه كل ما جرّبته، وما يظهر منها في الوثيقة:

| الـ Attribute | أين نضعه | ماذا يضيف إلى الوثيقة |
|---|---|---|
| `[ProducesResponseType<T>(status)]` | Action | رد بهذا الـ Status Code، ومعه نوع البيانات `T` |
| `[ProducesResponseType(status)]` | Action | رد بهذا الـ Status Code بدون بيانات (مثل `404` و`204`) |
| `[ProducesDefaultResponseType]` | Action | رد `default` (أي رد غير مذكور) |
| `[Produces("application/json")]` | Action أو Controller | يحصر **أنواع محتوى الرد** في JSON. بدونه تظهر `text/plain` و`application/json` و`text/json` |
| `[Consumes("application/json")]` | Action أو Controller | يحدد **نوع محتوى الطلب** (الجسم)، ويظهر `application/json` و`application/*+json` |
| `[EndpointSummary("...")]` | Action | **عنوان قصير** للـ Endpoint (`summary`) |
| `[EndpointDescription("...")]` | Action | **وصف أطول** (`description`) |
| `[Tags("Catalog")]` | Action أو Controller | **المجموعة** التي يظهر تحتها الـ Endpoint. الافتراضي اسم الـ Controller |
| `[HttpGet(Name = "GetProduct")]` | Action | **اسم العملية** (`operationId`) |
| `[ApiExplorerSettings(IgnoreApi = true)]` | Action أو Controller | **يخفي** الـ Endpoint من الوثيقة (يبقى يعمل) |
| تعليقات XML (`/// <summary>`) | Action | `summary` من `<summary>` و`description` من `<remarks>` |

**أمثلة:**
```csharp
[HttpGet("{id}", Name = "GetProduct")]
[EndpointSummary("Get one product")]
[EndpointDescription("Returns the product with the given id, or 404.")]
[Tags("Catalog")]
[ProducesResponseType<Product>(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public ActionResult<Product> GetById(int id) { ... }
```
النتيجة في الوثيقة: `operationId = GetProduct`، و`summary = Get one product`، والوصف، والمجموعة `Catalog`، والردان `200` و`404`.

**تعليقات XML:** تعمل بعد تفعيل الإعداد في ملف المشروع `.csproj`:
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```
```csharp
/// <summary>Returns all products.</summary>
/// <remarks>XML comment remarks go to the description.</remarks>
[HttpGet]
public ActionResult<List<Product>> GetAll() { ... }
```
النتيجة: `summary = Returns all products.` و`description = XML comment remarks go to the description.` (والسطر `NoWarn` فقط لإخفاء تحذير الدوال بلا تعليق).

**ملاحظات من التجربة:**
- `[Obsolete]` على الـ Action **لا يظهر** في الوثيقة كـ `deprecated` (في .NET 10 عند تجربتي).
- `[ApiController]` **لا يضيف `400` تلقائيًا** إلى الوثيقة، مع أنه يرجعه فعلًا عند الجسم الخاطئ. فـ Action `POST` بلا Attributes أظهر الرد `200` فقط. لذلك نكتب `400` بأنفسنا.
- `[Produces]` و`[Consumes]` و`[Tags]` و`[ApiExplorerSettings]` يمكن وضعها على الـ Controller كله فتسري على كل Actions.

---

## 10. ماذا نضيف لكل Verb؟

لكل Verb ردود تتكرر، ولكل رد سببه في الكود:

| الـ Verb | الردود الشائعة | من أين يأتي كل رد |
|---|---|---|
| **GET** (قائمة) | `200` | `Ok(list)` |
| **GET** (عنصر بالـ id) | `200`، `404` | `Ok(item)`، و`NotFound()` |
| **POST** (إنشاء) | `201`، `400`، `409` | `CreatedAtAction(...)`، وجسم خاطئ، و`Conflict()` |
| **PUT** (تعديل) | `204`، `400`، `404` | `NoContent()`، وجسم خاطئ، و`NotFound()` |
| **DELETE** (حذف) | `204`، `404`، `409` | `NoContent()`، و`NotFound()`، و`Conflict()` |

(وإذا كان الـ Action يشير إلى عناصر أخرى، مثل تسجيل طالب في مادة، يُضاف `404` أيضًا للـ POST.)

**ما نكتبه فوق الـ Action لكل Verb:**
```csharp
[HttpGet]
[EndpointSummary("Get all students")]
[ProducesResponseType<List<Student>>(StatusCodes.Status200OK)]
public ActionResult<List<Student>> GetAll() { ... }

[HttpGet("{id}")]
[EndpointSummary("Get one student")]
[ProducesResponseType<Student>(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public ActionResult<Student> GetById(int id) { ... }

[HttpPost]
[EndpointSummary("Create a student")]
[ProducesResponseType<Student>(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public ActionResult<Student> Create(Student student) { ... }

[HttpPut("{id}")]
[EndpointSummary("Update a student")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public IActionResult Update(int id, Student updated) { ... }

[HttpDelete("{id}")]
[EndpointSummary("Delete a student")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public IActionResult Delete(int id) { ... }
```
- أول Attribute بعد الـ Verb: `[EndpointSummary]` ليظهر عنوان واضح في الواجهة.
- ثم `[ProducesResponseType]` لكل رد يرجعه الكود فعلًا.
- ارجع إلى الكود: لكل `return` برقم مختلف (`NotFound()` و`Conflict()` و`NoContent()`) ينبغي أن يكون عندك `[ProducesResponseType]` له.

---

## 11. التطبيق على مشروع `UniversityApi`

طبّقنا هذا على مشروع الطلاب والمواد والتسجيلات (`UniversityApi`): أضفنا الحزمتين، والتفعيل في `Program.cs` مع عنوان `University API`، والـ Attributes لكل Action كما في القسم 10.

افتح `http://localhost:5120/swagger`، وستجد ثلاث مجموعات (`Courses` و`Enrollments` و`Students`)، وهذه الردود التي تعرضها الوثيقة لكل Endpoint:

| الـ Endpoint | الردود |
|---|---|
| `GET /api/students` و`/api/courses` و`/api/enrollments` | `200` |
| `GET .../{id}` | `200`، `404` |
| `GET /api/students/{id}/courses` و`/api/courses/{id}/students` | `200`، `404` |
| `POST /api/students` و`/api/courses` | `201`، `400`، `409` |
| `POST /api/enrollments` | `201`، `400`، `404`، `409` |
| `PUT /api/students/{id}` و`/api/courses/{id}` | `204`، `400`، `404` |
| `DELETE /api/students/{id}` و`/api/courses/{id}` | `204`، `404`، `409` |
| `DELETE /api/enrollments/{id}` | `204`، `404` |

ولم يتغير سلوك الـ API: كل طلبات `requests.http` (22 طلبًا) ما زالت تعطي نفس النتائج.

---

## 12. ملاحظة: ماذا يعني `ActionResult<T>` للتوثيق؟
عندما يكون نوع الإرجاع `ActionResult<Product>` تعرف الوثيقة أن الرد الناجح هو `Product`. ولهذا فضّلناه على `IActionResult` في الأمثلة، فمع `IActionResult` لا يوجد نوع تستنتج منه الوثيقة شكل الرد، فنحتاج `[ProducesResponseType<T>]` دائمًا.

---

## 13. الوثيقة في الإنتاج

وضعنا `MapOpenApi()` و`UseSwaggerUI()` داخل `IsDevelopment()`. فعند تشغيل التطبيق في بيئة `Production`:

| المسار | النتيجة |
|---|---|
| `/openapi/v1.json` | `404` |
| `/swagger` | `404` |
| `/api/products` (الـ API نفسه) | `200` |

- الـ API يعمل كالمعتاد، لكن الوثيقة والواجهة **غير منشورتين**.
- لا تنشر وثيقة الـ API للعامة إلا إذا أردت ذلك، لأنها تكشف كل المسارات.
- وإن أردت نشرها في الإنتاج فأخرج السطرين من شرط `IsDevelopment()`.

---

## 14. مشروع الدرس الجاهز: `SwaggerDemo`

المشروع الذي يجمع كل ما في الدرس موجود في مجلد `SwaggerDemo` بجانب هذا الملف:
```
SwaggerDemo/
├── Models/Product.cs
├── Controllers/ProductsController.cs   (كل الـ Attributes في أمثلة الدرس)
├── Program.cs                          (OpenAPI + Swagger UI + Scalar + معلومات الوثيقة)
├── SwaggerDemo.csproj                  (الحزم الثلاث + تعليقات XML)
└── requests.http                       (14 طلبًا مع الـ Status Code المتوقع لكل طلب)
```

**ما فيه:**
- الحزم: `Microsoft.AspNetCore.OpenApi` و`Swashbuckle.AspNetCore.SwaggerUI` و`Scalar.AspNetCore`.
- في `Program.cs`: `AddOpenApi` مع Document Transformer بعنوان `Shop API`، و`MapOpenApi()` و`UseSwaggerUI(...)` و`MapScalarApiReference()` داخل `IsDevelopment()`.
- وأيضًا ConfigureHttpJsonOptions بـ NumberHandling = Strict ليقبل Swagger UI معاملات المسار (انظر المشكلة الشائعة في القسم 6).
- في `.csproj`: `GenerateDocumentationFile` لتعليقات XML.
- في `ProductsController`: `[Produces]` على الـ Controller، وعلى الـ Actions: `[EndpointSummary]` و`[EndpointDescription]` و`[Tags]` و`[Consumes]` و`[ProducesResponseType]` و`Name` للـ `operationId` وتعليق XML، وEndpoint مخفي بـ `[ApiExplorerSettings(IgnoreApi = true)]`.

**التشغيل:** `dotnet run` ثم افتح (المنفذ `5234`):

| الرابط | ما تراه |
|---|---|
| `/openapi/v1.json` | الوثيقة |
| `/swagger` | Swagger UI |
| `/scalar/v1` | Scalar |
| `/api/products/hidden` | يعمل `200` لكنه غير موجود في الوثيقة |

وشغّل طلبات `requests.http` من الأعلى إلى الأسفل، وكلها تعطي الرمز المكتوب في عنوانها (جرّبتها كلها: 14 طلبًا). وفي بيئة `Production` يرجع `/openapi/v1.json` و`/swagger` و`/scalar/v1` بـ `404` ويبقى `/api/products` بـ `200`.

---
## 15. الملخص والواجب

| المفهوم | ما هو |
|---|---|
| **OpenAPI** | مواصفة لوصف الـ HTTP APIs بصيغة JSON أو YAML |
| **Swagger** | اسم المواصفة قديمًا، واليوم يُقصد به الأدوات مثل Swagger UI |
| `Microsoft.AspNetCore.OpenApi` | الحزمة التي تولّد الوثيقة من الكود |
| `AddOpenApi()` و`MapOpenApi()` | التفعيل، والوثيقة على `/openapi/v1.json` |
| **Swagger UI** | واجهة تعرض الوثيقة وتجرّب الـ API (`/swagger`) |
| **Scalar** | واجهة بديلة (`/scalar/v1`) |
| **Document Transformer** | تغيير معلومات الوثيقة (العنوان والوصف) |
| `[ProducesResponseType]` | توثيق الردود الممكنة وأنواعها |
| [EndpointSummary] و[EndpointDescription] و[Tags] | عنوان ووصف ومجموعة الـ Endpoint |
| [Produces] و[Consumes] | أنواع محتوى الرد والطلب |
| [ApiExplorerSettings(IgnoreApi = true)] | إخفاء Endpoint من الوثيقة |
| ردود كل Verb | GET: 200/404، POST: 201/400/409، PUT: 204/400/404، DELETE: 204/404/409 |
| `IsDevelopment()` | عرض الوثيقة أثناء التطوير فقط |

واجب:
1. فعّل OpenAPI وSwagger UI في مشروعك، وافتح `/openapi/v1.json` و`/swagger`.
   ثم جرّب **Try it out** على Endpoint فيه {id}: إن ظهر خطأ Required field is not provided فطبّق الحل في القسم 6.
2. غيّر عنوان الوثيقة ووصفها بـ Document Transformer.
3. وثّق ردود `Update` و`Delete` (`204` و`404`) بـ `[ProducesResponseType]`، وانظر النتيجة في Swagger UI.
4. أضف [EndpointSummary] و[ProducesResponseType] المناسبة لكل Action في UniversityApi (GET وPOST وPUT وDELETE)، وقارن ما تراه في Swagger UI بالجدول في القسم 11.
5. أخفِ Endpoint واحدًا بـ [ApiExplorerSettings(IgnoreApi = true)] وتأكد أنه اختفى من الواجهة ويعمل في Postman.
6. فعّل تعليقات XML في ملف المشروع واكتب <summary> لـ Action، وانظر أين يظهر.
7. جرّب Endpoint من Swagger UI بزر **Try it out**، وقارن الرد بما يرجع في Postman.
8. شغّل التطبيق بالبيئة `Production` وسجّل ما يظهر عند فتح `/swagger`.
