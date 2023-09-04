using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class UserRoleViewModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

    }

}
