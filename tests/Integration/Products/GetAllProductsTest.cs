using Microsoft.Data.Sqlite;
using app;
using app.Models;

namespace tests;

public class GetAllProductsTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly String _connectionString;

    public GetAllProductsTest(CustomWebApplicationFactory<Program> factory)
    {
        String dbPath = CustomWebApplicationFactory<Program>.dbPath;
        _connectionString = $"Data Source={dbPath}";
        _factory = factory;
    }

    #region GetProductsIntegration

    [Fact]
    public async Task ProductsEndpoint_HasProducts_ReturnsAllProducts()
    {
        //arrange
        DbHelper.initDb(_connectionString);
        var products = DbHelper.SeedProducts(10, _connectionString);
        var client = _factory.CreateClient();

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
        DbHelper.initDb(_connectionString);
        var client = _factory.CreateClient();

        //act
        var response = await client.GetAsync("/products");
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        Assert.Contains("No products", body);
    }

    #endregion
}
