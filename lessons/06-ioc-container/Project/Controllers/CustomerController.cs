using Entities;
using Microsoft.AspNetCore.Mvc;

namespace DependencyInjection.Controllers;

[ApiController]
[Route("customer")]
public class CustomerController(ICustomer customer) : ControllerBase
{
    //private ICustomer Customer = customer;
    

    [HttpGet]
    public IActionResult GetCustomers()
    {
        return Ok(customer.Buy());
    }
}