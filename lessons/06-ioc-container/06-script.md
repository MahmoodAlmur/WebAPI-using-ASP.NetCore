# الدرس: IoC Container

---

## 1. المقدمة
كل مرة كتبنا `builder.Services.AddXxx()` كنا نتعامل مع الـ **IoC Container**. في هذا الدرس نفتح هذه الحاوية: ماذا تفعل بالضبط، وبأي أشكال نسجّل فيها، وكم يعيش كل كائن، وما الأخطاء التي تقع فيها.

---

## 2. ما هو الـ IoC؟

**IoC** هي اختصار **Inversion of Control**، أي **عكس التحكم**.

- **بدون IoC:** الكلاس هو المتحكم، فهو الذي ينشئ ما يحتاجه (`new`).
- **مع IoC:** يتحكم طرف آخر، فهو الذي ينشئ الكائنات ويعطيها للكلاس. الكلاس فقد التحكم في الإنشاء، وهذا هو "العكس".

**الـ IoC Container** هو هذا الطرف: **حاوية** تعرف كيف تنشئ الكائنات وتحتفظ بها وتسلّمها لمن يطلبها.

## 3. ماذا تفعل الحاوية؟ ثلاث مهام

| المهمة | ما معناها | أين نراها |
|---|---|---|
| **Register** (التسجيل) | نخبرها ماذا تنشئ ولأي نوع | `builder.Services.AddXxx()` |
| **Resolve** (التسليم) | تنشئ الكائن وتسلّمه عند الطلب، وتبني سلسلة ما يحتاجه | تلقائيًا عند الـ Constructor |
| **Dispose** (التخلص) | تتخلص من الكائنات التي أنشأتها بعد انتهاء عمرها | تلقائيًا |

**من ينشئها؟** `WebApplication.CreateBuilder` يجهّز الحاوية، ونسجّل فيها عبر `builder.Services`. وعند `Build()` تُبنى الحاوية ولا نستطيع التسجيل بعده.

---

## 4. أشكال التسجيل

### 4.1 نوع وتنفيذه
```csharp
builder.Services.AddScoped<IPaymentMethod, CashPayment>();
```
"عندما يُطلب `IPaymentMethod` أعطِ `CashPayment`". وهو الشكل الأكثر استخدامًا.

### 4.2 كلاس بدون Interface
```csharp
builder.Services.AddScoped<Connection>();
```
يُطلب الكلاس نفسه ويُنشأ هو نفسه.

### 4.3 بواسطة Factory (دالة تنشئ الكائن)
```csharp
builder.Services.AddScoped<IPaymentMethod>(sp => new CardPayment());
```
نستخدمها عندما يحتاج إنشاء الكائن منطقًا خاصًا. المعامل `sp` هو الـ `IServiceProvider` الذي نطلب منه خدمات أخرى عند الحاجة.

### 4.4 كائن جاهز (للـ Singleton فقط)
```csharp
builder.Services.AddSingleton<IClock>(new SystemClock());
```
ننشئ الكائن بأنفسنا ونسلمه للحاوية لتحتفظ به.

---

## 5. الـ Lifetimes: كم يعيش الكائن؟

عند التسجيل نحدد **كم يعيش الكائن**، أي متى تنشئ الحاوية كائنًا جديدًا ومتى تعيد نفس الكائن.

| الطريقة | عمر الكائن |
|---|---|
| `AddTransient` | كائن جديد **في كل مرة** يُطلب فيها |
| `AddScoped` | كائن واحد **لكل طلب HTTP** |
| `AddSingleton` | كائن واحد **طوال عمر التطبيق** |

**تشبيه (مطعم):**
- **Transient:** كوب ورقي جديد في كل مرة تطلب فيها مشروبًا.
- **Scoped:** كوب واحد لكل زبون طوال زيارته، فإذا خرج الزبون أُخذ منه، وجاء الزبون التالي بكوب جديد.
- **Singleton:** ماكينة قهوة واحدة للمطعم كله، يستخدمها كل الزبائن.

---

### مثال كامل: نجرّب كل عمر على حدة

**الفكرة:** كلاس واحد `Operation` فيه رقم تعريف `Guid` يُولَّد **مرة واحدة عند إنشاء الكائن**. فإذا رأينا نفس الرقم مرتين فهو **نفس الكائن**، وإذا اختلف الرقم فهو **كائن جديد**.

نغيّر **سطر التسجيل فقط** ثلاث مرات (Transient ثم Scoped ثم Singleton)، وفي كل مرة نجرّب ونقرأ النتيجة.

#### الخطوة 1: الـ Interface والكلاس
ضعهما في مجلد `Services/`:
```csharp
public interface IOperation
{
    Guid Id { get; }
}

public class Operation : IOperation
{
    public Guid Id { get; } = Guid.NewGuid();
}
```

