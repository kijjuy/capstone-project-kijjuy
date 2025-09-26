using Microsoft.AspNetCore.Mvc;
using app.Repositories;

namespace app.Controllers;

public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IProductsRepository _products;

    public ProductsController(ILogger<ProductsController> logger,
        IProductsRepository productsRepository)
    {
        _logger = logger;
        _products = productsRepository;
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
        var products = _products.GetAllProducts();
        _logger.LogDebug($"size of products: {products.Count}");
        foreach (var product in products)
        {
            var props = product.GetType().GetProperties();
            foreach (var prop in props)
            {
		var val = prop.GetValue()
            }
        }
        return Ok("Product go here\n");
    }
}

