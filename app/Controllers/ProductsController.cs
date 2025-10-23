using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using app.Services;
using app.Models;
using System.Text.Json;

namespace app.Controllers;

public class ProductsController : Controller
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IProductsService _productsService;
    private readonly ICategoriesService _categoriesService;

    public ProductsController(ILogger<ProductsController> logger,
       IProductsService productsService,
       ICategoriesService categoriesService)
    {
        _logger = logger;
        _productsService = productsService;
	_categoriesService = categoriesService;
    }

    [HttpGet("/api/hello")]
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
    [HttpGet("/api/products")]
    public async Task<IActionResult> GetAllProducts()
    {
        _logger.LogDebug("Hit GetAllProducts method.");
        var products = await _productsService.GetAllProducts();
        _logger.LogDebug($"size of products: {products.Count}");

        string json = JsonSerializer.Serialize(products);
        return Ok(json);
    }

    [HttpGet("/products")]
    public async Task<IActionResult> Index()
    {
	var products = await _productsService.GetAllProducts();

	return View("Index", products);
    }

    /**
     * <summary>
     * Binds the value id from the url params then finds a product with that matching
     * id or returns not found.
     * </summary>
     */
    [HttpGet("/api/products/{id}", Name = "GetProductById")]
    public async Task<IActionResult> GetProductById(int id)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid id when trying to get product");
            return BadRequest(new { message = "Must provide a valid Id when trying to get a product." });
        }

        _logger.LogDebug("Hit GetProductById");
        var product = await _productsService.GetProductById(id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with id={id} could not be found." });
        }

        return Ok(product);
    }

    [HttpGet("/products/{id}")]
    public async Task<IActionResult> Details(int id) {
	var product = await _productsService.GetProductById(id);

	if (product == null) 
	{
	    return NotFound();
	}

	return View("Details", product);
    }

    /**
     * <summary>
     * Binds the value id from the url params then deletes a single product with that matching id.
     * </summary>
     */
    [HttpDelete("/api/products/{id}")]
    public IActionResult DeleteProduct(int id)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid id when trying to delete product.");
            return BadRequest(new { message = "Must provide a valid id when deleting a product." });
        }
        try
        {
            _productsService.DeleteProduct(id);
        }
        catch (Exception e)
        {
            if (e.GetType().Equals(typeof(BadSqlResultException)))
            {
                return NotFound(new { message = $"Product with id={id} could not be found." });
            }
            else
            {
                return new StatusCodeResult(500);
            }
        }
        return NoContent();
    }

    /**
     * Binds form data to CreateProductModel then uses that to create a 
     * new product in the database and returns the location of that new product.
     */
    [HttpPost("/api/products")]
    public IActionResult CreateProduct([FromForm] CreateProductModel newProduct)
    {
        if (!ModelState.IsValid)
        {
            LogModelErrors();
            _logger.LogWarning($"Attempted to create product with empty values.");
            return BadRequest(new { message = "Must provide valid inputs for name, categoryId, price, and description." });
        }

        _logger.LogDebug("hit create product endpoint");
        try
        {
            int newId = _productsService.CreateProduct(newProduct);
            return CreatedAtRoute(nameof(GetProductById), new { id = newId }, new { id = newId });
        }
        catch (Exception e)
        {
            _logger.LogError($"Unhandled Exception thrown when creating product. Err={e.Message}");
            return new StatusCodeResult(500);
        }
    }

    [HttpGet("/products/create")]
    public async Task<IActionResult> Create() 
    {
	var categories = await _categoriesService.GetAllCategories();
	ViewData["Categories"] = new SelectList(categories, "CategoryId", "CategoryName");

	return View();
    }

    [HttpPost("/products/create")]
    public async Task<IActionResult> Create(CreateProductModel product) 
    {
	if(!ModelState.IsValid) 
	{
	    _logger.LogWarning($"Error with model state when creating new product from form.");
	    LogModelErrors();
	    return View("Create");
	}

	long newId = _productsService.CreateProduct(product);
	_logger.LogInformation($"Created new product with id={newId}");
	return RedirectToAction("Details", new { id = newId });
    }

    [HttpGet("/products/update/{id}")]
    public async Task<IActionResult> Update(int id) 
    {
	var categories = await _categoriesService.GetAllCategories();
	ViewData["Categories"] = new SelectList(categories, "CategoryId", "CategoryName");

	var product = await _productsService.GetProductById(id);
	ViewData["CurrentProduct"] = product;
	return View();
    }

    [HttpPost("/products/update/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateProductModel product, int id)
    {
	_logger.LogDebug("Hit update put endpoint");
	if(!ModelState.IsValid)
	{
	    _logger.LogWarning($"Error with model state when updating product with id={id}");
	    LogModelErrors();
	    return View();
	}

	await _productsService.UpdateProduct(product, id);
	return RedirectToAction("Details", new { id = id });
    }


    /**
     * <summary>
     * Logs any errors for each of the keys within the model state
     * </summary>
     */
    private void LogModelErrors()
    {
        foreach (var key in ModelState.Keys)
        {
            var errors = ModelState[key].Errors;
            foreach (var err in errors)
            {
                _logger.LogWarning($"Model error on key={key}: err={err}; message={err.ErrorMessage}");
            }
        }
    }
}

