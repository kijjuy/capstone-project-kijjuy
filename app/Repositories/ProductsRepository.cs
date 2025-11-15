using app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace app.Repositories;

public interface IProductsRepository
{
    public Task<List<Product>> GetAllProducts(bool shouldGetUnavailable);
    public Task<Product?> GetProductById(int id, bool shouldGetUnavailable);
    public int DeleteProduct(int productId);
    public int CreateProduct(CreateProductModel product);
    public Task UpdateProduct(UpdateProductModel product, int id);
    public Task CreateImage(int productId, String imageName);
    public Task<List<ImageDataModel>> GetImageDataByProductId(int productId);
    public Task MarkProductUnavailable(long productId);
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
     * into Product.
     * </summary>
     */
    public async Task<List<Product>> GetAllProducts(bool shouldGetUnavailable = false)
    {
        _logger.LogDebug("Hit GetAllProducts");
        var products = new List<Product>();

        using SqliteConnection db = new SqliteConnection(_connString);

        SqliteCommand query;

        if (shouldGetUnavailable)
        {
            query = new SqliteCommand("SELECT * FROM products;", db);
        }
        else
        {
            query = new SqliteCommand("SELECT * FROM products WHERE is_available = 1;", db);
        }

        _logger.LogDebug("Selected data");

        if (query.Connection == null)
        {
            var message = "query connection was null when trying to get all products.";
            _logger.LogError(message);
            throw new DbConnectionException(message);
        }

        await query.Connection.OpenAsync();
        using var reader = await query.ExecuteReaderAsync();
        _logger.LogDebug("reader created");


        while (await reader.ReadAsync())
        {
            var rowDict = ReaderMapper.CreateSqlDictionary(reader);
            //_logger.LogDebug($"FieldCount: {reader.FieldCount}");
            var product = _readerMapper.MapDataToModel<Product>(rowDict);
            products.Add(product);
            //_logger.LogDebug("Reading data...");
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
    public async Task<Product?> GetProductById(int id, bool shouldGetUnavailable)
    {
        using SqliteConnection db = new SqliteConnection(_connString);

        SqliteCommand query;

        if (shouldGetUnavailable)
        {
            query = new SqliteCommand("SELECT * FROM products WHERE product_id = @id;", db);
        }
        else
        {
            query = new SqliteCommand("SELECT * FROM products WHERE product_id = @id AND is_available = 1", db);
        }

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
        return _readerMapper.MapDataToModel<Product>(rowDict);
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

    /**
     * <summary>
     * Creates image entry with <paramref name="productId"/> and <paramref name="imageId"/>.
     * </summary>
     */
    public async Task CreateImage(int productId, String imageName)
    {
        using SqliteConnection db = new SqliteConnection(_connString);
        SqliteCommand query = new SqliteCommand(@"
		INSERT INTO images (product_id, file_path) VALUES(@product_id, @file_path) RETURNING image_id;
		", db);

        query.Parameters.AddWithValue("@product_id", productId);
        query.Parameters.AddWithValue("@file_path", imageName);

        if (query.Connection == null)
        {
            var message = "query connection was null when trying to create image data.";
            _logger.LogError(message);
            throw new DbConnectionException(message);
        }

        await query.Connection.OpenAsync();
        var reader = await query.ExecuteReaderAsync();
        reader.Read();
        long id = (long)reader.GetValue(0);

        _logger.LogInformation($"Created new image data entry with id={id}.");
    }

    public async Task<List<ImageDataModel>> GetImageDataByProductId(int productId)
    {
        using SqliteConnection db = new SqliteConnection(_connString);
        SqliteCommand query = new SqliteCommand(@"
		SELECT file_path FROM images WHERE product_id = @product_id;
		", db);
        query.Parameters.AddWithValue("@product_id", productId);

        await query.Connection!.OpenAsync();

        var reader = await query.ExecuteReaderAsync();

        var imagesData = new List<ImageDataModel>();

        while (await reader.ReadAsync())
        {
            var rowDict = ReaderMapper.CreateSqlDictionary(reader);
            var imageData = _readerMapper.MapDataToModel<ImageDataModel>(rowDict);
            imagesData.Add(imageData);
        }

        _logger.LogDebug($"Got {imagesData.Count} images for product with id={productId}.");
        return imagesData;
    }

    public async Task MarkProductUnavailable(long productId)
    {
        using SqliteConnection db = new SqliteConnection(_connString);
        var query = new SqliteCommand(@"
		UPDATE products SET is_available = 0 WHERE product_id = @product_id
		", db);

        query.Parameters.AddWithValue("@product_id", productId);

        await query.Connection!.OpenAsync();

        var result = await query.ExecuteNonQueryAsync();
        _logger.LogInformation($"Set product with id={productId} to not available.");
    }
}

