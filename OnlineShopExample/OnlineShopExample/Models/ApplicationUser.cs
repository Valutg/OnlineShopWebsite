using Microsoft.AspNetCore.Identity;

namespace OnlineShopExample.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
