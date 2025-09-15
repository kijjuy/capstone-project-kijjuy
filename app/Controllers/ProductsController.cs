using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("hello")]
    public IActionResult HelloWorld()
    {
        _logger.LogInformation("Hit Hello World method");
        return Ok("Hello");
    }
}