#### الخطوة 2: الـ route
بعد `Build()`:
```csharp
app.MapGet("/operation", (IOperation first, IOperation second) =>
    $"first = {first.Id.ToString()[..8]}   second = {second.Id.ToString()[..8]}");
```
- الـ route يطلب `IOperation` **مرتين**: `first` و`second`.
- نعرض أول 8 خانات من الـ `Guid` ليسهل التمييز.
- السؤال في كل تجربة: هل `first` و`second` **نفس الكائن** أم كائنان؟ وهل يتغير الرقم عند **طلب جديد**؟

**طريقة التجربة في كل مرة:** غيّر سطر التسجيل، شغّل التطبيق، وافتح `http://localhost:5149/operation` **مرتين** (حدّث الصفحة).

---

#### التجربة 1: Transient
في `Program.cs`، بين `CreateBuilder` و`Build()`:
```csharp
builder.Services.AddTransient<IOperation, Operation>();
```
النتيجة:
```
الطلب الأول:  first = b5edf808   second = 189f0bac
الطلب الثاني: first = 3410371c   second = 93d5c59f
```
**القراءة:**
- `first` و`second` **مختلفان** داخل الطلب الواحد: كل مرة طُلب فيها `IOperation` أُنشئ كائن جديد.
- والأرقام تغيّرت في الطلب الثاني أيضًا.

#### التجربة 2: Scoped
غيّر السطر إلى:
```csharp
builder.Services.AddScoped<IOperation, Operation>();
```
النتيجة:
```
الطلب الأول:  first = 390c163f   second = 390c163f
الطلب الثاني: first = 6bde7177   second = 6bde7177
```
**القراءة:**
- `first` و`second` **متطابقان** داخل الطلب الواحد: كائن واحد يشاركه الجميع في هذا الطلب.
- والرقم **تغيّر** في الطلب الثاني: كائن جديد لكل طلب.

#### التجربة 3: Singleton
غيّر السطر إلى:
```csharp
builder.Services.AddSingleton<IOperation, Operation>();
```
النتيجة:
```
الطلب الأول:  first = 5f708413   second = 5f708413
الطلب الثاني: first = 5f708413   second = 5f708413
```
**القراءة:**
- `first` و`second` متطابقان، والرقم **لم يتغير** في الطلب الثاني: كائن واحد طوال عمر التطبيق.
- لو أوقفت التطبيق وشغّلته من جديد يظهر رقم جديد، لأن عمر التطبيق انتهى.

#### الخلاصة: المقارنة
| العمر | داخل الطلب الواحد (`first` و`second`) | بين طلبين |
|---|---|---|
| **Transient** | **مختلفان** | يتغير |
| **Scoped** | **متطابقان** | **يتغير** |
| **Singleton** | **متطابقان** | **لا يتغير** |

- **Transient:** كل طلب للكائن = كائن جديد، حتى داخل الطلب الواحد.
- **Scoped:** كائن واحد يشاركه كل من في الطلب نفسه، ويتجدد مع الطلب التالي.
- **Singleton:** كائن واحد للتطبيق كله.

### متى نستخدم كل واحد؟
- **Transient:** خدمة خفيفة بلا حالة (Stateless).
- **Scoped:** خدمة مرتبطة بطلب واحد، مثل الوصول لقاعدة البيانات.
- **Singleton:** شيء مكلف إنشاؤه أو يُشارك بين الجميع، مثل الـ Cache. ويجب أن يكون **آمنًا للتعامل من عدة طلبات في نفس الوقت** (Thread-safe).

---

## 6. خطأ مهم: Captive Dependency

**ما هو؟** أن يعتمد كائن **أطول عمرًا** على كائن **أقصر عمرًا**. مثال: `Singleton` يطلب `Scoped` في الـ Constructor. فالـ Singleton يعيش طوال التطبيق، لكنه سيحتفظ بكائن Scoped كان يجب أن يموت مع طلبه، أي "أسره" عنده.
```csharp
builder.Services.AddScoped<ScopedService>();
builder.Services.AddSingleton<SingletonService>();

public class SingletonService
{
    public SingletonService(ScopedService scoped) { }
}
```
في بيئة Development يتوقف التطبيق عند التشغيل بهذا الخطأ:
```
Cannot consume scoped service 'ScopedService' from singleton 'SingletonService'.
```
**القاعدة:** الكائن يعتمد فقط على كائنات **عمرها مثل عمره أو أطول**.

| يعتمد على ← | Transient | Scoped | Singleton |
|---|---|---|---|
| **Transient** | مسموح | مسموح | مسموح |
| **Scoped** | مسموح | مسموح | مسموح |
| **Singleton** | مسموح (لكن يبقى حيًا معه) | **خطأ** | مسموح |

---

## 7. أكثر من تنفيذ لنفس الـ Interface

```csharp
builder.Services.AddScoped<IPaymentMethod, CashPayment>();
builder.Services.AddScoped<IPaymentMethod, CardPayment>();
```
```csharp
app.MapGet("/many", (IPaymentMethod single, IEnumerable<IPaymentMethod> all) => new
{
    Single = single.Pay(),
    All = all.Select(p => p.Pay())
});
```
النتيجة:
```json
{"single":"Paid with card","all":["Paid with cash","Paid with card"]}
```
- طلب `IPaymentMethod` وحده يعطي **آخر تسجيل** (`CardPayment`).
- طلب `IEnumerable<IPaymentMethod>` يعطي **كل التنفيذات المسجّلة** بترتيب تسجيلها.

