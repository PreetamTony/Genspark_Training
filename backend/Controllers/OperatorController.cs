using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/operator")]
    [ApiController]
    [Authorize(Roles = "Operator")]
    public class OperatorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OperatorController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        private int GetOperatorProfileId()
        {
            var userId = GetUserId();
            var profile = _context.OperatorProfiles.FirstOrDefault(op => op.UserId == userId);
            return profile?.Id ?? 0;
        }

        // --- Coupon Management ---
        [HttpGet("coupons")]
        public async Task<IActionResult> GetMyCoupons()
        {
            var operatorId = GetOperatorProfileId();
            if (operatorId == 0)
                return Unauthorized(new { message = "Operator profile not found." });

            var coupons = await _context.Coupons
                .Where(c => c.OperatorId == operatorId)
                .Select(c => new
                {
                    c.Id,
                    c.Code,
                    c.DiscountAmount,
                    c.DiscountPercent,
                    c.ValidFrom,
                    c.ValidTo,
                    c.IsActive,
                    UsageCount = _context.Bookings.Count(b => b.Schedule.Bus.OperatorProfileId == operatorId && 
                        b.BookingSeats.Any(bs => bs.Booking.CouponCode == c.Code))
                })
                .ToListAsync();

            return Ok(coupons);
        }

        [HttpPost("coupons")]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto couponDto)
        {
            var operatorId = GetOperatorProfileId();
            if (operatorId == 0)
                return Unauthorized(new { message = "Operator profile not found." });

            // Validate coupon code is unique for this operator
            var exists = await _context.Coupons.AnyAsync(c => c.OperatorId == operatorId && c.Code == couponDto.Code);
            if (exists)
                return BadRequest(new { message = "Coupon code already exists for this operator." });

            // Create a new Coupon entity from the DTO
            var coupon = new Coupon
            {
                Code = couponDto.Code,
                DiscountAmount = couponDto.DiscountAmount,
                DiscountPercent = couponDto.DiscountPercent,
                ValidFrom = couponDto.ValidFrom,
                ValidTo = couponDto.ValidTo,
                IsActive = couponDto.IsActive,
                OperatorId = operatorId
            };
            
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Coupon created.", coupon });
        }

        [HttpPut("coupons/{couponId}")]
        public async Task<IActionResult> UpdateCoupon(int couponId, [FromBody] Coupon coupon)
        {
            var operatorId = GetOperatorProfileId();
            if (operatorId == 0)
                return Unauthorized(new { message = "Operator profile not found." });

            var existingCoupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Id == couponId && c.OperatorId == operatorId);
            if (existingCoupon == null)
                return NotFound(new { message = "Coupon not found." });

            // Ensure coupon code is unique for this operator (excluding current coupon)
            var exists = await _context.Coupons.AnyAsync(c => c.OperatorId == operatorId && c.Code == coupon.Code && c.Id != couponId);
            if (exists)
                return BadRequest(new { message = "Coupon code already exists for this operator." });

            existingCoupon.Code = coupon.Code;
            existingCoupon.DiscountAmount = coupon.DiscountAmount;
            existingCoupon.DiscountPercent = coupon.DiscountPercent;
            existingCoupon.ValidFrom = coupon.ValidFrom;
            existingCoupon.ValidTo = coupon.ValidTo;
            existingCoupon.IsActive = coupon.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Coupon updated.", coupon = existingCoupon });
        }

        [HttpDelete("coupons/{couponId}")]
        public async Task<IActionResult> DeleteCoupon(int couponId)
        {
            var operatorId = GetOperatorProfileId();
            if (operatorId == 0)
                return Unauthorized(new { message = "Operator profile not found." });

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Id == couponId && c.OperatorId == operatorId);
            if (coupon == null)
                return NotFound(new { message = "Coupon not found." });

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Coupon deleted." });
        }
    }
}
