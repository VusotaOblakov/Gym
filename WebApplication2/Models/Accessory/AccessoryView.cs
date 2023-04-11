
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class AccessoryView
    {
        [Required]
        public string AccessoryName { get; set; }
        public List<Accessory> Accessories { get; set; }
    }
}