### TryAdd: سجّل إن لم يكن مسجّلًا
```csharp
using Microsoft.Extensions.DependencyInjection.Extensions;

builder.Services.TryAddScoped<IPaymentMethod, CashPayment>();
```
إذا كان `IPaymentMethod` مسجّلًا من قبل فلا يفعل شيئًا، فلا يغيّر التسجيل الموجود (النتيجة أعلاه بقيت `CardPayment`). مفيد لمن يكتب مكتبات.

---

## 8. Keyed Services: تسجيل بمفتاح

عندما نريد **تنفيذًا محددًا** من بين عدة تنفيذات، نسجّل كل واحد بمفتاح:
```csharp
builder.Services.AddKeyedScoped<IPaymentMethod, CashPayment>("cash");
builder.Services.AddKeyedScoped<IPaymentMethod, CardPayment>("card");
```
ونطلبه بالمفتاح:
```csharp
using Microsoft.AspNetCore.Mvc;

app.MapGet("/keyed", ([FromKeyedServices("card")] IPaymentMethod payment) => payment.Pay());
```
النتيجة: `Paid with card`.

---

## 9. الحصول على خدمة يدويًا

**الشكل الطبيعي** هو الـ Constructor. لكن أحيانًا نحتاج الحاوية نفسها، عبر `IServiceProvider`:
```csharp
app.MapGet("/manual", (IServiceProvider sp) =>
{
    using IServiceScope scope = sp.CreateScope();
    IPaymentMethod payment = scope.ServiceProvider.GetRequiredService<IPaymentMethod>();
    return payment.Pay();
});
```
- `CreateScope()` ينشئ **نطاقًا جديدًا** (Scope)، وكل ما هو Scoped يُنشأ داخله ويُتخلص منه عند نهاية الـ `using`.
- `GetRequiredService<T>()` يرمي خطأ إن لم يجد التسجيل، و`GetService<T>()` يرجع `null`.

**تحذير: Service Locator.** أن تطلب الخدمات من `IServiceProvider` داخل كلاساتك بدل الـ Constructor يخفي ما يحتاجه الكلاس ويجعله أصعب في الاختبار. نستخدمه فقط عند الضرورة (مثل المهام الخلفية).

---

## 10. التخلص من الكائنات (Dispose)

الحاوية **تتخلص** من كل كائن `IDisposable` **أنشأته هي**، عند انتهاء عمره:
```csharp
public class Connection : IDisposable
{
    public Connection() => Console.WriteLine("Connection created");
    public void Dispose() => Console.WriteLine("Connection disposed");
}

builder.Services.AddScoped<Connection>();

app.MapGet("/dispose", (Connection connection) => "done");
```
افتح `/dispose`، فيظهر في الـ Console:
```
Connection created
Connection disposed
```
- الكائن الـ Scoped يُتخلص منه **عند نهاية الطلب**.
- الكائن الـ Singleton يُتخلص منه **عند إيقاف التطبيق**.
- **لا تتخلص أنت** (`Dispose`) من كائن أنشأته الحاوية، فهي من تفعل ذلك. أما كائن أنشأته أنت بـ `new` وسلمته جاهزًا، فالحاوية **لا** تتخلص منه.

---

## 11. تنظيم التسجيلات

مع كثرة الخدمات يمتلئ `Program.cs`. نجمعها في **Extension Method**:
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentMethod, CashPayment>();
        services.AddScoped<Customer>();
        return services;
    }
}
```
وفي `Program.cs`:
```csharp
builder.Services.AddPaymentServices();
```

---

## 12. الملخص والواجب

| المفهوم | ما هو |
|---|---|
| **IoC** | عكس التحكم: طرف آخر هو الذي ينشئ الكائنات |
| **IoC Container** | حاوية تسجّل وتسلّم وتتخلص |
| **التسجيل** | نوع وتنفيذ، أو كلاس، أو Factory، أو كائن جاهز |
| **Lifetimes** | Transient جديد دائمًا، Scoped لكل طلب، Singleton طوال التطبيق |
| **Captive Dependency** | Singleton يعتمد على Scoped، وهو خطأ |
| **أكثر من تنفيذ** | آخر تسجيل هو الذي يُعطى، و`IEnumerable` يعطي الكل |
| **Keyed Services** | تسجيل بمفتاح وطلب تنفيذ محدد |
| **Dispose** | الحاوية تتخلص مما أنشأته |

واجب:
1. نفّذ التجارب الثلاث للـ Lifetimes، وفي كل مرة افتح `/operation` مرتين، وسجّل ما يتشابه وما يختلف.
2. اجعل `SingletonService` يطلب `ScopedService` في الـ Constructor، وسجّل الخطأ.
3. سجّل تنفيذين لـ `IPaymentMethod` وأنشئ route يرجع كل نتائج `Pay()` عبر `IEnumerable`.
4. جرّب Keyed Services لاختيار `cash` أو `card` من الرابط.
