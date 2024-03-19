using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Gym
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public string? imagePath { get; set; }
        public string? mapLocate { get; set; }
        public string? adress { get; set; }
        public int city_id { get; set; }
        public string? owner_id { get; set; }
        [Required]
        [Range(1, 500, ErrorMessage = "Error. Enter from 1 to 500")]
        public decimal price { get; set; }
        [Required]
        [Range(1, 24, ErrorMessage = "Choose StartWork from 1 to 24")]
        public int startwork { get; set; }
        [Required]
        [Range(1, 24, ErrorMessage = "Choose EndWork from 1 to 24 but < StartWork")]
        public int endwork { get; set; }
    }


}
