using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class EditRolesViewModel
    {
        [Required]
        public string roleName { get; set; }
        public List<IdentityRole> Roles { get; set; }
    }
}
