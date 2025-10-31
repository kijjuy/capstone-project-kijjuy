namespace app.Models;

public class ProductViewModelWithImages
{
    public required ProductViewModel InternalModel { get; set; }

    public required List<String> ImageNames { get; set; }
}
