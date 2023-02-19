using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class EditRolesViewModel
    {
        public string RoleName { get; set; }
        public List<IdentityRole> Roles { get; set; }
    }
}
