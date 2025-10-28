using Microsoft.Extensions.Logging;
using app.Services;
using app.Repositories;
using app.Models;
using Moq;

namespace tests;

public class AddToCartTests
{
    ILogger<ProductsService> _logger;
    Mock<ICategoriesService> _mockCategoriesService = new Mock<ICategoriesService>();

    public AddToCartTests()
    {
	_logger = LoggerFactory.Create(conf => {
		conf.AddConsole();
		})
	.CreateLogger<ProductsService>();
    }


    [Fact]
    public async Task ValidId_ProductExists_AddsToCart()
    {
	//arrange
	var mockRepo = new Mock<IProductsRepository>();
	mockRepo.Setup(repo => repo.GetProductById(1))
	    .ReturnsAsync(new ProductDataModel{
		    ProductId = 1,
		    CategoryId = 1,
		    Name = "test",
		    Price = 123.45,
		    Description = "test description",
		    CreationDate = DateTime.Now,
		    UpdateDate = DateTime.Now,
		    IsAvailable = 1
	    });

	var service = new ProductsService(_logger, mockRepo.Object, _mockCategoriesService.Object);

	var cart = new List<long>();

	//act
	var isAdded = await service.AddProductToCart(cart, 1);

	//assert
	Assert.True(isAdded);
	Assert.Contains(1, cart);
	Assert.True(cart.Count() == 1);
    }
}
