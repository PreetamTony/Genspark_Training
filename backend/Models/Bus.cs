using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Bus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OperatorProfileId { get; set; }
        [ForeignKey("OperatorProfileId")]
        public OperatorProfile Operator { get; set; } = null!;

        [Required]
        public int LayoutId { get; set; }
        [ForeignKey("LayoutId")]
        public Layout Layout { get; set; } = null!;

        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        public BusStatus Status { get; set; } = BusStatus.PendingApproval;

        // Bus Features and Amenities
        public bool HasWaterBottle { get; set; } = true;
        public bool HasBlankets { get; set; } = false;
        public bool HasChargingPoint { get; set; } = true;
        public bool HasCCTV { get; set; } = false;
        public bool HasToilet { get; set; } = false;
        public bool HasWiFi { get; set; } = false;
        public bool HasReadingLight { get; set; } = false;
        public bool HasEmergencyExit { get; set; } = true;
        public bool HasGPS { get; set; } = true;

        // Bus Type Details
        [MaxLength(100)]
        public string BusType { get; set; } = string.Empty;

        // Rating and Performance
        public double Rating { get; set; } = 4.5;
        public int TotalRatings { get; set; } = 0;
        public int OnTimeTrips { get; set; } = 0;
        public int TotalTrips { get; set; } = 0;

        // Policy Information
        public string CancellationPolicy { get; set; } = string.Empty;
        public string ReschedulePolicy { get; set; } = string.Empty;
        public string ChildPolicy { get; set; } = "Children above the age of 3 will need a ticket";
        public string LuggagePolicy { get; set; } = "1 pieces of luggage will be accepted free of charge per passenger. Excess items will be chargeable";
        public string PetPolicy { get; set; } = "Pets are not allowed";
        public string ToiletPolicy { get; set; } = "In Bus Toilet with Only Urinal Facility";
        public string LiquorPolicy { get; set; } = "Carrying or consuming liquor inside the bus is prohibited";
    }
}
