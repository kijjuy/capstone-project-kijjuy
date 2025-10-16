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

	if(query.Connection == null) {
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
            var rowDict = CreateSqlDictionary(reader);
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
        query.Connection.Open();


        using var reader = await query.ExecuteReaderAsync();
        reader.Read();

        if (reader.FieldCount == 0)
        {
            _logger.LogWarning($"Could not find product with id={id}. Returning null.");
            return null;
        }

        var rowDict = CreateSqlDictionary(reader);
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

        int result = query.ExecuteNonQuery();
        _logger.LogDebug($"Delete affected {result} rows.");

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
     * Creates a dictionary with each key being the column name and value being the column value from the database.
     * This can be used to map any type T with ColumnAttributes.
     * </summary>
     */
    private Dictionary<String, object> CreateSqlDictionary(SqliteDataReader reader)
    {
        Dictionary<String, object> rowDict = new Dictionary<string, object>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            String name = reader.GetName(i);
            var val = reader.GetValue(i);
            rowDict[name] = val;
        }
        return rowDict;
    }
}
