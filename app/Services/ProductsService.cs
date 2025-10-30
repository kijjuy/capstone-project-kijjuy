using System.IO;
using app.Repositories;
using app.Models;

namespace app.Services;

public interface IProductsService
{
    public Task<List<ProductViewModel>> GetAllProducts();
    public Task<ProductViewModel?> GetProductById(int id);
    public void DeleteProduct(int productId);
    public Task<int> CreateProduct(CreateProductModel product);
    public Task UpdateProduct(UpdateProductModel product, int id);
    public Task<List<ProductViewModel>> GetProductsFromCart(IEnumerable<long> cart);
    public Task<bool> AddProductToCart(List<long> cart, int productId);
    public bool RemoveFromCart(List<long> cart, int productId);
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

        foreach (var file in product.Files)
        {
            try
            {
                await SaveFile(file);
            }
            catch (Exception e)
            {
                _logger.LogError($@"Error when saving image to disk. Error={e.Message}.");
                throw e;
            }
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
     * Tries to create a new file in the Images directory. Creates that directory if it doesn't exist.
     * Copies <paramref name="image"/> stream to the newly created file.
     *
     * <see langword="return"/> <see langword="false"/> if image creation failed.
     * <see langword="return"/> <see langword="true"/> if image creation succeeded.
     * </summary>
     */
    private async Task SaveFile(IFormFile image)
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
        var imageId = Guid.NewGuid();
        var imagePath = AppContext.BaseDirectory + "/Images/" + imageId + "." + filetype;

        try
        {
            Directory.CreateDirectory(AppContext.BaseDirectory + "/Images/");
        }
        catch (Exception e)
        {
            _logger.LogError($@"Error when creating/getting directory. Error={e.Message}.\n
		    {e.StackTrace}");
            throw e;
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
            throw e;
        }

        await image.CopyToAsync(newFile);
    }

    /**
     * <summary>
     * Loops through product ids in <paramref name="cart"/> and gets each product, adding them to
     * a list. Returns the list of those products.
     * </summary>
     */
    public async Task<List<ProductViewModel>> GetProductsFromCart(IEnumerable<long> cart)
    {
        var products = new List<ProductViewModel>();
        foreach (int productId in cart)
        {
            var product = await GetProductById(productId);
            if (product == null)
            {
                continue;
            }
            products.Add(product);
        }
        return products;
    }

    /**
     * <summary>
     * Takes in <paramref name="cart"/>, checks if the product exists, then adds it to <paramref name="cart"/>.
     * <see langword="return"/> <see langword="true"/> if product is added. 
     * <see langword="return"/> <see langword="false"/> if product was not added.
     * </summary>
     */
    public async Task<bool> AddProductToCart(List<long> cart, int productId)
    {
        _logger.LogDebug($"Count of cart before adding: ${cart.Count()}");
        var product = GetProductById(productId);
        if (product == null)
        {
            _logger.LogWarning($"Cound not find product with id={productId}. Returning same cart.");
            return false;
        }

        cart.Add(productId);
        _logger.LogDebug($"Count of cart after adding: ${cart.Count()}");
        return true;
    }

    /**
     * <summary>
     * Checks if <paramref name="productId"/> is inside of <paramref name="cart"/>. If not, <see langword="return"/>
     * <see langword="false"/>. If yes, <see langword="return"/> <see langword="true"/>
     * </summary>
     */
    public bool RemoveFromCart(List<long> cart, int productId)
    {
        return cart.Remove(productId);
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

