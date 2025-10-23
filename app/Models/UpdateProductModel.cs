namespace app.Models;

public class UpdateProductModel
{
    public required int CategoryId { get; set; }

    public required String Name { get; set; }

    public required decimal Price { get; set; }

    public required String Description { get; set; }

    public required bool IsAvailable { get; set; }
}
