namespace WebApplication2.Models
{
    public class BookingOrder
    {
        public int Id { get; set; }
        public int GymId { get; set; }
        public string UserId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime OrderDate { get; set; }
        public int BookingHour { get; set; }
        public int BookedSportid { get; set; }
    }

}
