# الدرس: Host Model و HttpContext

---

## 1. المقدمة
عندما نكتب `app.Run()`، من الذي يستقبل الطلبات؟ وعندما يصل طلب، أين تُحفظ بياناته (الـ Headers والـ Query والـ Body)؟ جواب السؤال الأول هو الـ Host، وجواب الثاني هو الـ HttpContext.

---

# الجزء الأول: Host Model

## 2. ما هو الـ Host؟

**ما هو؟** الـ Host هو الكائن الذي **يحتضن التطبيق كله ويدير دورة حياته**: يبدأ تشغيله، ويبقيه يعمل، ويوقفه بشكل سليم.

**لماذا يوجد؟** التطبيق ليس مجرد كود يرد على طلبات. هو يحتاج خادم ويب يستقبل الاتصالات، وإعدادات، وLogging، وحاوية خدمات (IoC Container)، وطريقة لإيقافه بدون قطع الطلبات الجارية. كل هذا يُجمع ويُدار في مكان واحد هو الـ Host.

**من ينشئه ومتى؟** نحن لا ننشئه بأيدينا:
```csharp
var builder = WebApplication.CreateBuilder(args);   // نجهّز الـ Host
var app = builder.Build();                          // ينشأ الـ Host (app)
app.Run();                                          // يبدأ ويبقى يعمل
```
- `app` من نوع `WebApplication`، وهو الـ Host نفسه.
- هذا الشكل يسمى **Minimal Hosting Model** (منذ .NET 6). قبله كان هناك ملفان: `Program.cs` و`Startup.cs`، وكان الـ Host يُنشأ بـ `Host.CreateDefaultBuilder` ثم `ConfigureWebHostDefaults`. الفكرة واحدة والكتابة اختصرت.

## 3. ماذا يحتضن الـ Host؟

عند `CreateBuilder` يجهّز لنا الـ Host هذه الأشياء تلقائيًا (لهذا لم نكتب أي كود لها في `Program.cs`):

| الشيء | ما الذي يجهّزه |
|---|---|
| **خادم الويب (Web Server)** | Kestrel، وهو الخادم الافتراضي |
| **الإعدادات (Configuration)** | `appsettings.json` ثم `appsettings.{Environment}.json` ثم User Secrets (في Development) ثم Environment Variables ثم Command-line args |
| **الـ Logging** | مزوّدات جاهزة (Console وDebug وغيرها) |
| **الـ IoC Container** | الحاوية التي نسجّل فيها الخدمات عبر `builder.Services` |
| **البيئة (Environment)** | `Development` أو `Production` من `ASPNETCORE_ENVIRONMENT` |

وهذا هو سبب أن `appsettings.json` و`launchSettings.json` يعملان بدون أن نقرأهما بالكود.

## 4. خادم الويب: Kestrel

**ما هو؟** برنامج يفتح منفذًا على الشبكة (مثل 5149) ويستقبل طلبات HTTP، ثم يمررها إلى تطبيقنا.

**لماذا يوجد؟** التطبيق لا يتعامل مع الشبكة مباشرة. Kestrel يتولى قراءة الطلب الخام، ثم يحوّله إلى **HttpContext** (الجزء الثاني من الدرس) ويدخله إلى الـ Pipeline.

**أين يُضبط؟** المنفذ يأتي من `applicationUrl` في `launchSettings.json` أثناء التطوير. والـ Kestrel مدمج في التطبيق نفسه، ولهذا نشغّل التطبيق بـ `dotnet run` بدون تثبيت أي خادم.

**في الإنتاج:** يوضع غالبًا أمامه خادم آخر يسمى **Reverse Proxy** (مثل IIS أو Nginx) يستقبل الطلبات من الإنترنت ويمررها إلى Kestrel.
- على IIS يوجد وضعان: **In-process** (التطبيق يعمل داخل عملية IIS نفسها، وهو الافتراضي) و**Out-of-process** (IIS يمرر الطلبات إلى Kestrel يعمل في عملية منفصلة).
- يكفي أن تعرف أن Kestrel هو من يستقبل الطلب في كل الأحوال.

## 5. دورة حياة الـ Host (Lifetime)

**ما هي؟** ثلاث مراحل يمر بها التطبيق: يبدأ ← يعمل ← يتوقف.

**لماذا تهمنا؟** عند الإيقاف (Ctrl+C مثلًا) لا يُقطع التطبيق فجأة، بل يتوقف عن استقبال طلبات جديدة وينهي الجارية (Graceful Shutdown). ويمكننا تنفيذ كود في كل مرحلة، مثل تنظيف موارد.

**الكود:** الـ Host يعطينا الخاصية `Lifetime`:
```csharp
var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() => Console.WriteLine("Host: started"));
app.Lifetime.ApplicationStopping.Register(() => Console.WriteLine("Host: stopping"));
app.Lifetime.ApplicationStopped.Register(() => Console.WriteLine("Host: stopped"));
```

**تجربة:** شغّل التطبيق وستظهر `Host: started` ثم اضغط Ctrl+C وستظهر `Host: stopping` و`Host: stopped`.

**ما هي `app.Run()` إذًا؟** تبدأ الـ Host وتحجز الـ Thread إلى أن يصل أمر الإيقاف. لهذا لا يُنفَّذ أي كود بعدها.

---

# الجزء الثاني: HttpContext

## 6. ما هو الـ HttpContext؟

