using app.Models;

namespace app.Services;

public interface ICartService
{
    public Task<List<ProductViewModelWithImages>> GetProductsFromCart(List<long> cart);
    public Task<CartViewModel> GetCartViewModelFromCart(List<long> cart);
    public Task<bool> AddProductToCart(List<long> cart, int productId);
    public bool RemoveFromCart(List<long> cart, int productId);
}

public class CartService : ICartService
{
    private readonly ILogger<ICartService> _logger;
    private readonly IProductsService _productsService;

    public CartService(ILogger<ICartService> logger,
        IProductsService productsService)
    {
        _logger = logger;
        _productsService = productsService;
    }

    /**
     * <summary>
     * Loops through product ids in <paramref name="cart"/> and gets each product, adding them to
     * a list. Returns the list of those products.
     * </summary>
     */
    public async Task<List<ProductViewModelWithImages>> GetProductsFromCart(List<long> cart)
    {
        var products = new List<ProductViewModelWithImages>();
        foreach (int productId in cart)
        {
            var product = await _productsService.GetProductByIdWithImages(productId);
            if (product == null)
            {
                _logger.LogWarning($"Cart item with productId={productId} was not found. Now removing it from the cart.");
                cart.Remove(productId);
                continue;
            }
            products.Add(product);
        }
        return products;
    }

    /**
     * <summary>
     * Gets view models from cart, then gets the sum of their prices and creates a new CartViewModel 
     * out of view models list and sum of prices for subtotal.
     * </summary>
     */
    public async Task<CartViewModel> GetCartViewModelFromCart(List<long> cart)
    {
        var products = await GetProductsFromCart(cart);
        double subtotal = 0;
        foreach (var product in products)
        {
            subtotal += product.InternalModel.Price;
        }

        return new CartViewModel
        {
            Products = products,
            Subtotal = subtotal
        };
    }

    /**
     * <summary>
     * Takes in <paramref name="cart"/>, checks if the product exists, then adds it to <paramref name="cart"/>.
     * <see langword="return"/> <see langword="true"/> if product is added. 
     * <see langword="return"/> <see langword="false"/> if product was not added.
     * </summary>
     */
    public async Task<bool> AddProductToCart(List<long> cart, int productId)
    {
        _logger.LogDebug($"Count of cart before adding: ${cart.Count()}");
        var product = await _productsService.GetProductById(productId);
        if (product == null)
        {
            _logger.LogWarning($"Cound not find product with id={productId}. Returning same cart.");
            return false;
        }

        cart.Add(productId);
        _logger.LogDebug($"Count of cart after adding: ${cart.Count()}");
        return true;
    }

    /**
     * <summary>
     * Checks if <paramref name="productId"/> is inside of <paramref name="cart"/>. If not, <see langword="return"/>
     * <see langword="false"/>. If yes, <see langword="return"/> <see langword="true"/>
     * </summary>
     */
    public bool RemoveFromCart(List<long> cart, int productId)
    {
        return cart.Remove(productId);
    }
}
