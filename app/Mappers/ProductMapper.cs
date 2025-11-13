using app.Models;
using app.Services;

namespace app.Mappers;

public interface IProductMapper {
    public Task<ProductViewModel> IntoViewModel(Product product);
    public Task<ProductViewModelWithImages> IntoViewModelWithImages(Product product);
}

public class ProductMapper : IProductMapper {

    private readonly ILogger<ProductMapper> _logger;
    private readonly ICategoriesService _categoriesService;
    private readonly IImagesService _imagesService;

    public ProductMapper(
	    ILogger<ProductMapper> logger,
	    ICategoriesService categoriesService,
    	    IImagesService imagesService
	    )
    {
	_logger = logger;
	_categoriesService = categoriesService;
	_imagesService = imagesService;
    }

    /**
     * <summary>
     * Transforms a base product model into a product view model.
     * </summary>
     */
    public async Task<ProductViewModel> IntoViewModel(Product product)
    {
	var category = await _categoriesService.GetCategoryById(product.CategoryId);
	return new ProductViewModel
	{
	    ProductId = product.ProductId,
	    CategoryName = category.CategoryName,
	    ProductName = product.Name,
	    Price = ((double)product.Price),
	    Description = product.Description,
	};
    }

    /**
     * <summary>
     * Transforms a base product model into a product with images view model.
     * </summary>
     */
    public async Task<ProductViewModelWithImages> IntoViewModelWithImages(Product product)
    {
	var viewModel = await IntoViewModel(product);
	var imagesData = await _imagesService.GetImageDataByProductId(product.ProductId);
	var imageNames = imagesData.Select(i => i.ImageName).ToList();
	return new ProductViewModelWithImages
	{
	    InternalModel = viewModel,
	    ImageNames = imageNames,
	};
    }
}
