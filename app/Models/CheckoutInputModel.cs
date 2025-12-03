using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class CheckoutInputModel 
{
    [Display(Name = "Name")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
    public String Name { get; set; }

    [Display(Name = "Address")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
    public String Address { get; set; }
}
