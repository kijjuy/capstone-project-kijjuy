using System.Text.Json;
using Microsoft.Data.Sqlite;
using app;

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

    private void initDb()
    {
        Console.WriteLine("---------------------------------------------------- hit initDb ---------------------------------------------------------");
        using var db = new SqliteConnection(_connectionString);
        db.Open();

        using var command = new SqliteCommand(@"
		-- First database migration

	    DROP TABLE IF EXISTS users;
	    DROP TABLE IF EXISTS orders;
	    DROP TABLE IF EXISTS order_products;
	    DROP TABLE IF EXISTS images;
	    DROP TABLE IF EXISTS products;
	    DROP TABLE IF EXISTS categories;
	    
	    CREATE TABLE categories (
	        category_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	        category_name NVARCHAR(30) NOT NULL
	    );
	    
	    
	    CREATE TABLE products (
	        product_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	        category_id INTEGER NOT NULL,
	        name NVARCHAR(50) NOT NULL,
	        price NUMERIC(6,2) NOT NULL,
	        description NVARCHAR(500) NOT NULL,
	        creation_date DATE NOT NULL DEFAULT CURRENT_TIMESTAMP,
	        update_date DATE NOT NULL DEFAULT CURRENT_TIMESTAMP,
	        is_available INTEGER NOT NULL,
	    
	        FOREIGN KEY (category_id) REFERENCES categories(category_id)
	    );
	    
	    
	    CREATE TABLE images (
	        image_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	        product_id INTEGER NOT NULL,
	        file_path NVARCHAR(100) NOT NULL,
	    
	        FOREIGN KEY (product_id) REFERENCES products(product_id)
	    );
	    
	    CREATE TABLE users (
	        user_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	        email TEXT NOT NULL,
	        pass_hash TEXT NOT NULL,
	        first_name NVARCHAR(50),
	        last_name NVARCHAR(50),
	        phone CHAR(10),
	        address NVARCHAR(100),
	        creation_date DATE NOT NULL DEFAULT CURRENT_TIMESTAMP,
	        update_date DATE NOT NULL DEFAULT CURRENT_TIMESTAMP
	    );
	    
	    CREATE TABLE orders (
	        order_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	        user_id INTEGER NOT NULL,
	        subtotal_paid NUMERIC(7,2) NOT NULL,
	        tax_paid NUMERIC(6,2) NOT NULL,
	        shipping_paid NUMERIC(5,2) NOT NULL,
	        total_paid NUMERIC (7,2) NOT NULL,
	        shipping_address NVARCHAR(100) NOT NULL,
	        shipping_name NVARCHAR(100) NOT NULL,
	        cc_last_4 CHAR(4) NOT NULL,
	        order_date DATE NOT NULL,
	    
	        FOREIGN KEY(user_id) REFERENCES users(user_id)
	    );
	    
	    CREATE TABLE order_products (
	        order_product_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	        product_id INTEGER NOT NULL,
	        order_id INTEGER NOT NULL,
	    
	        FOREIGN KEY(product_id) REFERENCES products(product_id),
	        FOREIGN KEY(order_id) REFERENCES orders(order_id)
	    );
	    
	    
	    -- Not working currently
	    DROP TRIGGER IF EXISTS auto_update_date_products;
	    
	    CREATE TRIGGER auto_update_date_products
	    AFTER UPDATE
	    on products
	    FOR EACH ROW
	        BEGIN
	    	UPDATE products SET update_date = CURRENT_TIMESTAMP WHERE old.product_id = new.product_id;
	        END;



		", db);
        command.ExecuteNonQuery();

        command.CommandText = @"
		    INSERT INTO categories (category_name) VALUES('category0');
		    INSERT INTO categories (category_name) VALUES('category1');
		    INSERT INTO categories (category_name) VALUES('category2');
		    ";

        Console.WriteLine("finished init dbing");
        command.ExecuteNonQuery();

    }

    private void SeedProducts(int newProductsCount)
    {
        using var db = new SqliteConnection(_connectionString);
        db.Open();

        using (var command = new SqliteCommand())
        {
            command.CommandText = "DELETE FROM products";
            command.Connection = db;
            command.ExecuteNonQuery();
        }

        for (int i = 0; i < newProductsCount; i++)
        {
            using (var command = new SqliteCommand())
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

    //TODO: currently repo is not using the same database as the seeded one... 
    [Fact]
    public async Task ProductsEndpointReturnsAllProducts()
    {
        //arrange
        initDb();
        Console.WriteLine("Now starting tests");
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
