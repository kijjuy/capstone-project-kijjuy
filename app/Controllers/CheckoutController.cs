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


    /**
     * <summary>
     * Sets up stripe checkout and redirects the user to that page.
     * </summary>
     */
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

	String url = await _checkoutService.SetupStripe(cart, orderId);
	Response.Redirect(url);
        return new StatusCodeResult(303);
    }

    [HttpPost("/checkout/webhook")]
    public async Task<IActionResult> StripeWebhook() 
    {
	var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
	//_logger.LogDebug(json);
	Event stripeEvent;

	try 
	{
	    stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _webhookSecret);
	} catch(Exception e) {
	    _logger.LogError("Error getting stripe event from request headers");
	    _logger.LogError(e.Message);
	    return BadRequest();
	}

	if(stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
	{
	    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

	    if(session == null) 
	    {
		_logger.LogError("Webhook had null session");
		return BadRequest();
	    }

	    String orderIdStr = session.ClientReferenceId;
	    int orderId;
	    try {
		orderId = Int32.Parse(orderIdStr);
	    } catch(Exception e) {
		_logger.LogError("Error parsing order id from session.");
		_logger.LogError(e.Message);
		return BadRequest();
	    }

	    double shipping;
	    try {
		shipping = Math.Round((double)((long)session.ShippingCost.AmountTotal) / 100, 2);
	    } catch(Exception e) {
		_logger.LogError("ShippingCost.AmountTotal was not found in the stripe session.");
		_logger.LogError(e.Message);
		return BadRequest();
	    }

	    double total;
	    try
	    {
		total = Math.Round((double)((long)session.AmountTotal!) / 100, 2);
	    } catch(Exception e)
	    {
		_logger.LogError("AmountTotal was not found in the stripe session.");
		return BadRequest();
	    }

	    await _checkoutService.CompleteCheckout(orderId, shipping, total);
	} 

	return Ok();
    }


    /**
     * <summary>
     * Endpoint stripe redirects to when order is complete.
     * </summary>
     */
    [HttpGet("/checkout/success")]
    [Authorize]
    public async Task<IActionResult> Success()
    {
	var user = await _userManager.FindByNameAsync(User.Identity.Name);
	user.CurrentOrderId = 0;
	user.Cart = new List<long>();
	_logger.LogDebug("updating using with empty cart and 0 order id");
	await _userManager.UpdateAsync(user);
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
