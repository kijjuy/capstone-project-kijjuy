using app.Repositories;
using app.Models;

namespace app.Services;

public interface IProductsService
{
    public Task<List<ProductViewModel>> GetAllProducts();
    public Task<ProductViewModel?> GetProductById(int id);
    public void DeleteProduct(int productId);
    public int CreateProduct(CreateProductModel product);
    public Task UpdateProduct(UpdateProductModel product, int id);
}

public class ProductsService : IProductsService
{

    private readonly ILogger<ProductsService> _logger;
    private readonly IProductsRepository _products;
    private readonly ICategoriesService _categoriesService;

    public ProductsService(ILogger<ProductsService> logger,
        IProductsRepository products,
    ICategoriesService categoriesService)
    {
        _logger = logger;
        _products = products;
        _categoriesService = categoriesService;
    }

    /**
     * <summary>
     * Gets all products from the database and converts 
     * them into ProductViewModels.
     * </summary>
     */
    public async Task<List<ProductViewModel>> GetAllProducts()
    {
        List<ProductDataModel> dataProducts = _products.GetAllProducts();
        List<ProductViewModel> viewProducts = new List<ProductViewModel>();

        foreach (var dataProduct in dataProducts)
        {
            var category = await _categoriesService.GetCategoryById(dataProduct.CategoryId);
            var viewProduct = new ProductViewModel
            {
                ProductId = dataProduct.ProductId,
                CategoryName = category.CategoryName,
                ProductName = dataProduct.Name,
                Price = dataProduct.Price,
                Description = dataProduct.Description,
            };
            viewProducts.Add(viewProduct);
        }
        return viewProducts;
    }

    /**
     * <summary>
     * Checks if id is valid, then
     * Gets a single product from the repository and maps it to ProductViewModel, 
     * or returns null if the product from the repo was null.
     * </summary>
     */
    public async Task<ProductViewModel?> GetProductById(int id)
    {
        if (id < 1)
        {
            _logger.LogWarning($"Tried to get product with bad id. id={id}");
            throw new ArgumentException("Product id must be greater than 0.");
        }
        var dataModel = await _products.GetProductById(id);
        if (dataModel == null)
        {
            return null;
        }
        var viewProduct = new ProductViewModel
        {
            ProductId = dataModel.ProductId,
            //TODO: Get category name from id here
            CategoryName = $"Category {dataModel.CategoryId}",
            ProductName = dataModel.Name,
            Price = dataModel.Price,
            Description = dataModel.Description,
        };
        return viewProduct;
    }

    /**
     * <summary>
     * Checks if the productId is valid, then deletes a single product from
     * the repository. If no products were deleted, throws a BadSqlResultException.
     * </summary>
     */
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

    /**
     * <summary>
     * Checks that all model properties are valid, then passes the create model to the
     * repository. Returns the result, which is the id of the new product.
     * </summary>
     */
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

    /**
     * <summary>
     * updated a product that has id=<paramref name="id"/> with new values from <paramref name="product"/>.
     * </summary>
     */
    public async Task UpdateProduct(UpdateProductModel product, int id)
    {
        await _products.UpdateProduct(product, id);
    }
}

