using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using app.Models;
using app.Services;

namespace app.Controllers;

public class CartController : Controller
{
    private readonly ILogger<CartController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProductsService _productsSercice;

    public CartController(ILogger<CartController> logger,
        UserManager<ApplicationUser> userManager,
        IProductsService productsSercice)
    {
        _logger = logger;
        _userManager = userManager;
        _productsSercice = productsSercice;
    }

    /**
     * <summary>
     * Returns the cart Index razor view.
     *
     * User must be logged in.
     * </summary>
     */
    [Authorize]
    [HttpGet("/cart")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.FindByNameAsync(User.Identity.Name);
	_logger.LogDebug("cart items:");
	foreach(long item in user.Cart)
	{
	    _logger.LogDebug($"{item}");
	}

	var products = new List<ProductViewModel>();
	try {
	    products = await _productsSercice.GetProductsFromCart(user.Cart);
	} catch(Exception e) {
	    _logger.LogWarning($"Error getting items from cart. Fixing cart.\n{e.StackTrace}");
	    FixCart(user);

	}
        return View("Index", products);
    }

    /**
     * <summary>
     * Adds a product with id=productId to the user's cart.
     *
     * User must be logged in.
     * </summary>
     */
    [Authorize]
    [HttpPost("/api/cart/{productId}")]
    public async Task<IActionResult> AddToCart(int productId)
    {
        _logger.LogDebug($"Hit AddToCart method with productId={productId}");

	if(productId < 1)
	{
	    _logger.LogWarning($"Error adding product because Id was less than 1.");
	    return BadRequest(new { message = "ProductId must be greater than 0" });
	}

        var user = await _userManager.FindByNameAsync(User.Identity.Name);
        _logger.LogDebug($"user cart length before adding: {user.Cart.Count()}");

        if (user.Cart.Contains(productId))
        {
	    _logger.LogInformation("Product already in cart.");
            return BadRequest(new { message = "Product is already in your cart." });
        }

        bool isAdded = await _productsSercice.AddProductToCart(user.Cart, productId);

        if (!isAdded)
        {
	    _logger.LogWarning($"Error adding to cart with productId={productId}");
            return new StatusCodeResult(500);
        }

        _logger.LogDebug($"user cart length after adding: {user.Cart.Count()}");

        await _userManager.UpdateAsync(user);

        return Ok();
    }

    /**
     * <summary>
     * Resets the user's cart with a new empty cart.
     * </summary>
     */
    private async Task FixCart(ApplicationUser user) 
    {
	user.Cart = new List<long>();
	await _userManager.UpdateAsync(user);
    }
}
