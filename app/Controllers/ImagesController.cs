using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

public class ImagesController : ControllerBase
{
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(ILogger<ImagesController> logger)
    {
        _logger = logger;
    }


    [HttpGet("/images/{imageName}")]
    public async Task<IActionResult> GetImageByName(String imageName)
    {
        //TODO: Change to image service, move this code to new images controller
        var filePath = AppContext.BaseDirectory + "Images/" + imageName;
        var fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileContent, "image/jpeg");
    }
}
