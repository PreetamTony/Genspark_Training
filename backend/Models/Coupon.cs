using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        public decimal DiscountAmount { get; set; } // Flat discount

        public decimal? DiscountPercent { get; set; } // Optional percent discount

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign key to OperatorProfile
        public int OperatorId { get; set; }
        [ForeignKey("OperatorId")]
        public OperatorProfile? Operator { get; set; }
    }
}