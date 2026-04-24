using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                // Additional validation
                if (dto == null)
                {
                    return BadRequest(new { message = "Registration data is required." });
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BadRequest(new { message = "Full name is required." });
                }

                if (dto.Name.Length < 2 || dto.Name.Length > 100)
                {
                    return BadRequest(new { message = "Name must be between 2 and 100 characters." });
                }

                if (dto.Role == Role.Operator && string.IsNullOrWhiteSpace(dto.CompanyName))
                {
                    return BadRequest(new { message = "Company name is required for operator registration." });
                }

                if (dto.Role == Role.Operator && dto.CompanyName.Length < 2 || dto.CompanyName?.Length > 100)
                {
                    return BadRequest(new { message = "Company name must be between 2 and 100 characters." });
                }

                if (dto.Role == Role.Operator && !dto.HeadOfficeLocationId.HasValue)
                {
                    return BadRequest(new { message = "Head office location is required for operator registration." });
                }

                if (dto.Role == Role.Operator && dto.HeadOfficeLocationId.HasValue)
                {
                    var headOfficeExists = await _context.Locations.AnyAsync(l => l.Id == dto.HeadOfficeLocationId.Value);
                    if (!headOfficeExists)
                    {
                        return BadRequest(new { message = "Invalid head office location selected." });
                    }
                }

                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                {
                    return Conflict(new { message = "An account with this email already exists." });
                }

                var user = new User
                {
                    Name = dto.Name.Trim(),
                    Email = dto.Email.ToLower().Trim(),
                    Role = dto.Role,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // If registering as Operator, create OperatorProfile
                if (dto.Role == Role.Operator)
                {
                    var profile = new OperatorProfile
                    {
                        UserId = user.Id,
                        CompanyName = dto.CompanyName.Trim(),
                        Status = OperatorStatus.PendingApproval,
                        HeadOfficeLocationId = dto.HeadOfficeLocationId
                    };
                    _context.OperatorProfiles.Add(profile);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { 
                    message = "Registered successfully. " + (dto.Role == Role.Operator ? "Awaiting admin approval." : "You can now login."),
                    userId = user.Id,
                    role = dto.Role.ToString()
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Failed to create account. Please try again." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred during registration." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { message = "Login credentials are required." });
                }

                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest(new { message = "Email and password are required." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
                if (user == null)
                {
                    return Unauthorized(new { message = "Account not found. No user exists with this email address." });
                }

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    return Unauthorized(new { message = "Incorrect password. The password you entered is wrong." });
                }

                // Check if operator is approved
                if (user.Role == Role.Operator)
                {
                    var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(op => op.UserId == user.Id);
                    if (profile == null)
                    {
                        return Unauthorized(new { message = "Operator profile not found. Please contact support." });
                    }
                    if (profile.Status == OperatorStatus.Disabled)
                    {
                        return Unauthorized(new { message = "Your operator account has been disabled. Please contact support." });
                    }
                    if (profile.Status == OperatorStatus.PendingApproval)
                    {
                        return Unauthorized(new { message = "Your operator account is pending admin approval." });
                    }
                    if (profile.Status == OperatorStatus.Rejected)
                    {
                        return Unauthorized(new { message = "Your operator account has been rejected. Please contact support." });
                    }
                }

                var token = GenerateJwtToken(user);

                // Get operatorProfileId if applicable
                int? operatorProfileId = null;
                if (user.Role == Role.Operator)
                {
                    var profile = await _context.OperatorProfiles.FirstOrDefaultAsync(op => op.UserId == user.Id);
                    operatorProfileId = profile?.Id;
                }

                return Ok(new
                {
                    token,
                    user = new { user.Id, user.Name, user.Email, Role = user.Role.ToString(), operatorProfileId },
                    expiresIn = "8 hours"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login. Please try again." });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
