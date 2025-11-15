using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using app.Services;
using app.Mappers;

namespace app.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly IProductsService _productsService;
    private readonly IProductMapper _mapper;

    public AdminController(
        ILogger<AdminController> logger,
        IProductsService productsService,
        IProductMapper mapper
        )
    {
        _logger = logger;
        _productsService = productsService;
        _mapper = mapper;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
    {
        var baseProducts = await _productsService.GetAllProducts(shouldGetUnavailable: true);
        var products = baseProducts
            .Select(async p => await _mapper.IntoViewModel(p))
            .Select(t => t.Result);

        return View("Index", products);
    }
}
