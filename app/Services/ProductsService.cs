using app.Repositories;
using app.Models;

namespace app.Services;

public interface IProductsService
{
    public List<ProductViewModel> GetAllProducts();
    public void DeleteProduct(int productId);
    public int CreateProduct(CreateProductModel product);
}

/**
 */
public class ProductsService : IProductsService
{

    private readonly ILogger<ProductsService> _logger;
    private readonly IProductsRepository _products;

    public ProductsService(ILogger<ProductsService> logger,
        IProductsRepository products)
    {
        _logger = logger;
        _products = products;
    }

    /**
     * Gets all products from the database and converts 
     * them into ProductViewModels.
     */
    public List<ProductViewModel> GetAllProducts()
    {
        List<ProductDataModel> dataProducts = _products.GetAllProducts();
        List<ProductViewModel> viewProducts = new List<ProductViewModel>();

        foreach (var dataProduct in dataProducts)
        {
            //TODO: Get categories here from category repository
            var viewProduct = new ProductViewModel
            {
                ProductId = dataProduct.ProductId,
                CategoryName = $"Category {dataProduct.CategoryId}",
                ProductName = dataProduct.Name,
                Price = dataProduct.Price,
                Description = dataProduct.Description,
            };
            viewProducts.Add(viewProduct);
        }
        return viewProducts;
    }

    public void DeleteProduct(int productId)
    {
        if (productId < 1)
        {
            _logger.LogError($"Attempted to delete product with id={productId}. Must be greater than zero.");
            throw new ArgumentException("productId must be greater than zero.");
        }

        int result = _products.DeleteProduct(productId);
        if (result != 1)
        {
            _logger.LogError($"Error deleting product with id={productId}. Expected to delete 1 row, instead deleted {result}.");
            throw new BadSqlResultException($"Error deleting product with id={productId}. Expected to delete 1 row, instead deleted {result}.");
        }
    }

    public int CreateProduct(CreateProductModel product)
    {
        if (product.Name == null || product.Name.Equals(String.Empty) ||
            product.Price == 0 ||
            product.CategoryId == 0 ||
            product.Description == null || product.Description.Equals(String.Empty))
        {
            _logger.LogWarning($"Attempted to create product with null values.");
            throw new ArgumentException("Cannot create product with null values.");
        }

        int result = _products.CreateProduct(product);
        if (result < 1)
        {
            _logger.LogError($"Error inserting new product. Expected newId >= 1, got newId={result}.");
            throw new BadSqlResultException($"Error inserting new product. Expected newId >= 1, got newId={result}.");
        }
        return result;
    }
}

public class BadSqlResultException : Exception
{
    public BadSqlResultException(String message)
    : base(message)
    { }
}
