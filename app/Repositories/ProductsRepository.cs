using app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace app.Repositories;

public interface IProductsRepository
{
    public List<ProductDataModel> GetAllProducts();
    public Task<ProductDataModel?> GetProductById(int id);
    public int DeleteProduct(int productId);
    public int CreateProduct(CreateProductModel product);
    public Task UpdateProduct(UpdateProductModel product, int id);
}

public class ProductsRepository : IProductsRepository
{

    private readonly ILogger<ProductsRepository> _logger;
    private readonly String _connString;
    private readonly ReaderMapper _readerMapper;

    public ProductsRepository(ILogger<ProductsRepository> logger,
       IOptions<RepositoryOptions> options,
       ReaderMapper readerMapper)
    {
        _logger = logger;
        _connString = options.Value.ConnectionString;
        _readerMapper = readerMapper;
        _logger.LogDebug($"---------------- conn string in reposisotry: {_connString}");
    }

    /**
     * <summary>
     * Fetches a list of all products from the database and maps them
     * into ProductDataModels.
     * </summary>
     */
    public List<ProductDataModel> GetAllProducts()
    {
        _logger.LogDebug("Hit GetAllProducts");
        List<ProductDataModel> products = new List<ProductDataModel>();

        using SqliteConnection db = new SqliteConnection(_connString);


        SqliteCommand query = new SqliteCommand("SELECT * FROM products;", db);
        _logger.LogDebug("Selected data");

        if (query.Connection == null)
        {
            var message = "query connection was null when trying to get all products.";
            _logger.LogError(message);
            throw new DbConnectionException(message);
        }

        query.Connection.Open();
        query.ExecuteNonQuery();
        using var reader = query.ExecuteReader();
        _logger.LogDebug("reader created");


        while (reader.Read())
        {
            var rowDict = ReaderMapper.CreateSqlDictionary(reader);
            _logger.LogDebug($"FieldCount: {reader.FieldCount}");
            var product = _readerMapper.MapDataToModel<ProductDataModel>(rowDict);
            products.Add(product);
            _logger.LogDebug("Reading data...");
        }

        _logger.LogDebug($"size of products: {products.Count}");
        _logger.LogDebug("Exiting GetAllProducts");
        return products;
    }

    /**
     * <summary>
     * Returns a single product with a matching id, or null.
     * </summary>
     */
    public async Task<ProductDataModel?> GetProductById(int id)
    {
        using SqliteConnection db = new SqliteConnection(_connString);
        SqliteCommand query = new SqliteCommand("SELECT * FROM products WHERE product_id = @id", db);
        query.Parameters.AddWithValue("@id", id);

        if (query.Connection == null)
        {
            var message = "query connection was null when trying to get product by id.";
            _logger.LogError(message);
            throw new DbConnectionException(message);
        }

        query.Connection.Open();


        using var reader = await query.ExecuteReaderAsync();
        reader.Read();

        _logger.LogDebug($"FieldCount of reader={reader.FieldCount}");
        _logger.LogDebug($"Reader.HasRows={reader.HasRows}");
        if (!reader.HasRows)
        {
            _logger.LogWarning($"Could not find product with id={id}. Returning null.");
            return null;
        }

        var rowDict = ReaderMapper.CreateSqlDictionary(reader);
        return _readerMapper.MapDataToModel<ProductDataModel>(rowDict);
    }

    //TODO: Make db calls async
    /**
     * <summary>
     * Deletes a product from the database and returns the amount of rows affected.
     * </summary>
     */
    public int DeleteProduct(int productId)
    {
        using SqliteConnection db = new SqliteConnection(_connString);
        SqliteCommand query = new SqliteCommand("DELETE FROM products WHERE product_id = @productId", db);
        query.Parameters.AddWithValue("@productId", productId);

        if (query.Connection == null)
        {
            var message = "query connection was null when trying to delete product.";
            _logger.LogError(message);
            throw new DbConnectionException(message);
        }

        query.Connection.Open();

        int result = query.ExecuteNonQuery();
        _logger.LogDebug($"Delete affected {result} rows.");

        query.Connection.Close();

        return result;

    }

    /**
     * <summary>
     * Inserts a new product into the database and returns the id of that new product.
     * </summary>
     */
    public int CreateProduct(CreateProductModel product)
    {
        using SqliteConnection db = new SqliteConnection(_connString);
        SqliteCommand query = new SqliteCommand("INSERT INTO products (name, price, category_id, description, is_available)" +
                        " VALUES(@name, @price, @category_id, @description, @is_available) RETURNING product_id", db);
        query.Parameters.AddWithValue("@name", product.Name);
        query.Parameters.AddWithValue("@price", product.Price);
        query.Parameters.AddWithValue("@category_id", product.CategoryId);
        query.Parameters.AddWithValue("@description", product.Description);
        query.Parameters.AddWithValue("@is_available", true);

        if (query.Connection == null)
        {
            var message = "query connection was null when trying to create product.";
            _logger.LogError(message);
            throw new DbConnectionException(message);
        }

        query.Connection.Open();

        using var reader = query.ExecuteReader();
        _logger.LogDebug($"Created {reader.RecordsAffected} new rows.");

        reader.Read();
        long newId = (long)reader.GetValue(0);
        _logger.LogInformation($"New product created with id={newId}");
        return (int)newId;
    }


    /**
     * <summary>
     * Sets all values of product with product_id=<paramref name="id"/> to new values in <paramref name="product"/>
     * </summary>
     */
    public async Task UpdateProduct(UpdateProductModel product, int id)
    {
        using SqliteConnection db = new SqliteConnection(_connString);
        SqliteCommand query = new SqliteCommand(@"
	    UPDATE products SET 
	    name = @name,
	    price = @price,
	    category_id = @category_id,
	    description = @description,
	    is_available = @is_available
	    WHERE product_id = @product_id;
		", db);

        query.Parameters.AddWithValue("@name", product.Name);
        query.Parameters.AddWithValue("@price", product.Price);
        query.Parameters.AddWithValue("@category_id", product.CategoryId);
        query.Parameters.AddWithValue("@description", product.Description);
        query.Parameters.AddWithValue("@is_available", product.IsAvailable);
        query.Parameters.AddWithValue("@product_id", id);

        if (query.Connection == null)
        {
            var message = "query connection was null when trying to update product.";
            _logger.LogError(message);
            throw new DbConnectionException(message);
        }

        await query.Connection.OpenAsync();
        var result = query.ExecuteNonQuery();
        _logger.LogInformation($"updated product with id={id} and got result={result}");
    }
}
