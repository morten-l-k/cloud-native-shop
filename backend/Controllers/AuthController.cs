using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ShopContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ShopContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public record LoginRequest(string CustomerId, string Password);
        public record LoginResponse(string Token, string CustomerId);

        // POST: auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerId) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("CustomerId and Password are required.");

            var customer = await _context.Customer
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);

            if (customer == null)
                return Unauthorized("Invalid credentials.");

            // Hardcoded password check — replace with proper hashing in production
            if (request.Password != "password")
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(customer.CustomerId);
            return Ok(new LoginResponse(token, customer.CustomerId));
        }

        private string GenerateJwtToken(string customerId)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, customerId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("customer_id", customerId)
            };

            var expiry = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiry),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
