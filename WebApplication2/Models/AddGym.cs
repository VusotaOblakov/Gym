using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class AddGym
    {
        public string name { get; set; }

        public string description { get; set; }

        public string adress { get; set; }
        public int city_id { get; set; }
        public int startwork { get; set; }
        public int endwork { get; set; }
        public decimal price { get; set; }
        public List<Accessory> Accessories { get; set; } = new List<Accessory>();
        public List<Sport> Sports { get; set; } = new List<Sport>();
    }


}
