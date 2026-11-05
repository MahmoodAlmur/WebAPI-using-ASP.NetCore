# الدرس: ASP.NET Core Project Structure (بنية المشروع)

---

## 1. المقدمة
عند فتح مشروع ASP.NET Core لأول مرة قد لا تعرف لماذا كل ملف موجود. نبدأ بأصغر مشروع ممكن، ونفهم دور كل ملف فيه، ومن أين يبدأ تنفيذ البرنامج.

## 2. لماذا القالب الفارغ؟
```powershell
dotnet new web -n ProjectStructure
cd ProjectStructure
code .
```
- `dotnet new` ينشئ مشروعًا من قالب (template) جاهز، و`web` هو أصغر قالب ويب: مشروع فارغ يرد على الطلبات برسالة Hello World.
- القوالب الأخرى (مثل `webapi`) تأتي بملفات كثيرة لا نعرف دورها بعد.
- نبدأ فارغين لنفهم كل ملف في المشروع.

## 3. الصورة الكبيرة
```
ProjectStructure/
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── ProjectStructure.csproj
```
5 ملفات فقط، ويمكن تقسيمها إلى 3 مجموعات:

| المجموعة | الملفات | سؤالها |
|---|---|---|
| تعريف المشروع | `ProjectStructure.csproj` | ما هذا المشروع وماذا يحتاج؟ |
| الكود | `Program.cs` | ماذا يفعل التطبيق؟ |
| الإعدادات والتشغيل | `appsettings*.json`، `launchSettings.json` | كيف يُضبط ويُشغَّل؟ |

مجلدا `bin/` و`obj/` يظهران بعد أول بناء (build)، وهما ناتج الـ build ولا نعدّل فيهما.

---

## 4. شرح كل ملف

### 4.1 `ProjectStructure.csproj`

**ما هو؟** ملف تعريف المشروع، أي بطاقة هوية المشروع. امتداده `csproj` أي C# Project، وهو بصيغة XML.

**لماذا يوجد؟** كلمة "مشروع" في .NET تعني هذا الملف. بدونه لا يعرف الأمر `dotnet` أن هذا المجلد مشروع. كل مشروع له ملف `.csproj` واحد.

**من يقرؤه ومتى؟** أداة `dotnet` (المترجم) عند `dotnet build` و`dotnet run`، وكذلك المحرر عند فتح المشروع. هو لا يعمل مع التطبيق، بل يُستخدم لبنائه فقط.

**ماذا بداخله؟**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```
- `Sdk="Microsoft.NET.Sdk.Web"`: نوع المشروع. هذا السطر هو ما يجعله مشروع **ويب**، فتتوفر لنا مكتبات ASP.NET Core بدون إضافتها بأنفسنا. لو كان `Microsoft.NET.Sdk` لكان برنامج Console عاديًا.
- `TargetFramework`: إصدار .NET الذي سيعمل عليه.
- `Nullable`: تحذيرات تحميك من أخطاء `null`.
- `ImplicitUsings`: لا تكتب `using` الشائعة في كل ملف. لهذا لا نرى أي `using` في `Program.cs`.
- لاحظ أنه لا توجد حزم (packages) لأن المشروع بسيط. وعندما نضيف حزمة (مثل Entity Framework) سيظهر هنا `PackageReference`.

**هل أعدّل عليه؟** نادرًا يدويًا. عند `dotnet add package` يعدّله الأمر نيابة عنك.

---

### 4.2 `Program.cs`: أهم ملف

**ما هو؟** نقطة بداية التطبيق (Entry Point). أول سطر يُنفَّذ عند تشغيل البرنامج هو أول سطر في هذا الملف.

**لماذا يوجد؟** أي برنامج يحتاج نقطة بداية. في المشاريع القديمة كان هناك دالة `Main` وملف `Startup.cs`. الآن اجتمع كل شيء في ملف واحد بفضل Top-level statements في C#، فلا نكتب `Main`.

**من يقرؤه ومتى؟** يُنفَّذ مرة واحدة عند تشغيل التطبيق ليبنيه، ثم يبقى التطبيق يعمل (`app.Run()`) وينتظر الطلبات.

**ماذا بداخله؟** 4 أسطر فقط:
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```
نشرحها سطرًا سطرًا:

