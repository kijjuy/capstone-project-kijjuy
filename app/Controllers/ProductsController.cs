using Microsoft.AspNetCore.Mvc;
using app.Services;
using System.Text.Json;

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

        string json = JsonSerializer.Serialize(products);
        return Ok(json);
    }

    [HttpDelete("products/{id}")]
    public IActionResult DeleteProduct(int id) {
	_productsService.DeleteProduct(id);
	return NoContent();
    }

}

