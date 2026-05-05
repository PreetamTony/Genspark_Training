using backend.Models;

namespace backend.DTOs
{
    public class SeatInfoDto
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public SeatStatus Status { get; set; }
        public Gender? PassengerGender { get; set; }
        public string? PassengerName { get; set; }
        public bool IsFemaleOccupied => PassengerGender == Gender.Female;
        public bool IsMaleOccupied => PassengerGender == Gender.Male;
        public bool IsOtherOccupied => PassengerGender == Gender.Other;
    }
}
