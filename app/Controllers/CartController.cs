using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using app.Models;
using app.Services;
using app.Mappers;

namespace app.Controllers;

public class CartController : Controller
{
    private readonly ILogger<CartController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICartService _cartService;
    private readonly IProductMapper _productMapper;

    public CartController(
        ILogger<CartController> logger,
        UserManager<ApplicationUser> userManager,
        ICartService cartService,
	IProductMapper productMapper
    )
    {
        _logger = logger;
        _userManager = userManager;
        _cartService = cartService;
        _productMapper = productMapper;
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

	_logger.LogInformation($"User with name={user.UserName} is viewing their cart.");

        CartViewModel products = new CartViewModel {
	    Products = new List<app.Models.ProductViewModelWithImages>(),
	    Subtotal = 0,
	};

        try
        {
            products = await _cartService.GetCartViewModelFromCart(user.Cart);
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Error getting items from cart. Fixing cart.\n{e.StackTrace}");
            FixCart(user);
	    return Redirect("/");
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

        if (productId < 1)
        {
            _logger.LogWarning($"Error adding product because Id was less than 1.");
            return BadRequest(new { message = "ProductId must be greater than 0" });
        }


	
	ApplicationUser user = new ApplicationUser();
	try 
	{
	    user = await _userManager.FindByNameAsync(User.Identity.Name);
	    if (user == null) 
	    {
		throw new Exception("Error getting user from FindByNameAsync");
	    }
	} 
	catch(Exception e) 
	{
	    _logger.LogWarning(e, "Error getting user. Perhaps they are not logged in?");
	    Response.Redirect("/Identity/Account/Login");
	    return StatusCode(303);
	}
        _logger.LogDebug($"user cart length before adding: {user.Cart.Count()}");



        if (user.Cart.Contains(productId))
        {
            _logger.LogInformation("Product already in cart.");
            return BadRequest(new { message = "Product is already in your cart." });
        }

        bool isAdded = await _cartService.AddProductToCart(user.Cart, productId);

        if (!isAdded)
        {
            _logger.LogWarning($"Error adding to cart with productId={productId}");
            return new StatusCodeResult(500);
        }

	_logger.LogInformation($"User with name={user.UserName} added product with id={productId} to their cart.");

        _logger.LogDebug($"user cart length after adding: {user.Cart.Count()}");

        await _userManager.UpdateAsync(user);

        return Ok();
    }

    /**
     * <summary>
     * Attempts to remove an item with value of <paramref name="productId"/> from the user's cart.
     * If not deleted, <see langword="return"/> BadRequest (400).
     * If deleted, <see langword="return"/> Ok (200).
     * </summary>
     */
    [Authorize]
    [HttpDelete("/api/cart/{productId}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var user = await _userManager.FindByNameAsync(User.Identity.Name);

        bool isRemoved = _cartService.RemoveFromCart(user.Cart, productId);
        if (!isRemoved)
        {
            _logger.LogWarning($"User tried to remove cart item with id={productId}. Item was not in cart.");
            return BadRequest(new { message = "Could not remove item from cart. Item not found." });
        }

        await _userManager.UpdateAsync(user);
        _logger.LogDebug($"removed item from cart with id={productId}");
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
