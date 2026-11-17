namespace Entities;

public interface IOperation
{
    Guid Id { get; }
}
public class Operation : IOperation
{
    public Guid Id { get; } = Guid.NewGuid();
}
