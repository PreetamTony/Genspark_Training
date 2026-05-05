namespace backend.DTOs
{
    public class CreateBookingDto
    {
        public int ScheduleId { get; set; }
        public List<int> SeatIds { get; set; } = new();
        public string? CouponCode { get; set; } // Optional coupon code
        public List<PassengerDto> Passengers { get; set; } = new();
    }

    public class PassengerDto
    {
        public int SeatId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
    }
}