using Entities;
using Microsoft.AspNetCore.Mvc;

namespace DependencyInjection.Controllers;

[ApiController]
[Route("customer")]
public class CustomerController : ControllerBase
{
    Customer customer = new Customer(new GooglePayment());


    [HttpGet]
    public IActionResult GetCustomers()
    {
        return Ok(customer.Buy());
    }
}