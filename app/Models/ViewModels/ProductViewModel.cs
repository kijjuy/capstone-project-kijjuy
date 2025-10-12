
/**
 * View model class for product containing only
 * info nessecary for frontend
 */
public class ProductViewModel
{
    public required long ProductId { get; set; }

    public required String CategoryName { get; set; }

    public required String ProductName { get; set; }

    public required double Price { get; set; }

    public required String Description { get; set; }
}
