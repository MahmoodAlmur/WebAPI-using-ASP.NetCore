# الدرس: Dependency Injection

---

## 1. المقدمة
عندما تكتب `new` داخل كلاس، فقد ربطته بذلك النوع إلى الأبد. تريد تغييره؟ تعدّل الكلاس. الحل هو **Dependency Injection**، وهو ما يعتمد عليه ASP.NET Core في كل شيء.

---

## 2. ما هو الـ Dependency؟

**ما هو؟** إذا كان الكلاس A يحتاج الكلاس B ليعمل، فإن B هو **Dependency** (اعتماد) لـ A.

**مثال بسيط:** العميل (`Customer`) يحتاج طريقة دفع (`CashPayment`) ليشتري. فطريقة الدفع هي الـ Dependency للعميل.

## 3. الطريقة الخاطئة: العميل ينشئ طريقة الدفع بنفسه

```csharp
public class CashPayment
{
    public string Pay() => "Paid with cash";
}

public class Customer
{
    private readonly CashPayment _payment = new CashPayment();   // العميل ينشئ طريقة الدفع

    public string Buy()
    {
        return _payment.Pay() + " - purchase completed";
    }
}
```

**ما المشكلة؟** (تسمى **Tight Coupling**، أي ارتباط شديد)
- العميل مربوط بالدفع النقدي فقط. تريد الدفع بالبطاقة؟ يجب أن تفتح كلاس `Customer` وتعدّله.
- مثل شخص لا يملك إلا طريقة دفع واحدة ثابتة، ولا يمكنه تغييرها.

## 4. الطريقة الصحيحة: طريقة الدفع تُسلَّم للعميل من الخارج

نحتاج شيئين:

**الأول: Interface**، أي عقد يقول "أي طريقة دفع يجب أن تعمل `Pay`":
```csharp
public interface IPaymentMethod
{
    string Pay();
}

public class CashPayment : IPaymentMethod
{
    public string Pay() => "Paid with cash";
}

public class CardPayment : IPaymentMethod
{
    public string Pay() => "Paid with card";
}
```

**الثاني: العميل يستقبل طريقة الدفع في الـ Constructor** ولا ينشئها:
```csharp
public class Customer
{
    private readonly IPaymentMethod _payment;

    public Customer(IPaymentMethod payment)
    {
        _payment = payment;
    }

    public string Buy()
    {
        return _payment.Pay() + " - purchase completed";
    }
}
```
- العميل الآن لا يعرف أي طريقة دفع يستخدم، يكفيه أنها `IPaymentMethod`.
- أي طريقة دفع تحقق الـ Interface يمكن إعطاؤها له.
- هذه الفكرة هي **Dependency Injection**.

## 5. مبدأ Dependency Inversion

**ما هو؟** مبدأ في تصميم البرمجيات (هو الحرف D في **SOLID**)، ونصه:

> **High-level modules should not depend on low-level modules. Both should depend on abstractions.**
>
> **Abstractions should not depend on details. Details should depend on abstractions.**

**معناه بالعربية:**
- الوحدات **عالية المستوى** لا تعتمد على الوحدات **منخفضة المستوى**، بل يعتمد **الاثنان على Abstraction**.
- والـ Abstraction لا يعتمد على التفاصيل، بل **التفاصيل هي التي تعتمد عليه**.

**المصطلحات:**
- **High-level module:** الوحدة التي فيها منطق التطبيق الأساسي، وتقرر **ماذا** يحدث.
- **Low-level module:** الوحدة التي فيها **تفاصيل التنفيذ**، وتقرر **كيف** يحدث.
- **Abstraction:** الـ Interface (أو الـ abstract class) الذي يفصل بينهما.

**تطبيقه على مثالنا:**
| في المبدأ | في مثالنا |
|---|---|
| High-level module | `Customer` (منطق الشراء) |
| Low-level module | `CashPayment` و`CardPayment` (كيف يتم الدفع) |
| Abstraction | `IPaymentMethod` |

**قبل تطبيق المبدأ:** العميل يعتمد مباشرة على التفصيل.
```
Customer  ───►  CashPayment
```
**بعد تطبيقه:** كلاهما يعتمد على الـ Abstraction.
```
Customer  ───►  IPaymentMethod  ◄───  CashPayment
```
- `Customer` يعرف `IPaymentMethod` فقط، ولا يعرف أي طريقة دفع.
- `CashPayment` هي التي تعتمد على الـ Interface (تنفّذه)، وليس العكس.

