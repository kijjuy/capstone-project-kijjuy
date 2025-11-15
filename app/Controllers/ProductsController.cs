using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using app.Services;
using app.Models;
using app.Mappers;

namespace app.Controllers;

public class ProductsController : Controller
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IProductsService _productsService;
    private readonly ICategoriesService _categoriesService;
    private readonly IProductMapper _productMapper;

    public ProductsController(
        ILogger<ProductsController> logger,
        IProductsService productsService,
        ICategoriesService categoriesService,
        IProductMapper productMapper
       )
    {
        _logger = logger;
        _productsService = productsService;
        _categoriesService = categoriesService;
        _productMapper = productMapper;
    }


    /**
     * <summary>
     * Returns the products Index razor view that contains a list of all products
     * </summary>
     */
    [HttpGet("/products")]
    public async Task<IActionResult> Index(
        [FromQuery] String nameFilter,
        [FromQuery] String categoryNameFilter,
        [FromQuery] double priceMin,
        [FromQuery] double priceMax)
    {
        _logger.LogDebug($"nameFilter: {nameFilter}");
        _logger.LogDebug($"categoryNameFilter: {categoryNameFilter}");
        _logger.LogDebug($"priceMin: {priceMin}");
        _logger.LogDebug($"priceMax: {priceMax}");

        var baseProducts = await _productsService.GetAllProducts();
        var products = baseProducts
            .Select(async p => await _productMapper.IntoViewModelWithImages(p))
            .Select(t => t.Result);


        if (nameFilter != null && !nameFilter.Equals(String.Empty))
        {
            products = products.Where(p => p.ProductName.Contains(nameFilter))
            .ToList();
        }
        if (categoryNameFilter != null && !categoryNameFilter.Equals(String.Empty))
        {
            products = products.Where(p => p.CategoryName.Contains(categoryNameFilter))
            .ToList();
        }
        if (priceMin != 0)
        {
            products = products.Where(p => p.Price > priceMin)
            .ToList();
        }
        if (priceMax != 0)
        {
            products = products.Where(p => p.Price < priceMax)
            .ToList();
        }

        return View("Index", products);
    }

    /**
     * <summary>
     * Returns the datails razor View with the product matching the id. If id is invalid,
     * return BadRequest result. If product is not found, return NotFound result.
     * </summary>
     */
    [HttpGet("/products/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        if (id < 1)
        {
            return BadRequest(new { message = "Product id must be above 0." });
        }

        Product product;
        try
        {
            product = await _productsService.GetProductById(id);
        }
        catch (Exception e)
        {
            _logger.LogError($"Unhandled exception when trying to get product by id. Error={e.Message}\n" +
            $"{e.StackTrace}");
            return new StatusCodeResult(500);
        }
        if (product == null || product == null)
        {
            return NotFound();
        }

        var productViewModel = await _productMapper.IntoViewModelWithImages(product);


        return View("Details", productViewModel);

    }


    /**
     * <summary>
     * Binds the value id from the url params then deletes a single product with that matching id.
     * Requires user to have admin role.
     * </summary>
     */
    [Authorize(Roles = "Admin")]
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
     * <summary>
     * Returns the Create razor view.
     *
     * Requires the user to have admin role.
     * </summary>
     */
    [Authorize(Roles = "Admin")]
    [HttpGet("/products/create")]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoriesService.GetAllCategories();
        ViewData["Categories"] = new SelectList(categories, "CategoryId", "CategoryName");

        return View();
    }

    /**
     * <summary>
     * Gets product details from Create razor page and uses them to add a new product 
     * to the database. If the model state is invalid, return to the create page. 
     * If successful, redirect to the new product.
     *
     * Requires the user to have Admin role.
     * </summary>
     */
    [Authorize(Roles = "Admin")]
    [HttpPost("/products/create")]
    public async Task<IActionResult> Create(CreateProductModel product)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning($"Error with model state when creating new product from form.");
            LogModelErrors();
            return View("Create");
        }

        long newId;
        try
        {
            newId = await _productsService.CreateProduct(product);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error when creating product. Error={e.Message}.\n{e.StackTrace}");
            return new StatusCodeResult(500);
        }
        _logger.LogInformation($"Created new product with id={newId}");
        return RedirectToAction("Details", new { id = newId });
    }

    /**
     * <summary>
     * Returns the Update razor view.
     *
     * Requires te user to have admin role.
     * </summary>
     */
    [Authorize(Roles = "Admin")]
    [HttpGet("/products/update/{id}")]
    public async Task<IActionResult> Update(int id)
    {
        var categories = await _categoriesService.GetAllCategories();
        ViewData["Categories"] = new SelectList(categories, "CategoryId", "CategoryName");

        var baseProduct = await _productsService.GetProductById(id, true);
        var product = await _productMapper.IntoViewModel(baseProduct);
        ViewData["CurrentProduct"] = product;
        return View();
    }

    /**
     * <summary>
     * Gets product details from the Update razor page and uses them to update 
     * the product with the Id in the route param.
     *
     * Requires the user to have Admin role.
     * </summary>
     */
    [Authorize(Roles = "Admin")]
    [HttpPost("/products/update/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateProductModel product, int id)
    {
        _logger.LogDebug("Hit update put endpoint");
        if (!ModelState.IsValid)
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

