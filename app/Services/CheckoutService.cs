using app.Models;
using app.Repositories;
using Stripe;
using Stripe.Checkout;

namespace app.Services;

public interface ICheckoutService
{
    public Task<CheckoutSummaryViewModel> GetCheckoutSummaryFromCart(List<long> cart);
    public Task<int> CreatePendingOrder(CheckoutInputModel input, IEnumerable<long> cart, String username);
    public Task<String> SetupStripe(List<long> cart);
}

public class CheckoutService : ICheckoutService
{
    private readonly ILogger<ICheckoutService> _logger;
    private readonly IProductsService _productsService;
    private readonly IProductsRepository _productsRepo;
    private readonly ICartService _cartService;
    private readonly IOrdersRepository _ordersRepo;
    private readonly IEmailService _emailService;
    private const double TAX_RATE = 0.13;

    public CheckoutService(
        ILogger<ICheckoutService> logger,
        IProductsService productsService,
        IProductsRepository productsRepository,
        ICartService cartService,
        IOrdersRepository ordersRepo,
	IEmailService emailService
    )
    {
        _logger = logger;
        _productsService = productsService;
        _productsRepo = productsRepository;
        _cartService = cartService;
        _ordersRepo = ordersRepo;
	_emailService = emailService;
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
	var cartTotals = await GetCartTotalValues(cart);

        return new CheckoutSummaryViewModel
        {
            Subtotal = String.Format("{0:C2}", cartTotals.Subtotal),
            Tax = String.Format("{0:C2}", cartTotals.Tax),
            Total = String.Format("{0:C2}", cartTotals.TotalNoShipping)
        };
    }

    public async Task<int> CreatePendingOrder(CheckoutInputModel input, IEnumerable<long> cart, String username)
    {
	var cartValues = await GetCartTotalValues(cart);
	int result = await _ordersRepo.CreatePendingOrder(input.Name, input.Address, cartValues.Subtotal,
		cartValues.Tax, username);

	if(result < 1) 
	{
	    throw new BadSqlResultException($"Got bad id when inserting pending order. Got id={result}");
	}

	return result;
    }

    /** 
     * <summary>
     * Loops through all products in cart and sets up a stripe checkout
     * page from them. 
     *
     * <see langword="return"/> Url to stripe checkout page
     * </summary>
     */
    public async Task<String> SetupStripe(List<long> cart) 
    {
        _logger.LogDebug($"sizeof cart: {cart.Count}");

	var lineItems = await GetLineItemsFromCart(cart);

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

	return session.Url;
    }

    /**
     * <summary>
     * Creates list of line items from a user's cart.
     * </summary>
     */
    private async Task<List<SessionLineItemOptions>> GetLineItemsFromCart (List<long> cart) {
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
	return lineItems;
    }

    private async Task<CartTotalValues> GetCartTotalValues(IEnumerable<long> cart) 
    {
        var products = await _cartService.GetProductsFromCart(cart.ToList());
        double subtotal = 0;
        foreach (var product in products)
        {
            subtotal += product.Price;
        }
        _logger.LogDebug($"Summed up products subtotal and got subtotal={subtotal}");

        var taxAmount = Math.Round(subtotal * TAX_RATE, 2);
        var total = Math.Round(subtotal + taxAmount, 2);

	return new CartTotalValues {
	    Subtotal = subtotal,
	    Tax = taxAmount,
	    TotalNoShipping = total,
	};
    }

    private class CartTotalValues 
    {
	public double Subtotal { get; set; }
	public double Tax { get; set; }
	public double TotalNoShipping { get; set; }
    }

}
