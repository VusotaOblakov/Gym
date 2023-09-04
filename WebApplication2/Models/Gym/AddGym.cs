using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class AddGym
    {
        [Required(ErrorMessage = "Enter name")]
        public string? name { get; set; }
        [Required(ErrorMessage = "Enter description")]
        public string? description { get; set; }
        [Required(ErrorMessage = "Enter adress")]
        public string? adress { get; set; }
        [Required(ErrorMessage = "Choose city")]
        public int city_id { get; set; }
        [Required]
        [Range(1, 24, ErrorMessage = "Choose StartWork from 1 to 24")]
        public int startwork { get; set; }
        [Required]
        [Range(1, 24, ErrorMessage = "Choose EndWork from 1 to 24 but < StartWork")]
        public int endwork { get; set; }
        [Required]
        [Range(1, 500, ErrorMessage = "Error. Enter from 1 to 500")]
        public decimal price { get; set; }
        public List<Accessory> Accessories { get; set; } = new List<Accessory>();
        public List<Sport> Sports { get; set; } = new List<Sport>();
    }


}
