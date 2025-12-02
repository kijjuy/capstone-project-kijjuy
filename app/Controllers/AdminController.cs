using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using app.Services;
using app.Mappers;
using app.Models;

namespace app.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly IProductsService _productsService;
    private readonly IProductMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        ILogger<AdminController> logger,
        IProductsService productsService,
        IProductMapper mapper,
	UserManager<ApplicationUser> userManager
        )
    {
        _logger = logger;
        _productsService = productsService;
        _mapper = mapper;
	_userManager = userManager;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
    {
        var baseProducts = await _productsService.GetAllProducts(shouldGetUnavailable: true);
        var products = baseProducts
            .Select(async p => await _mapper.IntoViewModel(p))
            .Select(t => t.Result);

        return View("Index", products);
    }

    /**
     * <summary>
     * Gets a list of all users that are not the current user. Used for Users view 
     * that allows admin user to make other users admins.
     * <summary>
     */
    [HttpGet("/admin/users")]
    public async Task<IActionResult> Users() 
    {
	var userViewModels = new List<UserViewModel>();

	foreach(var user in _userManager.Users) 
	{
	    if(user.UserName.Equals(User.Identity.Name)) {
		continue;
	    }

	    bool isAdmin = (await _userManager.GetRolesAsync(user)).Contains("Admin");

	    userViewModels.Add(
		new UserViewModel {
		    UserName = user.UserName,
		    IsAdmin = isAdmin,
	    });
	}

	ApplicationUser sadf = new ApplicationUser();

	return View(userViewModels);
    }

    [HttpGet("/admin/users/makeAdmin/{username}")]
    public async Task<IActionResult> MakeAdmin(String username) {
	var user = await _userManager.FindByNameAsync(username);
	await _userManager.AddToRoleAsync(user, "Admin");

	return RedirectToAction("Users");
    }

    [HttpGet("/admin/users/removeAdmin/{username}")]
    public async Task<IActionResult> RemoveAdmin(String username) {
	if(username.Equals(User.Identity.Name)) {
	    return RedirectToAction("Users");
	}

	var user = await _userManager.FindByNameAsync(username);
	await _userManager.RemoveFromRoleAsync(user, "Admin");

	return RedirectToAction("Users");
    }
}
