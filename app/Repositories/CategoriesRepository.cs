using app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace app.Repositories;

public interface ICategoriesRepository
{
    public Task<List<Category>> GetAllCategories();
    public Task<Category> GetCategoryById(long id);
}

public class CategoriesRepository : ICategoriesRepository
{
    private readonly ILogger<CategoriesRepository> _logger;
    private readonly String _connString;
    private readonly ReaderMapper _mapper;

    public CategoriesRepository(
        ILogger<CategoriesRepository> logger,
        IOptions<RepositoryOptions> options,
        ReaderMapper mapper
        )

    {
        _logger = logger;
        _connString = options.Value.ConnectionString;
        _mapper = mapper;
    }

    /**
     * <summary>
     * Gets all categories from the categories table in the database.
     * </summary>
     */
    public async Task<List<Category>> GetAllCategories()
    {
        _logger.LogDebug("Hit GetAllCategories");
        var categories = new List<Category>();
        using SqliteConnection db = new SqliteConnection(_connString);
        _logger.LogDebug("Created db connection");

        SqliteCommand query = new SqliteCommand("SELECT * FROM categories", db);
        query.Connection.Open();
        _logger.LogDebug("opened db connection");

        using var reader = await query.ExecuteReaderAsync();
        _logger.LogDebug("executed query");

        while (await reader.ReadAsync())
        {
            _logger.LogDebug("mapping to object...");
            var sqlDict = ReaderMapper.CreateSqlDictionary(reader);
            var category = _mapper.MapDataToModel<Category>(sqlDict);
            categories.Add(category);
        }

        return categories;
    }

    /**
     * <summary>
     * Returns a single category where category_id matches the passed in id, or null if not found.
     * </summary>
     */
    public async Task<Category> GetCategoryById(long id)
    {
        using SqliteConnection db = new SqliteConnection(_connString);
        SqliteCommand query = new SqliteCommand("SELECT * FROM categories WHERE category_id = @id", db);
        query.Parameters.AddWithValue("@id", id);

        await query.Connection.OpenAsync();
        var reader = await query.ExecuteReaderAsync();
        await reader.ReadAsync();

        if (!reader.HasRows)
        {
            return null;
        }

        var sqlDict = ReaderMapper.CreateSqlDictionary(reader);
        var category = _mapper.MapDataToModel<Category>(sqlDict);

        return category;
    }
}
