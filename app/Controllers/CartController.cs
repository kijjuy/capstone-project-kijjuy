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
	List<ProductViewModel> products = await _productsSercice.GetProductsFromCart(user.Cart);

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
    [HttpPost("/api/cart/{id}")]
    public async Task<IActionResult> AddToCart(int productId)
    {
        _logger.LogDebug("Hit AddToCart method");
        var product = await _productsSercice.GetProductById(productId);

        if (product == null)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            _logger.LogWarning($"user with id={user.Id} tried to add product with id={productId}. Product not found.");
            return NotFound(new { message = $"Count not find product with id={productId}" });
        }
        throw new NotImplementedException("Adding to cart not implemented");
    }
}
