using System.Text.Json;
using Microsoft.Data.Sqlite;
using app;

namespace tests;

public class GetAllProductsTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly String _connectionString = "Data Source=../../test.db;Mode=Memory;Cache=Shared";

    public GetAllProductsTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private void SeedProducts(int newProductsCount)
    {
        using var db = new SqliteConnection(_connectionString);
        db.Open();

        using (var command = new SqliteCommand()) {
	    command.CommandText = "DELETE FROM products";
	    command.Connection = db;
            command.ExecuteNonQuery();
	}

        for (int i = 0; i < newProductsCount; i++)
        {
	    using(var command = new SqliteCommand()) 
	    {
		command.Connection = db;
		command.CommandText = "INSERT INTO products (name, category_id, price, description, is_available)" +
		    "VALUES(@productName, @categoryId, @price, @description, @isAvailable);";
            	command.Parameters.AddWithValue("@productName", $"Product {i}");
            	command.Parameters.AddWithValue("@categoryId", 1);
            	command.Parameters.AddWithValue("@price", 99.99 + i);
            	command.Parameters.AddWithValue("@description", "Description for product " + i);
	    	command.Parameters.AddWithValue("@isAvailable", true);
            	command.ExecuteNonQuery();
	    }
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
