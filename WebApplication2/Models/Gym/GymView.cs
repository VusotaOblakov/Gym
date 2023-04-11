namespace WebApplication2.Models
{
    public class GymView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public decimal Price { get; set; }
        public int Startwork { get; set; }
        public int Endwork { get; set; }
        public List<Accessory> Accessories { get; set; }
        public List<Sport> Sports { get; set; }
    }

}
