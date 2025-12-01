using Microsoft.AspNetCore.Http;

namespace tests;

public static class ImageHelper
{
    /**
     * <summary>
     * Creates a list of fake IFormFile images, list is of size <paramref name="count"/>.
     * </summary>
     */
    public static async Task<List<IFormFile>> CreateFakeImages(int count)
    {
        var files = new List<IFormFile>();
        for (int i = 0; i < count; i++)
        {
            var name = "fake_image" + i;
            var fileName = name + ".jpeg";

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync("Fake image data " + i);
            await writer.FlushAsync();
            stream.Position = 0;

            var file = new FormFile(stream, 0, stream.Length, name, fileName);
            file.Headers = new HeaderDictionary();
            file.ContentType = "image/jpeg";
            files.Add(file);
        }
        return files;
    }
}
