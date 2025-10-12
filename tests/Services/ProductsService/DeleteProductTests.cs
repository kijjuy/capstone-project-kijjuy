using app.Services;
using app.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace tests;

public class DeleteProductTests
{
    private readonly ILogger<ProductsService> _serviceLogger;

    public DeleteProductTests()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        _serviceLogger = loggerFactory.CreateLogger<ProductsService>();
    }

    //To test: 
    //Delete with valid id and product exists returns 1
    //Delete with valid id and products doesn't exist (return 0 from repo) throws BadSqlDataException
    //Delete with invalid id (0) throws ArgumentException


    #region DeleteProduct

    /**
     * <summary>
     * Tests that service getting a valid response from the repo
     * does not throw any exception.
     * </summary>
     */
    [Fact]
    public void ProductIsDeletedDoesNothing()
    {
        //arrange
        var mockRepo = new Mock<IProductsRepository>();
        mockRepo.Setup(repo => repo.DeleteProduct(1))
            .Returns(1);

        var service = new ProductsService(_serviceLogger, mockRepo.Object);

        //act
        //assert
        service.DeleteProduct(1);
    }

    /**
     * <summary>
     * Tests that passing 0 to the DeleteProduct method throws an ArgumentException
     * </summary>
     */
    [Fact]
    public void InvalidProductIdThrowsArgumentException()
    {
        //arrange
        var mockRepo = new Mock<IProductsRepository>();
        var service = new ProductsService(_serviceLogger, mockRepo.Object);

        //act
        //assert
        Assert.Throws<ArgumentException>(() => service.DeleteProduct(0));
    }

    /**
     * <summary>
     * Tests that if the repo returns 0 then a BadSqlResultException is thrown.
     * </summary>
     */
    [Fact]
    public void NoRowsDeletedThrowsBadSqlDataException()
    {
        //arrange
        var mockRepo = new Mock<IProductsRepository>();
        mockRepo.Setup(repo => repo.DeleteProduct(1))
            .Returns(0);

        var service = new ProductsService(_serviceLogger, mockRepo.Object);

        //act
        //assert
        Assert.Throws<BadSqlResultException>(() => service.DeleteProduct(1));
    }

    #endregion

}