**ما هو؟** كائن يمثل **طلبًا واحدًا بكل تفاصيله وردّه**. يحمل كل ما يخص هذا الطلب: بياناته القادمة (Request) والرد الذي سنرسله (Response) والمستخدم وغير ذلك.

**لماذا يوجد؟** بدل أن نمرر عشرات المعاملات من مكان إلى آخر، يُمرَّر كائن واحد يحتوي على كل شيء. وهو أيضًا الوسيلة التي تتشارك بها أجزاء التطبيق (الـ Middlewares) معلومات عن الطلب نفسه.

**من ينشئه ومتى؟**
- ينشئه **Kestrel** عند وصول كل طلب، فيكون لكل طلب `HttpContext` جديد.
- يمر عبر الـ Pipeline كله، ثم **يُتخلص منه** عند انتهاء الطلب.
- لا يعيش أكثر من عمر الطلب.

**كيف نصل إليه؟**
| المكان | الطريقة |
|---|---|
| Minimal API | معامل في الدالة: `(HttpContext context) => ...` |
| Middleware | معامل `context` في `app.Use(async (context, next) => ...)` |
| Controller | خاصية جاهزة اسمها `HttpContext` |
| كلاس عادي | الواجهة `IHttpContextAccessor` (بعد تسجيلها بـ `AddHttpContextAccessor()`) |

## 7. ماذا بداخل الـ HttpContext؟

| الخاصية | ما هي | أمثلة |
|---|---|---|
| `Request` | الطلب القادم | `Method`, `Path`, `Query`, `Headers`, `Body`, `Cookies` |
| `Response` | الرد الذي سنرسله | `StatusCode`, `Headers`, `ContentType`, `Body` |
| `User` | المستخدم الحالي (`ClaimsPrincipal`) | يُستخدم في الـ Authentication |
| `Items` | قاموس لمشاركة بيانات بين الـ Middlewares في نفس الطلب | `context.Items["key"]` |
| `RequestServices` | للحصول على الخدمات من الـ IoC Container | الـ Dependency Injection |
| `Connection` | معلومات الاتصال | `RemoteIpAddress` |
| `TraceIdentifier` | رقم تعريف فريد للطلب | مفيد في الـ Logging |
| `RequestAborted` | `CancellationToken` يُلغى إذا قطع العميل الاتصال | |

## 8. تجربة 1: قراءة الـ Request

```csharp
app.MapGet("/info", (HttpContext context) => new
{
    context.Request.Method,
    Path = context.Request.Path.Value,
    Query = context.Request.QueryString.Value,
    UserAgent = context.Request.Headers.UserAgent.ToString(),
    Ip = context.Connection.RemoteIpAddress?.ToString(),
    context.TraceIdentifier
});
```
جرّب `http://localhost:5149/info?x=1`، والنتيجة JSON مثل:
```json
{"method":"GET","path":"/info","query":"?x=1","userAgent":"...","ip":"::1","traceIdentifier":"0HN...:00000001"}
```
- `Method` هو الـ HTTP Verb (GET).
- غيّر الرابط وأعد المحاولة لترى كيف تتغير القيم، وأعد الطلب لترى `TraceIdentifier` يتغير لأن لكل طلب HttpContext جديد.

## 9. تجربة 2: كتابة الـ Response

```csharp
app.MapGet("/write", async (HttpContext context) =>
{
    context.Response.StatusCode = 201;
    context.Response.Headers["X-Course"] = "ASP.NET Core";
    context.Response.ContentType = "text/plain; charset=utf-8";
    await context.Response.WriteAsync("Written directly to the response");
});
```
- افتح `http://localhost:5149/write` ثم افتح تبويب Network في المتصفح وانظر: الـ Status Code هو `201 Created`، والـ Header `X-Course` موجود.
- عادة لا نكتب في الـ Response يدويًا هكذا. عندما نكتب `return "Hello"` أو نرجع كائنًا، ASP.NET Core يكتب لنا الـ Response. لكن هذا هو ما يحدث تحت الغطاء.
- ترتيب مهم: الـ Headers وStatus Code يجب ضبطهما **قبل** كتابة الـ Body، لأن بعد بدء الكتابة يُرسَل الرأس ولا يمكن تغييره.

## 10. تنبيهات مهمة

- الـ HttpContext **خاص بطلب واحد**، فلا تخزّنه في متغير ثابت (`static`) أو تستخدمه بعد انتهاء الطلب.
- لا يُستخدم من عدة Threads في نفس الوقت (غير آمن للـ Threads).
- في كلاس عادي استخدم `IHttpContextAccessor` ولا تحاول تمريره يدويًا في كل مكان.

---

## 11. الملخص والواجب

| المفهوم | ما هو | من ينشئه |
|---|---|---|
| **Host** (`WebApplication`) | يحتضن التطبيق ويدير دورة حياته | `builder.Build()` |
| **Kestrel** | خادم الويب الذي يستقبل الطلبات | الـ Host |
| **HttpContext** | كل ما يخص طلبًا واحدًا وردّه | Kestrel عند كل طلب |

واجب:
1. أضف route باسم `/headers` يرجع كل الـ Headers القادمة في الطلب.
2. أضف route يرجع الـ `TraceIdentifier` وجرّبه أكثر من مرة، ما الذي تلاحظه؟
3. اطبع رسالة عند `ApplicationStopping`، ثم اضغط Ctrl+C وتأكد أنها تظهر.
