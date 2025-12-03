using Microsoft.AspNetCore.Identity;

namespace app.Models;

public class ApplicationUser : IdentityUser
{

    public String Address { get; set; }

    public List<long> Cart { get; set; }

    public ApplicationUser()
    : base()
    {
        Cart = new List<long>();
    }
}
