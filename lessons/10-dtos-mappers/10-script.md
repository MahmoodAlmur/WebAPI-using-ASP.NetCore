# الدرس 10: DTOs و Mappers

---

## 1. المقدمة
في الدروس السابقة كان الـ Controller **يستقبل الكيان (Entity) ويرجعه كما هو**. مثلًا `Student` يدخل في `POST` ويخرج من `GET`. هذا يعمل، لكنه يربط شكل الـ API بشكل البيانات الداخلي، وله مشاكل سنراها بالتجربة. الحل هو **DTO**، ونحتاج بعده **Mapper** يحوّل الكيان إلى Response، نكتبه بـ Extension Method.

---

## 2. ما هو الـ DTO؟

**ما هو؟** الـ **DTO** اختصار **Data Transfer Object**. هو **كلاس بسيط يحمل البيانات فقط** (بدون منطق)، يمثل **الشكل الذي تُنقل به البيانات بين العميل والـ API**. وهو منفصل عن **الكيان (Entity)** الذي يمثل البيانات داخل الـ Server.

```
العميل  <-- DTO -->  Controller  <-- Entity -->  البيانات
```

- **Entity:** شكل البيانات **داخل** التطبيق (قد يحتوي حقولًا داخلية).
- **DTO:** شكل البيانات **على حدود الـ API**: ما يرسله العميل وما يستقبله.

**ما معنى "حدود الـ API"؟** هي نقطة الدخول والخروج: الـ Parameters التي يستقبلها الـ Action، والقيمة التي يرجعها. هنا نستخدم DTOs، وداخل التطبيق نستخدم الكيانات.

---

## 3. المشكلة بدون DTO

نبدأ بمشروع الـ Controllers، ونضيف إلى الكيان `Student` الحقلين `Password` و`CreatedAt`:
```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";     // داخلي: يجب ألا يخرج من الـ Server
    public DateTime CreatedAt { get; set; }
}
```
ونترك الـ Controller يتعامل مع الكيان مباشرة:
```csharp
[HttpGet("{id}")]
public ActionResult<Student> GetById(int id) => Ok(data.Students.First(s => s.Id == id));

[HttpPost]
public ActionResult<Student> Create(Student student) { ... }
```

### المشكلة 1: تسريب حقول داخلية
`GET /api/students/1` يرجع:
```json
{"id":1,"name":"Ahmed Ali","email":"ahmed@example.com","password":"ahmed123","createdAt":"2026-08-01T00:00:00"}
```
ظهرت كلمة المرور `password`، وهي حقل لا نريد أن يراه أحد. كل حقل في الكيان **يخرج تلقائيًا**، وكل حقل نضيفه لاحقًا يخرج أيضًا بدون أن ننتبه.

### المشكلة 2: الـ Overposting
العميل يستطيع أن **يرسل حقولًا لم نقصد أن يحددها**:
```json
POST /api/students
{ "name": "Lina", "email": "l@e.com", "password": "pw", "createdAt": "2000-01-01T00:00:00" }
```
والرد:
```json
{"id":4,"name":"Lina","email":"l@e.com","password":"pw","createdAt":"2000-01-01T00:00:00"}
```
دخلت القيمة `createdAt` إلى الكيان كما أرسلها العميل، مع أن وقت الإنشاء يجب أن يحدده الـ Server. الـ Model Binding يملأ **كل** خصائص الكيان التي تطابق أسماء الجسم.

### المشكلة 3: شكل واحد لكل العمليات
`Id` يحدده الـ Server، فلا معنى لأن يظهر في `POST` كحقل مطلوب. لكن الكيان يستخدمه في الإنشاء والتعديل والعرض كلها بنفس الشكل.

### المشكلة 4: ربط الـ API بالكيان
إذا غيّرنا الكيان (إعادة تسمية حقل، أو إضافة حقل) **تغيّر شكل الـ API** وقد ينكسر العملاء الذين يستخدمونه.

---

## 4. الحل: DTO لكل غرض

نفصل الأشكال:

| النوع | الاستخدام | مثال |
|---|---|---|
| **Response** | ما **يرجعه** الـ API | `StudentResponse` |
| **Create Request** | ما **يرسله** العميل لإنشاء عنصر | `CreateStudentRequest` |
| **Update Request** | ما **يرسله** العميل لتعديل عنصر | `UpdateStudentRequest` |

