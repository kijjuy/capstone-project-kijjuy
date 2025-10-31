using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using app.Services;
using app.Models;

namespace app.Controllers;

public class CheckoutController : Controller
{
    private readonly ILogger<CheckoutController> _logger;
    private readonly ICheckoutService _checkoutService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(ILogger<CheckoutController> logger,
        ICheckoutService checkoutService,
    UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _checkoutService = checkoutService;
        _userManager = userManager;
    }

    [HttpGet("/checkout")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.FindByNameAsync(User!.Identity!.Name!);
        var checkoutSummary = await _checkoutService.GetCheckoutSummaryFromCart(user!.Cart);
        return View("Index", checkoutSummary);
    }
}
