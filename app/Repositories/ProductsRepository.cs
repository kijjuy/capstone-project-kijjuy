using app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace app.Repositories;

public interface IProductsRepository
{
    public List<ProductDataModel> GetAllProducts();
    public int DeleteProduct(int productId);
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

    public List<ProductDataModel> GetAllProducts()
    {
        _logger.LogDebug("Hit GetAllProducts");
        List<ProductDataModel> products = new List<ProductDataModel>();

        using (SqliteConnection db = new SqliteConnection(_connString))
        {
            SqliteCommand query = new SqliteCommand("SELECT * FROM products;", db);
            _logger.LogDebug("Selected data");
            query.Connection.Open();
            query.ExecuteNonQuery();
            using var reader = query.ExecuteReader();
            _logger.LogDebug("reader created");


            while (reader.Read())
            {
                Dictionary<String, object> rowDict = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    String name = reader.GetName(i);
                    var val = reader.GetValue(i);
                    rowDict[name] = val;
                }
                _logger.LogDebug($"FieldCount: {reader.FieldCount}");
                var product = _readerMapper.MapDataToModel<ProductDataModel>(rowDict);
                products.Add(product);
                _logger.LogDebug("Reading data...");
            }
        }
        _logger.LogDebug($"size of products: {products.Count}");
        _logger.LogDebug("Exiting GetAllProducts");
        return products;
    }

    /**
     * <summary>
     * Deletes a product from the database and returns the amount of rows affected.
     * </summary>
     */
    public int DeleteProduct(int productId)
    {
        using SqliteConnection db = new SqliteConnection(_connString);
        SqliteCommand query = new SqliteCommand("DELETE FROM products WHERE product_id = @productId", db);
        query.Parameters["productId"].Value = productId;

        int result = query.ExecuteNonQuery();
        _logger.LogDebug($"Delete affected {result} rows.");

        return result;

    }

}
