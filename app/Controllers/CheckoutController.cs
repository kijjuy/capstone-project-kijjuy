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

        var user = await _userManager.FindByNameAsync(User.Identity!.Name!);
        var cart = user!.Cart;

        _logger.LogDebug($"sizeof cart: {cart.Count}");

        var products = await _cartService.GetProductsFromCart(cart);
        _logger.LogDebug($"sizeof products: {products.Count}");

        var lineItems = new List<SessionLineItemOptions>();

        foreach (var product in products)
        {

            var productData = new SessionLineItemPriceDataProductDataOptions
            {
                Name = product.Name,
                Description = product.Description,
                TaxCode = "txcd_99999999",
            };

            var lineItem = new SessionLineItemOptions
            {
                Quantity = 1,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "CAD",
                    ProductData = productData,
                    UnitAmount = (int)(product.Price * 100),
                }
            };
            lineItems.Add(lineItem);
        }

        _logger.LogDebug($"sizeof lineItems: {lineItems.Count}");

        var options = new SessionCreateOptions
        {
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = "http://localhost:8080/checkout/success"
        };

        var client = new StripeClient("sk_test_51PPwFrDRzObLxTqvUVjLH4DmU8RyHUl1srpx5lpW45G7xYBctZSRCWufCKrn3h3mGmWVMuYMz4pHdNkBz6pFvsUm00cYFlK9Kr");


        var service = new SessionService(client);
        var session = service.Create(options);

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
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