| السطر | ماذا يفعل |
|---|---|
| `WebApplication.CreateBuilder(args)` | ينشئ **builder**: أداة تجهّز التطبيق. تقرأ الإعدادات (`appsettings.json`) وتجهّز الـ Logging وخادم الويب Kestrel. `args` هي معاملات سطر الأوامر. |
| `builder.Build()` | يحوّل ما جهّزه الـ builder إلى تطبيق (`app`) جاهز للعمل. |
| `app.MapGet("/", ...)` | يقول: عندما يصل طلب **GET** على المسار **`/`** نفّذ هذه الدالة وأرجع نتيجتها. (GET هو الـ HTTP Verb.) |
| `app.Run()` | يشغّل الخادم ويبقى ينتظر الطلبات إلى أن توقفه. بعده لا يُنفَّذ أي كود. |

- ثلاث مراحل: **إعداد** (builder) ← **بناء** (Build) ← **تشغيل** (Run). والـ `MapGet` يتم بين البناء والتشغيل.
- كل ما سنضيفه من الآن سيدخل في هذا الملف، ويُكتب في مكانين حسب نوعه:

#### أ) بعد `CreateBuilder` وقبل `Build()`: إعدادات المشروع وتسجيل الخدمات في الـ IoC Container
الـ `builder` (من نوع `WebApplicationBuilder`) هو المكان الذي **نكوّن فيه المشروع** قبل أن يُبنى. وفيه شيئان:
1. **الإعدادات وتكوين المشروع:** عبر `builder.Configuration` و`builder.Logging` و`builder.Environment`.
2. **تسجيل الـ Services داخل الـ IoC Container:** عبر `builder.Services`. الـ IoC Container هو الحاوية التي تحتفظ بالخدمات (كلاسات يحتاجها التطبيق)، وتسلّمها لأي كلاس يطلبها. الصيغة دائمًا `builder.Services.AddXxx()`.
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();                  // خدمات الـ Controllers
builder.Services.AddDbContext<AppDbContext>(...);   // قاعدة البيانات
builder.Services.AddScoped<IProductService, ProductService>();  // خدماتنا
builder.Services.AddAuthentication(...);            // تسجيل الدخول
builder.Services.AddOpenApi();                      // توثيق الـ API

var app = builder.Build();
```
- أمثلة أخرى: `AddCors`، `AddMemoryCache`، `AddHttpClient`، `AddAuthorization`.
- **عند `Build()`** يُبنى التطبيق (`app`) من كل ما سبق، ويُقفل الـ IoC Container، فلا يمكن إضافة خدمات جديدة بعده. هذا سبب وجود الفاصل بين المرحلتين.

#### ب) بعد `Build()` وقبل `Run()`: تحديد طريقة التعامل مع الـ HTTP Request عبر الـ Middlewares
بعد بناء التطبيق نحدد **كيف يتعامل مع كل HTTP Request** يصل إليه. نفعل ذلك بترتيب سلسلة من الـ **Middlewares**، وهي خطوات يمرّ بها الطلب واحدة بعد الأخرى إلى أن يصل إلى الكود الذي يرد عليه (الـ route). الصيغة `app.UseXxx()` للـ middleware، و`app.MapXxx()` لتسجيل الـ routes.
```csharp
var app = builder.Build();

app.UseHttpsRedirection();       // middleware: تحويل http إلى https
app.UseAuthentication();         // middleware: من أنت؟
app.UseAuthorization();          // middleware: هل تملك الصلاحية؟

app.MapGet("/", () => "Hello World!");   // route
app.MapControllers();                    // routes الـ Controllers

