namespace app.Models;

public class CartViewModel
{
    public required List<ProductViewModelWithImages> Products { get; set; }

    public required double Subtotal { get; set; }
}
