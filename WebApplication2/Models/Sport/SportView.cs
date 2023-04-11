using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class SportView
    {
        [Required]
        public string SportName { get; set; }
        public List<Sport> Sports { get; set; }
    }
}
