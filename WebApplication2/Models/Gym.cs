namespace WebApplication2.Models
{
    public class Gym
    {
        public int id { get; set; }
        public string name { get; set; }

        public string description { get; set; }

        public string adress { get; set; }
        public int city_id { get; set; }
        public string owner_id { get; set; }
        public decimal price  { get; set; }
        public int startwork { get; set; }
        public int endwork { get; set; }

    }


}
