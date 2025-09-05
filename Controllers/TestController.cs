using Microsoft.AspNetCore.Mvc;
using Cüzdan_Uygulaması.Exceptions;

namespace Cüzdan_Uygulaması.Controllers;

public class TestController : Controller
{
    [HttpGet]
    public IActionResult TestBusinessLogicException()
    {
        throw new BusinessLogicException("This is a test business logic exception");
    }

    [HttpGet]
    public IActionResult TestValidationException()
    {
        throw new ValidationException("TestField", "This field is invalid for testing purposes");
    }

    [HttpGet]
    public IActionResult TestResourceNotFoundException()
    {
        throw new ResourceNotFoundException("TestResource", "123");
    }

    [HttpGet]
    public IActionResult TestInsufficientFundsException()
    {
        throw new InsufficientFundsException(1000.00m, 500.00m, "Test Account");
    }

    [HttpGet]
    public IActionResult TestGenericException()
    {
        throw new InvalidOperationException("This is a test generic exception");
    }

    [HttpGet]
    public IActionResult TestApiException()
    {
        Response.Headers["Accept"] = "application/json";
        throw new BusinessLogicException("API test exception");
    }
}