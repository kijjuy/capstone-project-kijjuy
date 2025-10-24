using app;

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
}
