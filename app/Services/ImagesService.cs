namespace app.Services;

public interface IImagesService
{
    public Task<byte[]> GetImageBytesByName(String imageName);
}

public class LocalImagesService : IImagesService
{
    private readonly ILogger<IImagesService> _logger;

    public LocalImagesService(ILogger<IImagesService> logger)
    {
        _logger = logger;
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
}