كل نوع في ملف خاص، والـ Responses والـ Requests في مجلدين منفصلين:
```csharp
// Dtos/Responses/StudentResponse.cs
namespace UniversityApi.Dtos.Responses;

// ما يرجعه الـ API
public record StudentResponse(int Id, string Name, string Email);
```
```csharp
// Dtos/Requests/CreateStudentRequest.cs
namespace UniversityApi.Dtos.Requests;

// ما يرسله العميل للإنشاء
public record CreateStudentRequest(string Name, string Email, string Password);
```
```csharp
// Dtos/Requests/UpdateStudentRequest.cs
namespace UniversityApi.Dtos.Requests;

// ما يرسله العميل للتعديل
public record UpdateStudentRequest(string Name, string Email);
```

**التسمية:** نسمي كل نوع بحسب الغرض: `{الكيان}Response` للرد، و`Create{الكيان}Request` و`Update{الكيان}Request` للطلب. مثلًا `CustomerResponse` و`EmployeeResponse` و`CreateCustomerRequest` و`UpdateCustomerRequest`. وهنا مع كيانات مشروعنا: `StudentResponse` و`CourseResponse` و`EnrollmentResponse`.

**لماذا `record`؟** الـ DTO يحمل بيانات فقط، والـ `record` يعرّف الخصائص في سطر واحد، وقيمه **لا تتغير** بعد إنشائه (Immutable).

**ملاحظة:** `CreateStudentRequest` و`UpdateStudentRequest` لهما نفس الحقول الآن، لكننا نفصلهما لأنهما **يتغيران بشكل مستقل** مع الوقت (مثلًا قد يمنع التعديل تغيير الإيميل لاحقًا).

لا يوجد `Id` ولا `CreatedAt` في أي من الـ Requests: ما يحدده الـ Server لا يرسله العميل. و`Password` موجود في `CreateStudentRequest` فقط (العميل يرسلها عند التسجيل)، وليس في `UpdateStudentRequest` ولا في `StudentResponse`.

---

## 5. DTO يجمع بياناتٍ من أكثر من كيان

الكيان `Enrollment` فيه `StudentId` و`CourseId` فقط. لو رجعناه كما هو، يحتاج العميل أن يسأل مرتين ليعرف اسم الطالب وعنوان الدورة. نصمم DTO **على قدر حاجة العميل**:
```csharp
public record EnrollmentResponse(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseTitle,
    DateTime EnrolledAt);

public record EnrollRequest(int StudentId, int CourseId);
```
هنا الـ DTO **ليس نسخة من الكيان**، بل شكل مخصص للعميل. وحقل `Grade` في الكيان لم نعرضه لأنه ليس ضمن ما قررنا إظهاره.

---

## 6. استقبال الـ Request: عن طريق الـ Model Binding

الـ **Request** لا نحتاج لتحويله بـ Mapper. هو **Parameter في الـ Action**، والـ **Model Binding** هو الذي يقرأ جسم الطلب ويملأ كائن الـ Request:
```csharp
[HttpPost]
public ActionResult<StudentResponse> Create(CreateStudentRequest request)
```
عند وصول الطلب، يبني الـ Model Binding كائن `CreateStudentRequest` من الـ JSON. والحقول التي ليست في الـ Request (مثل `id` و`createdAt`) **لا يملؤها أحد**، فلا تصل إلى الكود.

ثم داخل الـ Action نبني الكيان بأنفسنا من الـ Request:
```csharp
// الـ Server يحدد الـ Id و CreatedAt
Student student = new Student
{
    Id = data.Students.Count == 0 ? 1 : data.Students.Max(s => s.Id) + 1,
    Name = request.Name,
    Email = request.Email,
    Password = request.Password,
    CreatedAt = DateTime.UtcNow
};

data.Students.Add(student);
```
والتعديل ننسخ الحقول المسموحة فقط على الكيان الموجود:
```csharp
[HttpPut("{id}")]
public IActionResult Update(int id, UpdateStudentRequest request)
{
    Student? student = data.Students.FirstOrDefault(s => s.Id == id);

    if (student == null)
        return NotFound();

    student.Name = request.Name;
    student.Email = request.Email;

    return NoContent();
}
```

---

## 7. إرجاع الـ Response: Mapper بـ Extension Method

الـ **Response** هو ما نرجعه، ويجب تحويل الكيان إليه. نكتب هذا التحويل **في مكان واحد** بدل تكراره في كل Action، على شكل **Extension Method** اسمها `ToResponse`. ننشئ مجلد `Mappers` داخل `Dtos` وفيه كلاس static لكل كيان:
```csharp
// Dtos/Mappers/StudentMapper.cs
namespace UniversityApi.Dtos.Mappers;

public static class StudentMapper
{
    public static StudentResponse ToResponse(this Student student)
    {
        return new StudentResponse(student.Id, student.Name, student.Email);
    }
}
```
والدالة تحدد الحقول التي تخرج: `Password` ليس فيها. فإذا أضفنا حقلًا للـ Response نعدّل هذا المكان فقط.

