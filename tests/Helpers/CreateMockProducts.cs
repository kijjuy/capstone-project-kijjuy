using app.Models;

namespace tests;

public class CreateMockProducts
{
    public static List<ProductViewModel> CreateProductViewModels(int numProducts)
    {
        var prodList = new List<ProductViewModel>();
        for (int i = 0; i < numProducts; i++)
        {
            var product = new ProductViewModel
            {
                ProductId = i + 1,
                CategoryName = "Category " + i % 3,
                ProductName = "Product " + i,
                Price = 99.99 + i,
                Description = "Description for product " + i
            };
            prodList.Add(product);
        }
        return prodList;
    }
}
