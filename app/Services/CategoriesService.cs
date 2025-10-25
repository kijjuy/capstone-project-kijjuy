using app.Models;
using app.Repositories;

namespace app.Services;

public interface ICategoriesService
{
    public Task<List<Category>> GetAllCategories();
    public Task<Category> GetCategoryById(long id);
}

public class CategoriesService : ICategoriesService
{
    private readonly ILogger<CategoriesService> _logger;
    private readonly ICategoriesRepository _categoriesRepo;

    public CategoriesService(ILogger<CategoriesService> logger, ICategoriesRepository categoriesRepo)
    {
        _logger = logger;
        _categoriesRepo = categoriesRepo;
    }

    /**
     * <summary>
     * Returns all categories currenlty in the database.
     * </summary>
     */
    public async Task<List<Category>> GetAllCategories()
    {
        return await _categoriesRepo.GetAllCategories();
    }

    /**
     * <summary>
     * returns a single category based on its id.
     * </summary>
     */
    public async Task<Category> GetCategoryById(long id)
    {
        if (id < 1)
        {
            _logger.LogWarning("Tried to get product with id less than 1.");
            throw new ArgumentException("Id must be greater than 0.");
        }

        return await _categoriesRepo.GetCategoryById(id);
    }
}

