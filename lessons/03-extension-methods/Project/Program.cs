namespace extension_methods;

public class Student
{
    public Student(int id, string name, int age, string address)
    {
        Id = id;
        Name = name;
        Age = age;
        Address = address;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    private int Age { get; set; }
    protected string Address { get; set; }

    public void PrintName()
    {
        Console.WriteLine($"Name: {Name}");
    }

    private string PrintAge()
    {
        return $"Age: {Age}";
    }
}


public static class StudentExtension
{
    public static void PrintId(this Student student)
    {
        Console.WriteLine($"Address: {student.Id}");
    }
}


public static class StringExtension
{
    public static string Truncate( this string text, int max)
    {
            return text.Length <= max
        ? text
        : text[..max];
    }

}

class Program
{
    static void Main(string[] args)
    {
        // Student student = new Student(1, "Ahmed", 30, "Muscat");

        // student.PrintName();
        // student.PrintId();


        string title = "Extension methods in C#";

        Console.WriteLine(title.Truncate(16));
    }
}
