using app;

namespace tests;

public class ProductsIndexTests : IClassFixture<CustomWebApplicationFactory<Program>>
{

    public ProductsIndexTests(CustomWebApplicationFactory<Program> factory)
    {
    }

    #region GetProductsIntegration

    [Fact]
    public async Task ProductsEndpoint_HasProducts_ReturnsAllProducts()
    {
        //arrange
        var factory = new CustomWebApplicationFactory<Program>();
        var client = factory.CreateDefaultClient();
        DbHelper.initDb(factory.connectionString);
        var products = DbHelper.SeedProducts(10, factory.connectionString);

        //act
        var response = await client.GetAsync("/products");
        var body = await response.Content.ReadAsStringAsync();

        //assert
        response.EnsureSuccessStatusCode();

        foreach (var product in products)
        {
            Assert.Contains(product.Name, body);
        }
    }



    [Fact]
    public async Task ProductsEndpoint_HasNoProducts_ReturnsNotFoundInfo()
    {
        //arrange
        var factory = new CustomWebApplicationFactory<Program>();
        var client = factory.CreateDefaultClient();
        DbHelper.initDb(factory.connectionString);

        //act
        var response = await client.GetAsync("/products");
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        Assert.Contains("No products", body);
    }

    #endregion
}
