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

        public record LoginRequest(string Id, string Password);
        public record LoginResponse(string Token, string Id, string Role);

        // POST: auth/login/customer
        [HttpPost("login/customer")]
        public async Task<IActionResult> LoginCustomer([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Id and Password are required.");

            var customer = await _context.Customer
                .FirstOrDefaultAsync(c => c.CustomerId == request.Id);

            if (customer == null || request.Password != "password")
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(customer.CustomerId, "customer");
            return Ok(new LoginResponse(token, customer.CustomerId, "customer"));
        }

        // POST: auth/login/seller
        [HttpPost("login/seller")]
        public async Task<IActionResult> LoginSeller([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Id and Password are required.");

            var seller = await _context.Seller
                .FirstOrDefaultAsync(s => s.SellerId == request.Id);

            if (seller == null || request.Password != "password")
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(seller.SellerId, "seller");
            return Ok(new LoginResponse(token, seller.SellerId, "seller"));
        }

        private string GenerateJwtToken(string id, string role)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("user_id", id),
                new Claim(ClaimTypes.Role, role)
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