### ماذا لو كان الـ Response قائمة (`IEnumerable`)؟
عندما يرجع الـ Action قائمة من الكيانات، نضيف Extension Method ثانية على **`IEnumerable<Student>`** تستدعي الأولى لكل عنصر:
```csharp
public static List<StudentResponse> ToResponseList(this IEnumerable<Student> students)
{
    return students.Select(s => s.ToResponse()).ToList();
}
```
- النوع `IEnumerable<Student>` يقبل `List<Student>` والمصفوفة ونتيجة `Where(...)`، فتعمل الدالة على أي قائمة.
- الدالة تعيد `List<StudentResponse>` جاهزة للإرجاع.
- سميناها `ToResponseList` (وليس `ToResponse`) ليكون الاسم واضحًا: واحدة للعنصر وواحدة للقائمة.

ولـ `Enrollment` تستقبل الدالة الكيانات التي تجمع منها البيانات:
```csharp
public static class EnrollmentMapper
{
    public static EnrollmentResponse ToResponse(this Enrollment enrollment, Student student, Course course)
    {
        return new EnrollmentResponse(
            enrollment.Id, student.Id, student.Name,
            course.Id, course.Title, enrollment.EnrolledAt);
    }
}
```

---

## 8. استخدام `ToResponse` في الـ Controller

نضيف `using UniversityApi.Dtos.Mappers;` (وكذلك `using UniversityApi.Dtos.Requests;` و`using UniversityApi.Dtos.Responses;`) ونستدعي الدالة:
```csharp
[HttpGet("{id}")]
public ActionResult<StudentResponse> GetById(int id)
{
    Student? student = data.Students.FirstOrDefault(s => s.Id == id);

    if (student == null)
        return NotFound();

    return Ok(student.ToResponse());
}
```
والقائمة:
```csharp
[HttpGet]
public ActionResult<List<StudentResponse>> GetAll()
{
    return Ok(data.Students.ToResponseList());
}
```
ونفس الشيء بعد `Where`:
```csharp
List<Course> courses = data.Courses
    .Where(c => courseIds.Contains(c.Id))
    .ToList();

return Ok(courses.ToResponseList());
```
وبعد الإنشاء نرجع الكيان الجديد كـ Response:
```csharp
return CreatedAtAction(nameof(GetById), new { id = student.Id }, student.ToResponse());
```
والـ Enrollments: نجد الطالب والدورة ثم نستدعي `enrollment.ToResponse(student, course)`.

**النتائج عند التجربة:**
- `GET /api/students/1` يرجع `{"id":1,"name":"Ahmed Ali","email":"ahmed@example.com"}` بدون `password`.
- `POST` بهذا الجسم:
```json
{ "id": 99, "name": "Omar Zaid", "email": "omar@example.com", "password": "omar123", "createdAt": "2000-01-01T00:00:00" }
```
  يرجع `201` مع `{"id":4,"name":"Omar Zaid","email":"omar@example.com"}`. الحقول التي ليست في `CreateStudentRequest` تُتجاهل، و`Id` تسلسلي من الـ Server.
- `GET /api/enrollments` يرجع الأسماء: `"studentName":"Ahmed Ali","courseTitle":"ASP.NET Core"`.

---

## 9. مثال: تسجيل الدخول (Identity)

نضيف `IdentityController` فيه Action واحدة لتسجيل الدخول. هو **للشرح فقط**: كلمة المرور تُخزَّن وتُقارن **كنص عادي، بدون Hash وبدون Token**.

الـ Request والـ Response:
```csharp
// Dtos/Requests/LoginRequest.cs
public record LoginRequest(string Email, string Password);

// Dtos/Responses/LoginResponse.cs
public record LoginResponse(int StudentId, string Name, string Message);
```
الـ Controller:
```csharp
[ApiController]
[Route("api/identity")]
public class IdentityController(InMemoryData data) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        Student? student = data.Students
            .FirstOrDefault(s => s.Email == request.Email && s.Password == request.Password);

        if (student == null)
            return Unauthorized("Wrong email or password.");

        return Ok(new LoginResponse(student.Id, student.Name, "Logged in."));
    }
}
```
- الـ `LoginRequest` يحمل كلمة المرور **داخلًا** فقط، والـ `LoginResponse` لا يحملها أبدًا.
- نستخدم `Unauthorized()` (الرمز `401`) لأي فشل، **بدون أن نقول** هل الخطأ في الإيميل أم في كلمة المرور.

التجربة:
- `POST /api/identity/login` مع `{ "email": "ahmed@example.com", "password": "ahmed123" }` يرجع `200` مع `{"studentId":1,"name":"Ahmed Ali","message":"Logged in."}`.
- مع كلمة مرور خاطئة أو إيميل غير موجود يرجع `401`.
- مع جسم مكسور يرجع `400`.
- بعد `POST /api/students` بكلمة مرور جديدة، يعمل تسجيل الدخول بها، والـ `StudentResponse` لا يحتوي `password`.

