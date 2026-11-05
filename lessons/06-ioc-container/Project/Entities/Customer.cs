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


public class Customer
{
    // Tited Coupled
    //CashPayment cashPayment = new CashPayment();
    public Customer(IPayment payment)
    {
        this.Payment = payment;
    }

    public IPayment Payment;

    public string Buy()
    {
        return $"buying using {Payment.Pay()}";
    }
}