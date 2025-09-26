using app.Repositories;
using app.Models;

namespace app.Services;

public interface IProductsService {
    public List<ProductViewModel> GetAllProducts();
}

/**
 */
public class ProductsService : IProductsService {

    private readonly ILogger<ProductsService> _logger;
    private readonly IProductsRepository _products;

    public ProductsService(ILogger<ProductsService> logger,
	    IProductsRepository products) {
	_logger = logger;
	_products = products;
    }

    /**
     * Gets all products from the database and converts 
     * them into ProductViewModels.
     */
    public List<ProductViewModel> GetAllProducts() {
	List<ProductDataModel> dataProducts = _products.GetAllProducts();
	List<ProductViewModel> viewProducts = new List<ProductViewModel>();

	foreach(var dataProduct in dataProducts) {
	    //TODO: Get categories here from category repository
	    var viewProduct = new ProductViewModel{
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
}
