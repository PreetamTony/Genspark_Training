namespace backend.DTOs
{
    public class CreateBookingDto
    {
        public int ScheduleId { get; set; }
        public List<int> SeatIds { get; set; } = new();
        public string? CouponCode { get; set; } // Optional coupon code
    }
}