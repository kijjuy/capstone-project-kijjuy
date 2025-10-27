using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using app.Services;

namespace app.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController : Controller 
{
    private readonly ILogger<AdminController> _logger;
    private readonly IProductsService _productsService;

    public AdminController(ILogger<AdminController> logger, IProductsService productsService)
    {
	_logger = logger;
	_productsService = productsService;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
    {
	var products = await _productsService.GetAllProducts();

	return View("Index", products);
    }
}
