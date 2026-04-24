using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [Route("api/coupons")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CouponsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponDto request)
        {
            try
            {
                Console.WriteLine($"=== COUPON VALIDATION DEBUG ===");
                Console.WriteLine($"Coupon Code: {request.CouponCode}");
                Console.WriteLine($"Schedule ID: {request.ScheduleId}");

                // Get the schedule to find the operator
                var schedule = await _context.Schedules
                    .Include(s => s.Bus)
                    .FirstOrDefaultAsync(s => s.Id == request.ScheduleId);

                if (schedule == null)
                {
                    Console.WriteLine("Schedule not found");
                    return BadRequest(new { valid = false, message = "Invalid schedule." });
                }

                Console.WriteLine($"Schedule found - Bus ID: {schedule.Bus.Id}, OperatorProfileId: {schedule.Bus.OperatorProfileId}");

                // Find the coupon for this operator
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == request.CouponCode && 
                                           c.OperatorId == schedule.Bus.OperatorProfileId && 
                                           c.IsActive);

                if (coupon == null)
                {
                    Console.WriteLine($"Coupon not found for operator {schedule.Bus.OperatorProfileId}");
                    
                    // Let's check if the coupon exists at all
                    var anyCoupon = await _context.Coupons
                        .FirstOrDefaultAsync(c => c.Code == request.CouponCode);
                    
                    if (anyCoupon != null)
                    {
                        Console.WriteLine($"Coupon exists but belongs to operator {anyCoupon.OperatorId}, not {schedule.Bus.OperatorProfileId}");
                        return BadRequest(new { valid = false, message = "This coupon is not valid for this bus operator." });
                    }
                    else
                    {
                        Console.WriteLine($"No coupon found with code {request.CouponCode}");
                        return BadRequest(new { valid = false, message = "Invalid or expired coupon code." });
                    }
                }

                // Check if coupon is within validity period
                var now = DateTime.UtcNow;
                if (coupon.ValidFrom.HasValue && coupon.ValidFrom.Value > now)
                {
                    return BadRequest(new { valid = false, message = "Coupon is not yet valid." });
                }

                if (coupon.ValidTo.HasValue && coupon.ValidTo.Value < now)
                {
                    return BadRequest(new { valid = false, message = "Coupon has expired." });
                }

                // Calculate discount
                var basePrice = schedule.BasePrice;
                decimal discount = 0;

                if (coupon.DiscountPercent.HasValue && coupon.DiscountPercent.Value > 0)
                {
                    discount = basePrice * (coupon.DiscountPercent.Value / 100m);
                }
                else
                {
                    discount = coupon.DiscountAmount;
                }

                // Ensure discount doesn't exceed base price
                if (discount > basePrice) discount = basePrice;

                var discountedPrice = basePrice - discount;

                return Ok(new { 
                    valid = true, 
                    message = "Coupon applied successfully.",
                    discount = discount,
                    discountedPrice = discountedPrice,
                    coupon = new
                    {
                        coupon.Code,
                        DiscountAmount = coupon.DiscountAmount,
                        DiscountPercent = coupon.DiscountPercent
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { valid = false, message = "Failed to validate coupon. Please try again." });
            }
        }
    }

    public class ValidateCouponDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public int ScheduleId { get; set; }
    }
}