app.Run();
```
- `Use...`: خطوات تمرّ بها كل الطلبات **بالترتيب الذي كُتبت به**، ولهذا الترتيب مهم.
- `Map...`: ربط مسار معيّن بكود ينفّذه، وهو ما يرد على الطلب فعلًا.
- أمثلة أخرى: `UseCors`، `UseStaticFiles`، `UseExceptionHandler`.

| قبل `Build()` (على الـ builder) | بعد `Build()` وقبل `Run()` (على الـ app) |
|---|---|
| `builder.Configuration` / `builder.Logging` | `app.Use...` (Middlewares) |
| `builder.Services.Add...` (الـ IoC Container) | `app.Map...` (Routes) |
| تكوين المشروع وتسجيل الخدمات | تحديد طريقة التعامل مع الـ HTTP Request |
| يُنفَّذ مرة واحدة عند البداية | يمرّ عليه كل طلب |

**هل أعدّل عليه؟** نعم، دائمًا. هنا نبني التطبيق.

**تجربة:** أضف سطرًا:
```csharp
app.MapGet("/hello/{name}", (string name) => $"Hello {name}!");
```
شغّل وجرّب `/hello/Ahmed`.

---

### 4.3 `appsettings.json` و`appsettings.Development.json`

**ما هما؟** ملفا إعدادات التطبيق بصيغة JSON.

**لماذا يوجدان؟** لفصل الإعدادات المتغيرة عن الكود، فتغيّر الإعداد (مثل عنوان قاعدة البيانات) بدون أن تعيد ترجمة الكود.

**من يقرؤهما ومتى؟** يقرؤهما `CreateBuilder` تلقائيًا عند بداية التشغيل (السطر الأول في `Program.cs`، ولهذا لم نكتب أي كود لقراءتهما).
- `appsettings.json` يُقرأ دائمًا.
- `appsettings.Development.json` يُقرأ فوقه فقط حين تكون البيئة `Development`، ويتغلب على القيم المتكررة. وبنفس القاعدة يوجد `appsettings.Production.json` عند النشر.

**ماذا بداخلهما؟**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
- `Logging`: كم من الرسائل يكتب التطبيق في الـ log. `Information` تعرض معلومات عامة، و`Warning` تعرض التحذيرات فقط (لـ ASP.NET Core نفسه حتى لا يكثر الضجيج).
- `AllowedHosts`: الأسماء المسموح بها للوصول إلى التطبيق، و`*` تعني الكل.
- في ملف `Development` الجزء الخاص بـ `AllowedHosts` غير موجود، فيؤخذ من الأساسي. وهذا يوضح فكرة "الدمج".
- يمكن إضافة `ConnectionStrings` وإعدادات JWT هنا.

**تجربة:** غيّر `"Default": "Information"` إلى `"Warning"` في `appsettings.Development.json` وانظر كيف تقل الرسائل في الـ Terminal.

**هل أعدّل عليهما؟** نعم. تنبيه: لا تضع كلمات سر حقيقية هنا لأنها تُرفع مع الكود.

---

### 4.4 `Properties/launchSettings.json`

**ما هو؟** ملف يحدد كيف يُشغَّل المشروع على جهازك أثناء التطوير.

**لماذا يوجد؟** لتحديد المنفذ والبيئة بدون أن تكتبهما في كل مرة.

**من يقرؤه ومتى؟** أداة `dotnet run` ومحرر الكود (Visual Studio / VS Code) عند الضغط على Run. **لا يُنشر مع التطبيق ولا يقرؤه التطبيق في الإنتاج.**

**ماذا بداخله؟**
```json
"http": {
  "commandName": "Project",
  "launchBrowser": true,
  "applicationUrl": "http://localhost:5149",
  "environmentVariables": { "ASPNETCORE_ENVIRONMENT": "Development" }
}
```
- Profile لكل طريقة تشغيل (`http` و`https`). كل واحد منها له اسم، وتختاره بـ `--launch-profile`.
- `applicationUrl`: العنوان والمنفذ (المنفذ يختلف من جهاز لآخر، لأنه يُولَّد عشوائيًا عند إنشاء المشروع).
- `launchBrowser`: يفتح المتصفح تلقائيًا عند التشغيل.
- `ASPNETCORE_ENVIRONMENT`: البيئة، وهي التي تحدد أي `appsettings.*.json` يُقرأ. هذا هو الرابط بين هذا الملف وملفات الإعدادات.

**هل أعدّل عليه؟** أحيانًا، لتغيير المنفذ مثلًا.

---

### 4.5 الـ Dev Certificate (شهادة HTTPS للتطوير)

**لماذا نتكلم عنها هنا؟** لأن في `launchSettings.json` profile اسمه `https` يستخدم العنوان `https://localhost:7123`. وHTTPS يحتاج **شهادة (Certificate)**، فمن أين تأتي على جهازك؟

