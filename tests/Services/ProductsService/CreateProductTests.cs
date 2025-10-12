using app.Services;
using app.Repositories;
using app.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace tests;

public class CreateProductTests
{
    private readonly ILogger<ProductsService> _serviceLogger;

    public CreateProductTests()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        _serviceLogger = loggerFactory.CreateLogger<ProductsService>();

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
    public void GoodDataReturnsValidProduct() 
    {
	//arrange
	var mockRepo = new Mock<IProductsRepository>();
	CreateProductModel cpm = new CreateProductModel
	{
	    Name = "TestName",
	    CategoryId = 1,
	    Price = (decimal)123.45,
	    Description = "Test description",
	};

	mockRepo.Setup(repo => repo.CreateProduct(cpm))
	    .Returns(1);

	ProductsService service = new ProductsService(_serviceLogger, mockRepo.Object);

	//act
	int result = service.CreateProduct(cpm);
	
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
    public void MissingModelDataThrowsArgumentException() 
    {
	//arrange
	var mockRepo = new Mock<IProductsRepository>();

	CreateProductModel cpmEmptyName = new CreateProductModel
	{
	    Name = "",
	    CategoryId = 1,
	    Price = (decimal)123.45,
	    Description = "Test description",
	};
	CreateProductModel cpmNullName = new CreateProductModel
	{
	    Name = null,
	    CategoryId = 1,
	    Price = (decimal)123.45,
	    Description = "Test description",
	};
	CreateProductModel cpmEmptyDescription = new CreateProductModel
	{
	    Name = "test name",
	    CategoryId = 1,
	    Price = (decimal)123.45,
	    Description = "",
	};
	CreateProductModel cpmNullDescription = new CreateProductModel
	{
	    Name = "Test name",
	    CategoryId = 1,
	    Price = (decimal)123.45,
	    Description = null,
	};
	CreateProductModel cpmCatIdIsZero = new CreateProductModel
	{
	    Name = "Test name",
	    CategoryId = 0,
	    Price = (decimal)123.45,
	    Description = "Test description",
	};
	CreateProductModel cpmPriceIsZero = new CreateProductModel
	{
	    Name = "Test name",
	    CategoryId = 1,
	    Price = 0,
	    Description = "Test Description",
	};
	ProductsService service = new ProductsService(_serviceLogger, mockRepo.Object);

	//act
	//assert
	Assert.Throws<ArgumentException>(() => service.CreateProduct(cpmEmptyName));
	Assert.Throws<ArgumentException>(() => service.CreateProduct(cpmEmptyDescription));
	Assert.Throws<ArgumentException>(() => service.CreateProduct(cpmNullName));
	Assert.Throws<ArgumentException>(() => service.CreateProduct(cpmNullDescription));
	Assert.Throws<ArgumentException>(() => service.CreateProduct(cpmCatIdIsZero));
	Assert.Throws<ArgumentException>(() => service.CreateProduct(cpmPriceIsZero));
    }

    /**
     * <summary>
     * Tests that a return value of 0 throws a BadSqlResultException.
     * </summary>
     */
    [Fact]
    public void BadReturnValueFromRepoThrowsBadSqlDataException() 
    {
        //arrange
        var mockRepo = new Mock<IProductsRepository>();

        CreateProductModel cpm = new CreateProductModel 
        {
            Name = "Test name",
            CategoryId = 1,
            Price = (decimal)123.45,
            Description = "Test Description",
        };

        mockRepo.Setup(repo => repo.CreateProduct(cpm))
            .Returns(0);

        ProductsService service = new ProductsService(_serviceLogger, mockRepo.Object);

        //act
        //assert
        Assert.Throws<BadSqlResultException>(() => service.CreateProduct(cpm));
    }

    #endregion
}
