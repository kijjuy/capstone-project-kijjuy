using app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace app.Repositories;

public interface IProductsRepository
{
    public List<ProductDataModel> GetAllProducts();
}

public class ProductsRepository : IProductsRepository
{

    private readonly ILogger<ProductsRepository> _logger;
    private readonly String _connString;

    public ProductsRepository(ILogger<ProductsRepository> logger,
       IOptions<RepositoryOptions> options)
    {
        _logger = logger;
        _connString = options.Value.ConnectionString;
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
                var product = ReaderMapper.MapDataToModel<ProductDataModel>(reader);
		products.Add(product);
                _logger.LogDebug("Reading data...");
            }
        }
	_logger.LogDebug($"size of products: {products.Count}");
        _logger.LogDebug("Exiting GetAllProducts");
	return products;
    }
}
