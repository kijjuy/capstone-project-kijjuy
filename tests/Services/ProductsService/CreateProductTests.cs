using app.Services;
using app.Repositories;
using app.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace tests;

public class CreateProductTests
{
    private readonly ILogger<ProductsService> _serviceLogger;
    private readonly Mock<ICategoriesService> _mockCategoriesService;

    public CreateProductTests()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        _serviceLogger = loggerFactory.CreateLogger<ProductsService>();

        _mockCategoriesService = new Mock<ICategoriesService>();
    }

    //To test:

    #region CreateProducts

    //valid data creates product
    /**
     * <summary>
     * Tests that a return result of non-zero from the repository 
     * succeeds.
     * </summary>
     */
    [Fact]
    public async Task GoodDataReturnsValidProduct()
    {
        //arrange
        var mockRepo = new Mock<IProductsRepository>();
        CreateProductModel cpm = new CreateProductModel
        {
            Name = "TestName",
            CategoryId = 1,
            Price = (decimal)123.45,
            Description = "Test description",
            Files = await ImageHelper.CreateFakeImages(1),
        };

        mockRepo.Setup(repo => repo.CreateProduct(cpm))
            .Returns(1);

        ProductsService service = new ProductsService(_serviceLogger, mockRepo.Object, _mockCategoriesService.Object);

        //act
        int result = await service.CreateProduct(cpm);

        //assert

        Assert.Equal(1, result);
    }

    /**
     * <summary>
     * Tests that the service throws an ArgumentException if any of the following conditions are met:
     * Name is null,
     * Name is empty string,
     * Description is null,
     * Description is empty string,
     * CategoryId == 0,
     * Price == 0,
     * </summary>
     */
    [Fact]
    public async Task MissingModelDataThrowsArgumentException()
    {
        //arrange
        var mockRepo = new Mock<IProductsRepository>();

        CreateProductModel cpmEmptyName = new CreateProductModel
        {
            Name = "",
            CategoryId = 1,
            Price = (decimal)123.45,
            Description = "Test description",
            Files = await ImageHelper.CreateFakeImages(1),
        };
        CreateProductModel cpmNullName = new CreateProductModel
        {
            Name = null,
            CategoryId = 1,
            Price = (decimal)123.45,
            Description = "Test description",
            Files = await ImageHelper.CreateFakeImages(1),
        };
        CreateProductModel cpmEmptyDescription = new CreateProductModel
        {
            Name = "test name",
            CategoryId = 1,
            Price = (decimal)123.45,
            Description = "",
            Files = await ImageHelper.CreateFakeImages(1),
        };
        CreateProductModel cpmNullDescription = new CreateProductModel
        {
            Name = "Test name",
            CategoryId = 1,
            Price = (decimal)123.45,
            Description = null,
            Files = await ImageHelper.CreateFakeImages(1),
        };
        CreateProductModel cpmCatIdIsZero = new CreateProductModel
        {
            Name = "Test name",
            CategoryId = 0,
            Price = (decimal)123.45,
            Description = "Test description",
            Files = await ImageHelper.CreateFakeImages(1),
        };
        CreateProductModel cpmPriceIsZero = new CreateProductModel
        {
            Name = "Test name",
            CategoryId = 1,
            Price = 0,
            Description = "Test Description",
            Files = await ImageHelper.CreateFakeImages(1),
        };
        ProductsService service = new ProductsService(_serviceLogger, mockRepo.Object, _mockCategoriesService.Object);

        //act
        //assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(cpmEmptyName));
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(cpmEmptyDescription));
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(cpmNullName));
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(cpmNullDescription));
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(cpmCatIdIsZero));
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(cpmPriceIsZero));
    }

    /**
     * <summary>
     * Tests that a return value of 0 throws a BadSqlResultException.
     * </summary>
     */
    [Fact]
    public async Task BadReturnValueFromRepoThrowsBadSqlDataException()
    {
        //arrange
        var mockRepo = new Mock<IProductsRepository>();

        CreateProductModel cpm = new CreateProductModel
        {
            Name = "Test name",
            CategoryId = 1,
            Price = (decimal)123.45,
            Description = "Test Description",
            Files = await ImageHelper.CreateFakeImages(1)
        };

        mockRepo.Setup(repo => repo.CreateProduct(cpm))
            .Returns(0);

        ProductsService service = new ProductsService(_serviceLogger, mockRepo.Object, _mockCategoriesService.Object);

        //act
        //assert
        await Assert.ThrowsAsync<BadSqlResultException>(() => service.CreateProduct(cpm));
    }

    #endregion


}
