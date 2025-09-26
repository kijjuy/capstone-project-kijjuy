using Microsoft.AspNetCore.Mvc;
using app.Repositories;
using app.Services;

namespace app.Controllers;

public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IProductsService _productsService;

    public ProductsController(ILogger<ProductsController> logger,
       IProductsService productsService)
    {
        _logger = logger;
        _productsService = productsService;
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
        var products = _productsService.GetAllProducts();
        _logger.LogDebug($"size of products: {products.Count}");
        foreach (var product in products)
        {
            var props = product.GetType().GetProperties();
            foreach (var prop in props)
            {
                var val = prop.GetValue(product);
                _logger.LogDebug($"product info: {val}");
            }
        }
        return Ok("Product go here\n");
    }
}

