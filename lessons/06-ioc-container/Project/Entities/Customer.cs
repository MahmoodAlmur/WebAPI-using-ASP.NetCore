namespace Entities;


public interface IPayment
{
    string Pay();
}


public class CashPayment : IPayment
{
    public string Pay()
    {
        return "Cash Payment";
    }
}

public class CardPayment : IPayment
{
    public string Pay()
    {
        return "Card Payment";
    }
}


public class GooglePayment : IPayment
{
    public string Pay()
    {
        return "Google Payment";
    }
}


public interface ICustomer
{
    string Buy();
}


public class Customer(IPayment payment) : ICustomer
{
    // Tited Coupled
    //CashPayment cashPayment = new CashPayment();
    // public Customer(IPayment payment)
    // {
    //     this.Payment = payment;
    // }
    // public IPayment Payment;

    public string Buy()
    {
        return $"buying using {payment.Pay()}";
    }
}