**ما هي؟** شهادة HTTPS موقّعة ذاتيًا (Self-signed) تُنشأ لجهاز التطوير فقط، ليعمل `https://localhost` بدون شراء شهادة حقيقية.

**لماذا تحتاج إلى الثقة بها (trust)؟** المتصفح يثق فقط بالشهادات الصادرة عن جهات معروفة (Certificate Authorities). والشهادة الموقّعة ذاتيًا ليست منها، فيعرض لك تحذير "اتصالك ليس خاصًا". والحل أن تجعل جهازك يثق بها.

**من ينشئها ومتى؟** تُنشأ مع تثبيت .NET SDK أو عند أول تشغيل. وتُحفظ في مخزن الشهادات الخاص بالمستخدم على جهازك، وليست جزءًا من المشروع، ولا تظهر في ملفاته، ولا تُرفع إلى GitHub.

**الأوامر:**
```powershell
dotnet dev-certs https --check    # هل الشهادة موجودة؟ وهل أثق بها؟
dotnet dev-certs https --trust    # إنشاء الشهادة (إن لم توجد) وجعل جهازك يثق بها
dotnet dev-certs https --clean    # حذف الشهادة (ثم أعد --trust لإنشاء واحدة جديدة)
```
- عند `--trust` على Windows تظهر نافذة تسألك إن كنت توافق، اضغط Yes.
- على Linux لا يدعم الأمر `--trust` بشكل كامل، ويلزم إعداد يدوي حسب التوزيعة.

**تجربة:**
```powershell
dotnet run --launch-profile https
```
- افتح `https://localhost:7123` (استخدم المنفذ الموجود عندك).
- إن لم تكن الشهادة موثوقة فسيظهر تحذير في المتصفح. شغّل `dotnet dev-certs https --trust` ثم أعد فتح المتصفح وسيختفي التحذير.

**تنبيهات:**
- شهادة التطوير **للتطوير فقط**. في الإنتاج تستخدم شهادة حقيقية (مثل Let's Encrypt أو من مزود الاستضافة).
- هذه الشهادة هي ما يستخدمه `UseHttpsRedirection` عندما يحوّل http إلى https.

---

## 5. التشغيل والتجربة
```powershell
dotnet run --launch-profile http
```
- افتح `http://localhost:5149` (استخدم المنفذ الموجود عندك) ← ترى `Hello World!`.
- اربط ما حدث بالملفات: `launchSettings` حدد المنفذ والبيئة ← `.csproj` عرّف أنه مشروع ويب ← `Program.cs` بنى التطبيق وسجّل الـ route ← `appsettings` ضبطت الـ Logging ← جاء طلب GET على `/` فنُفِّذ `MapGet` وأُرجع النص.

## 6. الملخص والواجب

| الملف | ما هو | من يقرؤه |
|---|---|---|
| `ProjectStructure.csproj` | بطاقة هوية المشروع | أداة `dotnet` عند البناء |
| `Program.cs` | نقطة البداية | يعمل عند تشغيل التطبيق |
| `appsettings*.json` | الإعدادات | التطبيق عند البداية |
| `launchSettings.json` | تشغيل محلي | `dotnet run` والمحرر فقط |

واجب:
1. غيّر المنفذ في `launchSettings.json` وشغّل المشروع عليه.
2. أضف route جديد `/about` يرجع اسمك.
