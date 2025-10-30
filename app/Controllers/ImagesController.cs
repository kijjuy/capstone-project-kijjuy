using Microsoft.AspNetCore.Mvc;
using app.Services;

namespace app.Controllers;

public class ImagesController : ControllerBase
{
    private readonly ILogger<ImagesController> _logger;
    private readonly IImagesService _imagesService;

    public ImagesController(ILogger<ImagesController> logger,
	    IImagesService imagesService)
    {
        _logger = logger;
	_imagesService = imagesService;
    }


    /**
     * <summary>
     * Streams the image file that has matching name <paramref name="imageName"/>.
     * Returns NotFound if no file with name=<paramref name="imageName"/> exists.
     * </summary>
     */
    [HttpGet("/images/{imageName}")]
    public async Task<IActionResult> GetImageByName(String imageName)
    {
	byte[] fileContent;
	try {
	    fileContent = await _imagesService.GetImageBytesByName(imageName);
	} catch(Exception e) {
	    if(!e.GetType().Equals(typeof(FileNotFoundException))) {
		throw;
	    }
	    _logger.LogError($"Image with name={imageName} not found. Error={e.Message}");
	    return NotFound();
	}
        return File(fileContent, "image/jpeg");
    }
}
