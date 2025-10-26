using app;
using System.Net;

namespace tests;

public class ProductDetailsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{

    public ProductDetailsTests(CustomWebApplicationFactory<Program> factory)
    {
    }

    [Fact]
    public async Task ProductExists_ReturnsProductDetails()
    {
        //arrange
        var factory = new CustomWebApplicationFactory<Program>();
        var client = factory.CreateDefaultClient();
        DbHelper.initDb(factory.connectionString);
        var product = DbHelper.SeedProducts(1, factory.connectionString)
            .FirstOrDefault();

        //act
        var response = await client.GetAsync("/products/" + 1);

        //assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains(product.Name, body);
    }

    [Fact]
    public async Task ProductNotExists_ReturnsNotFound()
    {
        //arrange
        var factory = new CustomWebApplicationFactory<Program>();
        var client = factory.CreateDefaultClient();
        DbHelper.initDb(factory.connectionString);

        //act
        var response = await client.GetAsync("/products/1");

        //assert
        Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvalidProductId_ReturnsBadRequest()
    {
        //arrange
        var factory = new CustomWebApplicationFactory<Program>();
        var client = factory.CreateDefaultClient();
        DbHelper.initDb(factory.connectionString);

        //act
        var response = await client.GetAsync("/products/0");

        //
        Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
    }
}
