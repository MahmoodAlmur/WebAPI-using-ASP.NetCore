# الدرس 4: Middleware و Request Pipeline

---

## 1. المقدمة
كل سطر مثل `app.UseHttpsRedirection()` و`app.UseAuthorization()` في `Program.cs` هو Middleware، وترتيبها مهم. وفهم الـ Pipeline يعني فهم 80% من طريقة عمل ASP.NET Core.

## 2. ما هو الـ Middleware؟

**ما هو؟** قطعة كود صغيرة تقف في طريق الطلب، تستقبل الـ `HttpContext`، وتقرر: تفعل شيئًا ثم تمرّر الطلب للتي بعدها، أو توقف الطلب وترد بنفسها.

**لماذا يوجد؟** هناك أعمال تتكرر مع **كل** طلب، مثل: تحويل http إلى https، ومعالجة الأخطاء، والتحقق من المستخدم، وتسجيل الطلبات. لو كتبناها داخل كل دالة لتكرر الكود في كل مكان. الـ Middleware يجمع كل عمل منها في مكان واحد ويطبّقه على الجميع.

**من ينشئه ومتى؟** نحن نضيفه في `Program.cs` **بعد `Build()` وقبل `Run()`** بالصيغة `app.UseXxx()`. والـ Host يبني منها الـ Pipeline عند التشغيل.

## 3. ما هو الـ Request Pipeline؟

**ما هو؟** سلسلة مرتبة من الـ Middlewares، يمر عبرها كل طلب واحدًا بعد الآخر.

**ترتيب العمل:** الترتيب هو ترتيب كتابتها في `Program.cs`، وللطلب اتجاهان:
```
الطلب  ──►  Middleware 1  ──►  Middleware 2  ──►  Middleware 3  ──►  الكود الذي يرد (Endpoint)
الرد   ◄──  Middleware 1  ◄──  Middleware 2  ◄──  Middleware 3  ◄──
```
- ينزل الطلب إلى الأعماق، ثم يصعد الرد بنفس السلسلة بالعكس (يشبه طبقات البصلة).
- كل Middleware يعمل **مرتين**: مرة قبل تمرير الطلب، ومرة بعد رجوع الرد.

## 4. تجربة 1: أول Middleware بأيدينا

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine("A: before next");
    await next();
    Console.WriteLine("A: after next");
});
```
- `context`: هو الـ HttpContext، الكائن الذي يمثل الطلب الحالي والرد.
- `next`: الدالة التي تمرّر الطلب إلى الـ Middleware التالي.
- ما يُكتب **قبل** `await next()` يعمل في اتجاه الطلب، وما يُكتب **بعده** يعمل في اتجاه الرد.

أضف Middleware ثانيًا بنفس الشكل واسمه `B`، وبعدهما:
```csharp
app.MapGet("/", () => "Hello World!");
```
افتح `http://localhost:5149/` وانظر الـ Console:
```
A: before next
B: before next
B: after next
A: after next
```
هذا هو "الدخول بالترتيب والخروج بالعكس". بدّل مكان A وB وانظر كيف يتغير الناتج، وهنا نفهم أن **الترتيب مهم**.

## 5. تجربة 2: إيقاف الطلب (Short-circuit)

**ما هو؟** أن يرد الـ Middleware بنفسه **بدون** استدعاء `next()`، فلا يصل الطلب لما بعده.

**لماذا؟** مثل حارس البوابة: إن لم يستوفِ الطلب الشرط يُرفض هنا ولا يصل إلى كودك.

```csharp
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/secure") &&
        !context.Request.Headers.ContainsKey("X-Course"))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Missing X-Course header");
        return;                       // لا نستدعي next()
    }
    await next();
});

app.MapGet("/secure", () => "Secure area");
```
- ارسل الطلب بدون الـ Header: `400 Bad Request` ونص الرسالة.
- ارسل الطلب مع الـ Header `X-Course: 1` فيظهر `Secure area`.
- ملاحظة: الـ `return` بدون `next()` هو سر إيقاف الطلب.

## 6. الفرق بين `Use` و`Run` و`Map`

| الأمر | ما يفعله | هل يمرّر للتالي؟ |
|---|---|---|
| `app.Use(...)` | Middleware عادي، يستقبل `next` | نعم، إن استدعينا `next()` |
| `app.Run(...)` | Middleware **نهائي**، لا يستقبل `next` | لا، وأي شيء بعده لا يُنفَّذ أبدًا |
| `app.Map("/path", ...)` | يفرّع الـ Pipeline: طلبات هذا المسار تذهب لفرع مستقل | الفرع له سلسلته الخاصة |

