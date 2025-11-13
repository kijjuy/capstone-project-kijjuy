using app.Models;
using app.Repositories;

namespace app.Services;

public interface IImagesService
{
    public Task<byte[]> GetImageBytesByName(String imageName);
    public Task<List<ImageDataModel>> GetImageDataByProductId(long id);
}

public class LocalImagesService : IImagesService
{
    private readonly ILogger<IImagesService> _logger;
    private readonly IProductsRepository _productsRepo;

    public LocalImagesService(
        ILogger<IImagesService> logger,
        IProductsRepository prodRepo
    )
    {
        _logger = logger;
        _productsRepo = prodRepo;
    }

    public async Task<byte[]> GetImageBytesByName(String imageName)
    {
        var filePath = AppContext.BaseDirectory + "Images/" + imageName;

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Could not find file at location=" + filePath);
        }

        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task<List<ImageDataModel>> GetImageDataByProductId(long id)
    {
        return await _productsRepo.GetImageDataByProductId((int)id);
    }
}
