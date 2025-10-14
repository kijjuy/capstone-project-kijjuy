using Microsoft.AspNetCore.Mvc;
using app.Services;
using app.Models;
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

    /**
     * <summary>
     * Gets all products from the database.
     * </summary>
     */
    [HttpGet("products")]
    public IActionResult GetAllProducts()
    {
        _logger.LogDebug("Hit GetAllProducts method.");
        var products = _productsService.GetAllProducts();
        _logger.LogDebug($"size of products: {products.Count}");

        string json = JsonSerializer.Serialize(products);
        return Ok(json);
    }

    /**
     * <summary>
     * Binds the value id from the url params then deletes a single product with that matching id.
     * </summary>
     */
    [HttpDelete("products/{id}")]
    public IActionResult DeleteProduct(int id) {
	if(!ModelState.IsValid) {
	    _logger.LogWarning("Invalid id when trying to delete product.");
	    return BadRequest(new { message = "Must provide a valid id when deleting a product." });
	}
	_productsService.DeleteProduct(id);
	return NoContent();
    }

    /**
     * Binds form data to CreateProductModel then uses that to create a 
     * new product in the database and returns the location of that new product.
     */
    [HttpPost("products")]
    public IActionResult CreateProduct([FromForm]CreateProductModel newProduct) {
	if(!ModelState.IsValid) {
	    LogModelErrors();
	    _logger.LogWarning($"Attempted to create product with empty values.");
	    return BadRequest(new { message = "Must provide valid inputs for name, categoryId, price, and description." });
	}

	_logger.LogDebug("hit create product endpoint");
	try {
	    int newId = _productsService.CreateProduct(newProduct);
	    return CreatedAtRoute(nameof(GetProductById), new {id = newId}, new { id = newId });
	} catch(Exception e) {
	    _logger.LogError($"Unhandled Exception thrown when creating product. Err={e.Message}");
	    return new StatusCodeResult(500);
	}
    }

    private void LogModelErrors() {
	    foreach(var key in ModelState.Keys) {
		 var errors = ModelState[key].Errors;
		 foreach(var err in errors) {
		     _logger.LogWarning($"Model error on key={key}: err={err}; message={err.ErrorMessage}");
		 }
	    }
    }
}