مثال `Map`:
```csharp
app.Map("/admin", adminApp =>
{
    adminApp.Run(async context => await context.Response.WriteAsync("Admin branch"));
});
```
افتح `/admin` ترى `Admin branch`.

ملاحظة: الـ `MapGet` الذي استخدمناه من البداية هو أيضًا نهاية للسلسلة، فهو الذي يرد على الطلب.

## 7. تجربة 3: Middleware في كلاس مستقل

**لماذا؟** عندما يكبر الكود لا يصح أن نكتبه كله داخل `Program.cs`. الكلاس يُرتّب الكود ويمكن إعادة استخدامه واختباره.

**الشكل:** كلاس فيه شيئان: Constructor يستقبل `RequestDelegate`، ودالة `InvokeAsync` تستقبل `HttpContext`.
```csharp
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTimingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        await _next(context);
        watch.Stop();
        Console.WriteLine($"{context.Request.Method} {context.Request.Path} took {watch.ElapsedMilliseconds} ms");
    }
}
```
وفي `Program.cs`:
```csharp
app.UseMiddleware<RequestTimingMiddleware>();
```
- `_next` هو الدالة التي تمرّر الطلب إلى الـ Middleware التالي.
- الكلاس يوضع **أسفل** `Program.cs` (بعد `app.Run()`) أو في ملف مستقل.
- النتيجة في الـ Console: `GET / took 6 ms`.
- هذا يعطيك Middleware يقيس زمن كل طلب، وهو مفيد جدًا في الـ Logging.

## 8. الـ Middlewares الجاهزة وترتيبها المعتاد

ASP.NET Core يأتي بمجموعة جاهزة، وهذا هو الترتيب الموصى به:

| الترتيب | الـ Middleware | وظيفته |
|---|---|---|
| 1 | `UseExceptionHandler` | يلتقط الأخطاء من كل ما بعده |
| 2 | `UseHttpsRedirection` | تحويل http إلى https |
| 3 | `UseStaticFiles` | تقديم الملفات الثابتة (صور وCSS) |
| 4 | `UseRouting` | اختيار الـ Endpoint المناسب للطلب |
| 5 | `UseCors` | السماح للمواقع الأخرى بالاتصال |
| 6 | `UseAuthentication` | من أنت؟ |
| 7 | `UseAuthorization` | هل تملك الصلاحية؟ |
| 8 | الـ Endpoints (`MapGet` / `MapControllers`) | الكود الذي يرد على الطلب |

لماذا هذا الترتيب؟ أمثلة:
- `UseExceptionHandler` في الأول لأنه يلتقط أخطاء كل ما بعده. لو وضعته في الآخر لا يلتقط شيئًا.
- `UseAuthentication` قبل `UseAuthorization` لأن لا يمكن التحقق من صلاحية شخص لم نعرفه بعد.
- `UseAuthorization` قبل الـ Endpoints لأن الصلاحية تُفحص قبل تنفيذ الكود.

ملاحظة: في الشكل الحديث (Minimal Hosting) الـ `UseRouting` يُضاف تلقائيًا ولا نكتبه بأنفسنا.

## 9. نقاط مهمة

- في `Program.cs`: كل سطر `app.Use...` هو Middleware، وترتيبها هو ترتيب الـ Pipeline.
- الـ Middleware يستقبل الـ `HttpContext`، وقد استخدمنا `context.Request` و`context.Response` في الأمثلة.
- الـ `Items` في الـ HttpContext وسيلة لتمرير بيانات من Middleware إلى ما بعده.

## 10. الملخص والواجب

| المفهوم | ما هو |
|---|---|
| **Middleware** | قطعة كود في طريق الطلب، تمرّره أو توقفه |
| **Pipeline** | سلسلة الـ Middlewares، تدخل بالترتيب وتخرج بالعكس |
| **`next()`** | تمرير الطلب للتالي، وعدم استدعائه = إيقاف الطلب (Short-circuit) |
| **`Use` / `Run` / `Map`** | عادي / نهائي / تفريع |
| **الترتيب** | مهم، فهو يحدد ما يحدث لكل طلب |

واجب:
1. اكتب Middleware يضيف Header اسمه `X-Request-Id` للرد.
2. اجعل الـ Middleware الذي يمنع الطلب بدون Header يعمل على كل المسارات وليس `/secure` فقط، ثم جرّب `/`.
3. بدّل ترتيب A وB وسجّل ما يتغير.