**لماذا اسمه Inversion (عكس)؟** لأن اتجاه الاعتماد انعكس: بدل أن يشير السهم من الوحدة العليا إلى التفصيل، صار التفصيل (`CashPayment`) هو الذي يشير إلى الـ Abstraction الذي تحتاجه الوحدة العليا.

**ما الفرق بين المبدأ والـ Dependency Injection؟**
| | ما هو | يجيب عن |
|---|---|---|
| **Dependency Inversion** | **مبدأ** تصميم | **ماذا** نفعل: نعتمد على Abstractions |
| **Dependency Injection** | **أسلوب** تنفيذ | **كيف** نطبقه: نمرّر ما يحتاجه الكلاس من الخارج |

نستخدم الـ Interface لنطبق المبدأ، ونستخدم الـ DI لنعطي الكلاس التنفيذ المناسب.

## 6. ما هو الـ Dependency Injection؟

**التعريف:** أسلوب يستقبل فيه الكلاس ما يحتاجه من الخارج بدل أن ينشئه بنفسه.

بكلمات المثال: بدل أن ينشئ العميل طريقة دفعه، **تُعطى له** طريقة الدفع من الخارج.

**أطرافه:**
| الطرف | في مثالنا |
|---|---|
| **الخدمة (Service)** ما يُعطى | طريقة الدفع `IPaymentMethod` |
| **المستخدم (Client)** من يحتاجها | العميل `Customer` |
| **من يعطيها (Injector)** | الكود الذي ينشئ طريقة الدفع ويمرّرها للعميل |

**طريقة الحقن:** الأكثر استخدامًا هي عبر الـ Constructor، كما فعلنا في `Customer`.

## 7. تجربة 1: نمرّر طريقة الدفع بأيدينا

في `Program.cs`:
```csharp
using ProjectStructure.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/buy", () =>
{
    IPaymentMethod payment = new CashPayment();
    Customer customer = new Customer(payment);
    return customer.Buy();
});

app.Run();
```
- أنشأنا طريقة الدفع **خارج** `Customer` ثم مرّرناها له في الـ Constructor.
- ضع الكلاسات في مجلد `Services/`.

افتح `http://localhost:5149/buy`:
```
Paid with cash - purchase completed
```

## 8. تجربة 2: تبديل طريقة الدفع بسطر واحد

غيّر سطر واحد فقط:
```csharp
IPaymentMethod payment = new CardPayment();
```
أعد التشغيل وافتح نفس الرابط:
```
Paid with card - purchase completed
```
- لم نعدّل `Customer` أبدًا.
- هذه هي قوة الـ DI: **تغيير ما يُعطى للكلاس دون تعديل الكلاس**.

## 9. تجربة 3: كائن وهمي للاختبار

نكتب طريقة دفع وهمية لا تدفع شيئًا:
```csharp
public class FakePayment : IPaymentMethod
{
    public string Pay() => "Fake payment";
}
```
ونعطيها للعميل:
```csharp
Customer customer = new Customer(new FakePayment());
```
النتيجة:
```
Fake payment - purchase completed
```
- اختبرنا `Customer` بدون أن يدفع فعليًا، وهذا مستحيل لو كان ينشئ `CashPayment` بنفسه.

## 10. فوائد الـ DI

| الفائدة | كيف تتحقق |
|---|---|
| **سهولة التغيير** | نبدّل طريقة الدفع دون تعديل `Customer` |
| **Loose Coupling** | العميل لا يعرف نوع الدفع، فقط الـ Interface |
| **سهولة الاختبار** | نعطي العميل طريقة دفع وهمية بدل الحقيقية |

## 11. الملخص والواجب

| المفهوم | ما هو |
|---|---|
| **Dependency** | ما يحتاجه الكلاس ليعمل (طريقة الدفع للعميل) |
| **Tight Coupling** | الكلاس ينشئ ما يحتاجه بـ `new` فيرتبط به |
| **Dependency Injection** | الكلاس يستقبل ما يحتاجه من الخارج، غالبًا عبر الـ Constructor |
| **Dependency Inversion** | مبدأ: الوحدات العليا والدنيا تعتمد على Abstractions لا على بعضها |
| **Interface** | يجعل الجزء قابلًا للتبديل |

واجب:
1. أضف طريقة دفع ثالثة `PaypalPayment` وأعطها للعميل بدل الحالية.
2. أنشئ `IGreeter` وكلاسين `ArabicGreeter` و`EnglishGreeter`، وكلاس `Welcome` يستقبل `IGreeter` في الـ Constructor، وبدّل بين الكلاسين بسطر واحد.
3. اكتب `FakeGreeter` واستخدمه مع `Welcome`.
