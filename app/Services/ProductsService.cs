using app.Repositories;
using app.Models;

namespace app.Services;

public interface IProductsService
{
    //TODO: restructure to return base product model, then transform into view models
    public Task<List<Product>> GetAllProducts();
    public Task<Product?> GetProductById(int id);
    public void DeleteProduct(int productId);
    public Task<int> CreateProduct(CreateProductModel product);
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
    public async Task<List<Product>> GetAllProducts()
    {
        return await _products.GetAllProducts();
    }

    /**
     * <summary>
     * Checks if id is valid, then
     * Gets a single product from the repository and maps it to ProductViewModel, 
     * or returns null if the product from the repo was null.
     * </summary>
     */
    public async Task<Product?> GetProductById(int id)
    {
        if (id < 1)
        {
            _logger.LogWarning($"Tried to get product with bad id. id={id}");
            throw new ArgumentException("Product id must be greater than 0.");
        }
        return await _products.GetProductById(id);
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
     * Creates a new product from data in <paramref name="product"/>. Once product is 
     * created, uses productId to insert image data into images table.
     * <see langword="return"/> new product Id.
     * </summary>
     */
    public async Task<int> CreateProduct(CreateProductModel product)
    {
        if (product.Name == null || product.Name.Equals(String.Empty) ||
            product.Price == 0 ||
            product.CategoryId == 0 ||
            product.Description == null || product.Description.Equals(String.Empty))
        {
            _logger.LogWarning($"Attempted to create product with null values.");
            throw new ArgumentException("Cannot create product with null values.");
        }

        List<String> imageNames = await SaveImages(product);

        int newId = _products.CreateProduct(product);

        if (newId < 1)
        {
            _logger.LogError($"Error inserting new product. Expected newId >= 1, got newId={newId}.");
            throw new BadSqlResultException($"Error inserting new product. Expected newId >= 1, got newId={newId}.");
        }

        foreach (var imageName in imageNames)
        {
            await _products.CreateImage(newId, imageName);
        }

        return newId;
    }

    private async Task<List<String>> SaveImages(CreateProductModel product)
    {
        List<String> imageIds = new List<String>();
        foreach (var file in product.Files)
        {
            var imageId = await SaveFile(file);
            imageIds.Add(imageId);
        }
        return imageIds;
    }

    /**
     * <summary>
     * Tries to create a new file in the Images directory. Creates that directory if it doesn't exist.
     * Copies <paramref name="image"/> stream to the newly created file.
     *
     * <see langword="return"/> <see langword="false"/> if image creation failed.
     * <see langword="return"/> <see langword="true"/> if image creation succeeded.
     * </summary>
     */
    private async Task<String> SaveFile(IFormFile image)
    {
        if (!(image.ContentType == "image/jpg" ||
                image.ContentType == "image/png" ||
                image.ContentType == "image/jpeg"))
        {
            String message = $"Filetype was not an allowed type. Filetype was {image.ContentType}";
            _logger.LogError(message);
            throw new FileTypeException(message);
        }


        var filetype = image.ContentType.Split("/")[1];
        _logger.LogDebug($"Got filetype of ${filetype} when creating image.");
        var imageName = Guid.NewGuid() + "." + filetype;
        var imagePath = AppContext.BaseDirectory + "/Images/" + imageName;

        try
        {
            Directory.CreateDirectory(AppContext.BaseDirectory + "/Images/");
        }
        catch (Exception e)
        {
            _logger.LogError($@"Error when creating/getting directory. Error={e.Message}.\n
		    {e.StackTrace}");
            throw;
        }

        FileStream newFile;
        try
        {
            newFile = File.Create(imagePath);
        }
        catch (Exception e)
        {
            _logger.LogError($@"Error when creating new file for image. Error={e.Message}.\n
		    {e.StackTrace}");
            throw;
        }

        await image.CopyToAsync(newFile);
        return imageName;
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

