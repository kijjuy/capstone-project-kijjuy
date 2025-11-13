namespace app.Models;

public class ProductViewModelWithImages
{
    public required ProductViewModel InternalModel { get; set; }

    public required List<String> ImageNames { get; set; }

    public String CategoryName
    {
        get => InternalModel.CategoryName;
    }

    public long ProductId
    {
        get => InternalModel.ProductId;
    }

    public String ProductName
    {
        get => InternalModel.ProductName;
    }

    public double Price
    {
        get => InternalModel.Price;
    }

    public String Description
    {
        get => InternalModel.Description;
    }
}
