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
	try 
	{
	    fileContent = await _imagesService.GetImageBytesByName(imageName);
	    return File(fileContent, "image/jpeg");
	} catch(Exception e) 
	{
	    _logger.LogWarning($"Image with name={imageName} not found. Error={e.Message}");
	}

	try
	{
	    fileContent = await _imagesService.GetImageBytesByName("placeholder.jpg");
	    return File(fileContent, "image/jpeg");
	} catch(Exception e)
	{
	    _logger.LogError("Could not stream placeholder image.");
	    _logger.LogError(e.Message);
	}
	return NotFound();
    }
}
