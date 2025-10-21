using Microsoft.AspNetCore.Mvc;
using app.Services;

namespace app.Controllers;

public class CategoriesController : ControllerBase
{
    private readonly ILogger<CategoriesController> _logger;
    private readonly ICategoriesService _categoriesService;

    public CategoriesController(ILogger<CategoriesController> logger, ICategoriesService categoriesService)
    {
        _logger = logger;
        _categoriesService = categoriesService;

    }

    [HttpGet("/api/categories")]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoriesService.GetAllCategories();
        return Ok(categories);
    }
}
