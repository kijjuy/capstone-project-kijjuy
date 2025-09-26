using Microsoft.AspNetCore.Mvc;
using app.Repositories;

namespace app.Controllers;

public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IProductsRepository _product;

    public ProductsController(ILogger<ProductsController> logger,
        IProductsRepository productsRepository)
    {
        _logger = logger;
        _product = productsRepository;
    }

    [HttpGet("hello")]
    public IActionResult HelloWorld()
    {
        _logger.LogDebug("Hit Hello World method");
        return Ok("Hello\n");
    }

    [HttpGet("products")]
    public IActionResult GetAllProducts()
    {
        _logger.LogDebug("Hit GetAllProducts method.");
        _product.GetAllProducts();
        return Ok("Product go here\n");
    }
}

