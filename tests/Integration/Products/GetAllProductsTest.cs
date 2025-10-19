using System.Text.Json;
using Microsoft.Data.Sqlite;
using app;

namespace tests;

public class GetAllProductsTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly String _connectionString = "Data Source=test.db";

    public GetAllProductsTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    //TODO: Currently not finding database... try to fix this
    private void SeedProducts(int newProductsCount)
    {
        using var db = new SqliteConnection(_connectionString);
        db.Open();

        using var command = new SqliteCommand("DELETE FROM products", db);
        command.ExecuteNonQuery();

        for (int i = 0; i < newProductsCount; i++)
        {
            command.CommandText = "INSERT INTO products (Name, CategoryId, Price, Description)" +
        "VALUES(Product@productId, @categoryId, @price, @description);";
            command.Parameters.AddWithValue("@productId", i);
            command.Parameters.AddWithValue("@categoryId", i % 3);
            command.Parameters.AddWithValue("@price", 99.99 + i);
            command.Parameters.AddWithValue("@description", "Description for product " + i);
            command.ExecuteNonQuery();
        }
    }

    #region GetProductsIntegration

    [Fact]
    public async Task ProductsEndpointReturnsAllProducts()
    {
        //arrange
        SeedProducts(10);
        var client = _factory.CreateClient();

        //act
        var response = await client.GetAsync("/Products");

        //assert
        response.EnsureSuccessStatusCode();
        var body = response.Content.ReadAsStream();
        List<ProductViewModel> products = new List<ProductViewModel>();
        JsonSerializer.Serialize<List<ProductViewModel>>(body, products);
        Console.WriteLine($"size of products = {products.Count}");
    }
    #endregion
}
