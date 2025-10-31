using app.Models;

namespace app.Services;

public interface ICheckoutService
{
    public Task<CheckoutSummaryViewModel> GetCheckoutSummaryFromCart(List<long> cart);
}

public class CheckoutService : ICheckoutService
{
    private readonly ILogger<ICheckoutService> _logger;
    private readonly IProductsService _productsService;
    private const double TAX_RATE = 0.13;

    public CheckoutService(ILogger<ICheckoutService> logger,
        IProductsService productsService)
    {
        _logger = logger;
        _productsService = productsService;
    }

    /**
     * <summary>
     * Gets a list of products from products service from <paramref name="cart"/>.
     * Gets sum of all product prices, and uses that to get tax amount. 
     * <see langword="return"/> CheckoutSummaryViewModel with subtotal, tax amount, and total.
     * </summary>
     */
    public async Task<CheckoutSummaryViewModel> GetCheckoutSummaryFromCart(List<long> cart)
    {
        var products = await _productsService.GetProductsFromCart(cart);
        double subtotal = 0;
        foreach (var product in products)
        {
            subtotal += product.Price;
        }
        _logger.LogDebug($"Summed up products subtotal and got subtotal={subtotal}");

        var taxAmount = Math.Round(subtotal * TAX_RATE, 2);
        var total = Math.Round(subtotal + taxAmount, 2);
        return new CheckoutSummaryViewModel
        {
            Subtotal = String.Format("{0:C2}", subtotal),
            Tax = String.Format("{0:C2}", taxAmount),
            Total = String.Format("{0:C2}", total)
        };
    }
}
