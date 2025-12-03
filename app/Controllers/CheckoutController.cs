using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using app.Services;
using app.Models;
using Stripe.Checkout;
using Stripe;

namespace app.Controllers;

public class CheckoutController : Controller
{
    private readonly ILogger<CheckoutController> _logger;
    private readonly ICheckoutService _checkoutService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICartService _cartService;

    public CheckoutController(
        ILogger<CheckoutController> logger,
        ICheckoutService checkoutService,
        UserManager<ApplicationUser> userManager,
        ICartService cartService,
    IOptions<CheckoutOptions> options
    )
    {
        _logger = logger;
        _checkoutService = checkoutService;
        _userManager = userManager;
        _cartService = cartService;
    }

    /**
     * <summary>
     * Gets the user's cart and returns a viewmodel containing subtotal, tax,
     * and total. This is rendered in an mvc view.
     * </summary>
     */
    [HttpGet("/checkout")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var checkoutSummary = await GetCurrentUserCheckoutSummary(null);


        return View("Index", checkoutSummary);
    }


    [HttpPost("/checkout")]
    [Authorize]
    public async Task<IActionResult> Stripe([FromForm] CheckoutInputModel input)
    {
	if(!ModelState.IsValid) 
	{
	    var summary = await GetCurrentUserCheckoutSummary(input);
	    return View("Index", summary);
	}


        var user = await _userManager.FindByNameAsync(User.Identity!.Name!);
        var cart = user!.Cart;

	int orderId = await _checkoutService.CreatePendingOrder(input, cart, user.UserName);
	user.CurrentOrderId = orderId;

	await _userManager.UpdateAsync(user);

	String url = await _checkoutService.SetupStripe(cart);
        return new StatusCodeResult(303);
    }

    [HttpGet("/checkout/success")]
    [Authorize]
    public IActionResult Success()
    {
        return Json(new { Message = "checkout success!" });
    }

    private async Task<CheckoutSummaryViewModel> GetCurrentUserCheckoutSummary(CheckoutInputModel? prevInput) 
    {

        var user = await _userManager.FindByNameAsync(User!.Identity!.Name!);
	var checkoutSummary = await _checkoutService.GetCheckoutSummaryFromCart(user!.Cart);

	if(prevInput != null) 
	{
	    checkoutSummary.Input = prevInput;
	    return checkoutSummary;
	}

	var input = new CheckoutInputModel 
	{
	    Name = user.UserName, 
	    Address = "",
	};

	if(user.Address != null && !user.Address.Equals("")) 
	{
	    input.Address = user.Address;
	}

	checkoutSummary.Input = input;

	return checkoutSummary;
    }
}
