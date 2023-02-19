using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class ProfileModel
    {
        public string Name { get; set; }
        public string Role { get; set; }

        public ProfileModel(IdentityUser user, UserManager<IdentityUser> userManager)
        {
            Name = user.UserName;
            Role = userManager.GetRolesAsync(user).Result.FirstOrDefault();
        }
    }
}
