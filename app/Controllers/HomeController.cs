using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

public class HomeController : Controller
{
    protected readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /**
     * <summary>
     * Returns the index homepage view.
     * </summary>
     */
    [HttpGet("/")]
    public IActionResult Index()
    {
        return View();
    }
}
