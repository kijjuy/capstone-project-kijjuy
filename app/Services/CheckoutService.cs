using app.Models;
using app.Repositories;
using Stripe;
using Stripe.Checkout;

namespace app.Services;

public interface ICheckoutService
{
    public Task<CheckoutSummaryViewModel> GetCheckoutSummaryFromCart(List<long> cart);
    public Task FinalizeCheckout(UserCheckoutDetails checkoutDetails, List<long> cart, String userName);
    public Task<int> CreatePendingOrder(CheckoutInputModel input, IEnumerable<long> cart, String username);
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

    public async Task FinalizeCheckout(UserCheckoutDetails checkoutDetails, List<long> cart, String userName)
    {
        var products = await _cartService.GetProductsFromCart(cart);
        double subtotal = 0;
        foreach (var product in products)
        {
            subtotal += product.Price;
        }

        var taxAmount = Math.Round(subtotal * TAX_RATE, 2);

        //TODO: temporary shipping number, get later at some point
        double shippingAmount = 12.99;
        var total = Math.Round(subtotal + taxAmount + shippingAmount, 2);

        //TODO: temp cc number, get later at some point
        String ccLast4 = "1234";

	checkoutDetails.ShippingName = "test name";
	checkoutDetails.ShippingAddress = "123 main street";


        int orderId = await _ordersRepo.AddOrder(
            userName,
            subtotal,
            taxAmount,
            shippingAmount,
            total,
            checkoutDetails.ShippingAddress,
            checkoutDetails.ShippingName,
            ccLast4,
            DateTime.Now
        );

        foreach (long productId in cart)
        {
            await _ordersRepo.AddOrderProduct(orderId, productId);
            await _productsRepo.MarkProductUnavailable(productId);
        }

	var messageBody = @$"
	    Thank you for your order!
	    Total spent: ${total}
	    ";

	await _emailService.SendEmail(
		userName,
		"Ward4Woods Order",
		messageBody
	    );
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
