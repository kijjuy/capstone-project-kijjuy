using app.Controllers;
using app.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace tests;

public class ProductsControllerTests
{
    private readonly ILogger<ProductsController> _controllerLogger;
    private readonly Mock<ICategoriesService> _mockCategoriesService;

    public ProductsControllerTests()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        _controllerLogger = loggerFactory.CreateLogger<ProductsController>();

        _mockCategoriesService = new Mock<ICategoriesService>();
    }

    [Fact]
    public async Task GetAllProducts_HasMultipleProducts_ReturnsAllProducts()
    {
        //arrange
        var mockService = new Mock<IProductsService>();
        var products = CreateMockProducts.CreateProductViewModels(10);
        mockService.Setup(service => service.GetAllProducts())
        .ReturnsAsync(products);

        var controller = new ProductsController(_controllerLogger, mockService.Object, _mockCategoriesService.Object);

        //act
        var result = controller.GetAllProducts();

        //TODO: what to do with the result...
        Console.WriteLine("--------------------------------- result of GetAllProducts_HasMultipleProducts_ReturnsAllProducts: " + result.ToString());
    }
}
