using Microsoft.AspNetCore.Mvc;
using app.Services;

namespace app.Controllers;

public class HomeController : Controller
{
    protected readonly ILogger<HomeController> _logger;
    private readonly ICategoriesService _categoriesService;

    public HomeController(
        ILogger<HomeController> logger,
        ICategoriesService categoriesService
        )
    {
        _logger = logger;
        _categoriesService = categoriesService;
    }

    /**
     * <summary>
     * Returns the index homepage view.
     * </summary>
     */
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoriesService.GetAllCategories();

        foreach (var category in categories)
        {
            _logger.LogDebug($"cat id: {category.CategoryId} ; cat name: {category.CategoryName}");
        }

        var cuttingBoard = categories
            .Where(c => c.CategoryName.Equals("Cutting Boards"))
            .FirstOrDefault();

        var charcuterie = categories
            .Where(c => c.CategoryName.Equals("Charcuterie Boards"))
            .FirstOrDefault();

        var choppingBlock = categories
            .Where(c => c.CategoryName.Equals("Chopping Blocks"))
            .FirstOrDefault();


        ViewData["cuttingBoard"] = cuttingBoard == null ? "" : cuttingBoard.CategoryName;
        ViewData["charcuterie"] = charcuterie == null ? "" : charcuterie.CategoryName;
        ViewData["choppingBlock"] = choppingBlock == null ? "" : choppingBlock.CategoryName;

        return View();
    }
}
