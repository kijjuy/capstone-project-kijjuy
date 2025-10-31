namespace app.Models;

public class CheckoutSummaryViewModel
{
    public required String Subtotal { get; set; }

    public required String Tax { get; set; }

    public required String Total { get; set; }
}