---

## 10. أثر الـ DTOs على Swagger

الوثيقة تصف الآن **الـ DTOs** وليس الكيانات. الـ Schemas في `/openapi/v1.json`:
```
StudentResponse            [id, name, email]
CreateStudentRequest  [name, email, password]
UpdateStudentRequest  [name, email]
CourseResponse             [id, title]
CreateCourseRequest   [title]
UpdateCourseRequest   [title]
EnrollmentResponse         [id, studentId, studentName, courseId, courseTitle, enrolledAt]
EnrollRequest         [studentId, courseId]
LoginRequest          [email, password]
LoginResponse         [studentId, name, message]
```
لا يوجد `Student` ولا `password` في أي Response في الوثيقة. ولهذا نكتب في `[ProducesResponseType<StudentResponse>]` الـ DTO وليس الكيان.

---

## 11. تنظيم الملفات والتسمية

```
UniversityApi/
├── Controllers/        <- Students, Courses, Enrollments, Identity
├── Data/
├── Models/                 <- الكيانات: Student, Course, Enrollment
└── Dtos/
    ├── Requests/           <- CreateStudentRequest, UpdateStudentRequest, CreateCourseRequest, UpdateCourseRequest, EnrollRequest
    ├── Responses/          <- StudentResponse, CourseResponse, EnrollmentResponse
    └── Mappers/            <- StudentMapper, CourseMapper, EnrollmentMapper
```
- ملف واحد لكل Request أو Response، والمجلد يدل على نوعه.
- أسماء الـ DTOs بحسب الغرض: `StudentResponse` للرد، و`CreateStudentRequest` و`UpdateStudentRequest` للطلب.
- كلاس Mapper لكل كيان فيه الدالتان `ToResponse` (عنصر) و`ToResponseList` (قائمة) فقط، والـ Requests يستقبلها الـ Model Binding.

---

## 12. قواعد مهمة

- **لا نرجع الكيان من الـ API ولا نستقبله** من العميل: نستخدم DTO على الطرفين.
- الـ DTO **يحمل بيانات فقط**: بدون منطق وبدون اعتماد على الـ Database.
- ما يحدده الـ Server (`Id`، وقت الإنشاء) **لا يكون في Request DTO**.
- **تحويل الكيان إلى Response في مكان واحد** (Extension Method) وليس مكررًا داخل كل Action.
- `ToResponse` لا تحتوي منطق عمل (مثل التحقق من تكرار الإيميل)، بل تنسخ الحقول فقط. ذلك المنطق يبقى في الـ Controller.

---

## 13. تجربة المشروع

الملف `requests.http` فيه 27 طلبًا، كلها أعطت الرمز المتوقع عند التجربة، ومنها:

| الطلب | النتيجة |
|---|---|
| `GET /api/students/1` | `200` بدون `password` |
| `POST /api/students` مع `id` و`createdAt` | `201` والحقول الإضافية تُتجاهل |
| `POST /api/students` بإيميل مكرر | `409` |
| `POST /api/students` بجسم مكسور | `400` |
| `GET /api/enrollments` | `200` مع `studentName` و`courseTitle` |
| `POST /api/enrollments` | `201` مع الأسماء |
| `POST /api/identity/login` بكلمة مرور صحيحة | `200` |
| `POST /api/identity/login` بكلمة مرور خاطئة | `401` |

---

## 14. الخلاصة

| المفهوم | المعنى |
|---|---|
| **Entity** | شكل البيانات داخل التطبيق |
| **DTO** | شكل البيانات على حدود الـ API |
| **Response** | ما يرجعه الـ API، ويخفي الحقول الداخلية |
| **Request** | ما يرسله العميل، ويمنع الـ Overposting |
| **Model Binding** | يملأ كائن الـ Request من جسم الطلب |
| **Mapper (Extension Method)** | `ToResponse` تحوّل الكيان إلى Response في مكان واحد |

### جرّب بنفسك
1. أضف حقل `Password` للكيان `Student` وجرّب `GET` بدون DTO، ولاحظ أنه يظهر.
2. أنشئ `StudentResponse` و`CreateStudentRequest` وغيّر الـ Controller، وأعد التجربة.
3. أرسل `POST` بحقول زائدة (`id`، `createdAt`) وتأكد أنها لا تصل.
4. انقل تحويل الكيان إلى `StudentResponse` إلى Extension Method `ToResponse` في كلاس `StudentMapper`.
5. أضف `Phone` إلى `StudentResponse` وعدّل مكانًا واحدًا فقط.
