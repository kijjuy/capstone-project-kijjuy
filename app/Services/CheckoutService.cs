using app.Models;
using app.Repositories;
using System.Text;
using Stripe;
using Stripe.Checkout;

namespace app.Services;

public interface ICheckoutService
{
    public Task<CheckoutSummaryViewModel> GetCheckoutSummaryFromCart(List<long> cart);
    public Task<int> CreatePendingOrder(CheckoutInputModel input, IEnumerable<long> cart, String username);
    public Task<String> SetupStripe(List<long> cart, int orderId);
    public Task CompleteCheckout(int orderId, double shipping, double total);
}

public class CheckoutService : ICheckoutService
{
    private readonly ILogger<ICheckoutService> _logger;
    private readonly IProductsService _productsService;
    private readonly IProductsRepository _productsRepo;
    private readonly ICartService _cartService;
    private readonly IOrdersRepository _ordersRepo;
    private readonly IEmailService _emailService;
    private readonly String _stripeClientSecret;
    private readonly String _stripeReturnUrl;
    private const double TAX_RATE = 0.13;
    public const double shippingCost = 12.99;

    public CheckoutService(
        ILogger<ICheckoutService> logger,
        IProductsService productsService,
        IProductsRepository productsRepository,
        ICartService cartService,
        IOrdersRepository ordersRepo,
	IEmailService emailService,
	IConfiguration config
    )
    {
        _logger = logger;
        _productsService = productsService;
        _productsRepo = productsRepository;
        _cartService = cartService;
        _ordersRepo = ordersRepo;
	_emailService = emailService;
	_stripeClientSecret = config["StripeSecrets:ApiKey"];
	_stripeReturnUrl = config["Stripe:ReturnUrl"];
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
	    Shipping = String.Format("{0:C2}", cartTotals.Shipping),
            Total = String.Format("{0:C2}", cartTotals.TotalNoShipping),
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

	foreach(long productId in cart) 
	{
	    await _ordersRepo.AddOrderProduct(result, productId);
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
    public async Task<String> SetupStripe(List<long> cart, int orderId) 
    {
        _logger.LogDebug($"sizeof cart: {cart.Count}");

	var lineItems = await GetLineItemsFromCart(cart);

        _logger.LogDebug($"sizeof lineItems: {lineItems.Count}");

	var shippingOptions = new List<SessionShippingOptionOptions> {
	    new SessionShippingOptionOptions {
		ShippingRateData = new SessionShippingOptionShippingRateDataOptions {
		    Type = "fixed_amount",
		    FixedAmount = new SessionShippingOptionShippingRateDataFixedAmountOptions {
			Amount = (long)(shippingCost * 100),
			Currency = "CAD"
		    },
		    DisplayName = "Standard Shipping",
		},
	    },
	};

	var autoTax =  new SessionAutomaticTaxOptions {
	    Enabled = true,
	};

	var shippingAddrOptions = new SessionShippingAddressCollectionOptions {
	    AllowedCountries = new List<String> { "CA" },
	};

        var options = new SessionCreateOptions
        {
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = _stripeReturnUrl,
	    ClientReferenceId = orderId.ToString(),
	    ShippingOptions = shippingOptions,
	    AutomaticTax = autoTax,
	    ShippingAddressCollection = shippingAddrOptions,
        };

        var client = new StripeClient(_stripeClientSecret);

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

    public async Task CompleteCheckout(int orderId, double shipping, double total)
    {
	_logger.LogDebug("marking order as complete");
	await _ordersRepo.CompleteOrder(orderId, shipping, total);

	String username = await _ordersRepo.GetUsernameFromOrderId(orderId);

	var products = await _productsRepo.GetProductsByOrderId(orderId);

	String message = BuildInvoiceEmail(username, products, total);
	_logger.LogInformation("Order complete, sending order.");
	await _emailService.SendEmail(username, "Ward4Woods Order", message);
    }

    /**
     * <summary>
     * Constructs an object containing values of product subtotal, tax, and total (without tax).
     * </summary>
     */
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
        var total = Math.Round(subtotal + taxAmount + shippingCost, 2);

	return new CartTotalValues {
	    Subtotal = subtotal,
	    Tax = taxAmount,
	    Shipping = shippingCost,
	    TotalNoShipping = total,
	};
    }

    private class CartTotalValues 
    {
	public double Subtotal { get; set; }
	public double Tax { get; set; }
	public double Shipping { get; set; }
	public double TotalNoShipping { get; set; }
    }

    public string BuildInvoiceEmail(string customerName, List<Models.Product> products, double total)
    {
    var sb = new StringBuilder();

    sb.Append(@"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<title>Your Ward 4 Woods Order</title>
<style>
    body {
        font-family: Arial, sans-serif;
        color: #333;
        background-color: #f7f7f7;
        padding: 20px;
    }
    .container {
        max-width: 600px;
        margin: auto;
        background: #ffffff;
        padding: 20px;
        border-radius: 10px;
        border: 1px solid #e0e0e0;
    }
    h1 {
        text-align: center;
        color: #2c3e50;
    }
    .header {
        text-align: center;
        margin-bottom: 25px;
    }
    .summary {
        margin-bottom: 20px;
        padding: 10px;
        background: #eef6ff;
        border-left: 4px solid #3874cb;
    }
    table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 15px;
    }
    th {
        background: #3874cb;
        color: white;
        padding: 10px;
        text-align: left;
    }
    td {
        padding: 10px;
        border-bottom: 1px solid #dddddd;
    }
    .total-row td {
        font-size: 18px;
        border-top: 2px solid #3874cb;
        font-weight: bold;
    }
    .footer {
        margin-top: 25px;
        font-size: 12px;
        text-align: center;
        color: #777;
    }
</style>
</head>
<body>
<div class='container'>
    <div class='header'>
        <h1>Thank You for Your Order!</h1>
        <p>Your Ward 4 Woods invoice is below.</p>
    </div>

    <div class='summary'>
        <strong>Hello " + customerName + @"!</strong><br/>
        Thank you for supporting our small woodworking business.<br/>
        Below is a summary of your order.
    </div>

    <table>
        <tr>
            <th style='width:60%;'>Product</th>
            <th style='width:20%;'>Price</th>
        </tr>");

    foreach (var p in products)
    {
        sb.Append($@"
        <tr>
            <td>{System.Net.WebUtility.HtmlEncode(p.Name)}</td>
            <td>{p.Price.ToString("C2")}</td>
        </tr>");
    }

    sb.Append($@"
        <tr class='total-row'>
            <td>Total</td>
            <td>{total.ToString("C2")}</td>
        </tr>
    </table>

    <div class='footer'>
        If you have any questions about your order, simply reply to this email.<br/>
        &copy; {DateTime.Now.Year} Ward 4 Woods. All rights reserved.
    </div>
</div>
</body>
</html>");

    return sb.ToString();
}

}
