namespace app.Models;

public class CreateProductModel
{
    public required int CategoryId { get; set; }

    public required String Name { get; set; }

    public required decimal Price { get; set; }

    public required String Description { get; set; }
}
