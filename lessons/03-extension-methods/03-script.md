# الدرس: Extension Methods

---

## 1. المقدمة
افتح `Program.cs` وانظر: `builder.Services.AddControllers()` و`app.UseHttpsRedirection()` و`app.MapGet(...)`. هل تعرف أن `IServiceCollection` و`WebApplication` لا يحتويان على هذه الدوال أصلًا؟ كلها Extension Methods. فما هي؟ وكيف نكتب مثلها؟

## 2. ما هو الـ Extension Method؟

**ما هو؟** طريقة لإضافة دالة جديدة إلى نوع (Type) موجود **بدون تعديل كوده** وبدون وراثة منه. تُستدعى كأنها دالة أصلية في النوع.

**لماذا يوجد؟** أحيانًا نريد إضافة دالة إلى نوع:
- **لا نملك كوده**: مثل `string` أو `IServiceCollection`، وهي جزء من .NET.
- **لا يمكن الوراثة منه**: مثل `string` (sealed).
- أو نريد كتابة الاستدعاء بشكل أنظف وقابل للتسلسل (chaining).

## 3. القواعد (ثلاث فقط)

1. الدالة داخل **static class**.
2. الدالة نفسها **static**.
3. المعامل الأول يسبقه **`this`**، وهو النوع الذي نمدّده.

```csharp
public static class StringExtensions          // 1) static class
{
    public static string Truncate(this string text, int maxLength)   // 2) static  3) this
    {
        if (text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }
}
```

## 4. تجربة 1: على `string`

```csharp
string title = "ASP.NET Core extension methods";
Console.WriteLine(title.Truncate(10));      // ASP.NET Co...
```
- لاحظ أننا استدعيناها `title.Truncate(10)` ولم نمرر `title` كمعامل.
- المعامل `maxLength` هو الوحيد المكتوب بين القوسين، لأن `this string text` هو الكائن الذي نستدعي عليه.

## 5. كيف يعمل تحت الغطاء؟

- المترجم (Compiler) يحوّل `title.Truncate(10)` إلى `StringExtensions.Truncate(title, 10)`. أي أنها دالة static عادية، والـ `this` مجرد سكر لغوي (Syntactic sugar).
- لذلك:
  - **لا تصل إلى الأعضاء الخاصة (private)** في النوع، فهي ليست جزءًا منه.
  - لو وُجدت دالة **أصلية** بنفس الاسم والمعاملات في النوع، **فالأصلية هي التي تُنفَّذ** وتتجاهل الـ Extension.
  - يلزم استيراد الـ namespace (`using`) الذي فيه الكلاس لتظهر الدالة.

## 6. أمثلة تعرفها بالفعل

- **LINQ كلها Extension Methods:** `Where` و`Select` و`ToList` و`FirstOrDefault` ممدّدة على `IEnumerable<T>`.
- **ASP.NET Core:**

| الدالة | نوعها الممدَّد | ما يعنيه الاسم |
|---|---|---|
| `builder.Services.AddControllers()` | `IServiceCollection` | `Add...` تسجيل خدمة في الـ IoC Container |
| `app.UseHttpsRedirection()` | `IApplicationBuilder` | `Use...` إضافة Middleware إلى الـ Pipeline |
| `app.MapGet(...)` / `app.MapControllers()` | `IEndpointRouteBuilder` | `Map...` تسجيل Endpoint |

- هذا هو عُرف التسمية في ASP.NET Core، ونلتزم به في إضافاتنا.

## 7. تجربة 2: Extension لتسجيل الخدمات (`Add...`)

**المشكلة:** كلما كثرت الخدمات امتلأ `Program.cs` بأسطر التسجيل.

**الحل:** نجمعها في دالة واحدة:
```csharp
namespace ProjectStructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGreeting(this IServiceCollection services)
    {
        services.AddSingleton<IGreeter, Greeter>();
        return services;
    }
}
```
وفي `Program.cs`:
```csharp
using ProjectStructure.Extensions;

builder.Services.AddGreeting();
```
- ترجع `IServiceCollection` نفسه (`return services`) حتى يمكن **تسلسل** الاستدعاءات: `services.AddA().AddB()`.

## 8. تجربة 3: Extension للـ Middleware (`Use...`)

نلفّ Middleware بدالة واضحة:
```csharp
namespace ProjectStructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRequestLog(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
            await next();
        });
    }
}
```
وفي `Program.cs`:
```csharp
app.UseRequestLog();
```
- الآن يقرأ أي شخص `Program.cs` فيفهم الـ Pipeline من الأسماء فقط، بدل أن يرى الكود كله.
- جرّب `http://localhost:5149/` وستظهر `Request: GET /` في الـ Console.

## 9. أين نضع الملفات؟

- مجلد اسمه `Extensions/` داخل المشروع.
- اسم الملف والكلاس ينتهيان بـ `Extensions`: مثل `StringExtensions.cs` و`ServiceCollectionExtensions.cs`.
- ملف لكل نوع ممدَّد، فتعرف أين تبحث.

```
ProjectStructure/
├── Extensions/
│   ├── StringExtensions.cs
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
└── Program.cs
```
- نصيحة: لو وضعت الـ namespace `Microsoft.Extensions.DependencyInjection` لدوال `Add...` تظهر مباشرة بدون `using`، وهذا ما تفعله مكتبات Microsoft. لكن للمبتدئ الأفضل أن يبقى الـ namespace الخاص بمشروعك فتعرف مصدر الدالة.

## 10. متى نستخدمها؟ ومتى نتجنبها؟

**نستخدمها عندما:**
- نريد تنظيم `Program.cs` وتقصيره.
- نريد إضافة دالة مفيدة لنوع لا نملكه.
- نريد استدعاءات مترابطة (chaining).

**نتجنبها عندما:**
- يكون النوع لنا، فنضيف الدالة فيه مباشرة.
- نكثر منها حتى يصعب على الآخرين اكتشاف من أين جاءت الدالة.

## 11. الملخص والواجب

| المفهوم | ما هو |
|---|---|
| **Extension Method** | دالة تُضاف لنوع موجود بدون تعديل كوده |
| **القواعد** | `static class` + `static` method + `this` على أول معامل |
| **تحت الغطاء** | دالة static عادية يحوّلها المترجم |
| **في ASP.NET Core** | `Add...` للخدمات، و`Use...` للـ Middleware، و`Map...` للـ Endpoints |

واجب:
1. اكتب Extension على `string` باسم `IsEmail` يتحقق أن النص يحتوي `@` ونقطة.
2. اكتب Middleware يقيس زمن الطلب، ثم لفّه بدالة `UseRequestTiming()`.
3. جرّب أن تحذف سطر `using` وانظر الخطأ.
