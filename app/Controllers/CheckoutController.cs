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

    /**
     * <summary>
     * Gets the user's cart and returns a viewmodel containing subtotal, tax,
     * and total. This is rendered in an mvc view.
  navToCheckout()   * </summary>
     */
    [HttpGet("/checkout")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.FindByNameAsync(User!.Identity!.Name!);
        var checkoutSummary = await _checkoutService.GetCheckoutSummaryFromCart(user!.Cart);
        return View("Index", checkoutSummary);
    }

    /**
     * <summary>
     * Completes the checkout and marks all of the products from the cart as 
     * not available.
     * </summary>
     */
    [HttpGet("/checkout/complete")]
    [Authorize]
    public async Task<IActionResult> CompleteCheckout([FromQuery] UserCheckoutDetails checkoutDetails)
    {
        //TODO: wait for stripe webhook here to confirm purchase
        var user = await _userManager.FindByNameAsync(User.Identity!.Name!);

	await _checkoutService.FinalizeCheckout(checkoutDetails, user.Cart, user.UserName);

        user.Cart = new List<long>();
        await _userManager.UpdateAsync(user);


        return RedirectToAction(controllerName: "Home", actionName: "Index");
    }
}
