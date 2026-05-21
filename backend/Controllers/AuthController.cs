using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using CloudNativeShop.Backend.Models;
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
        public record RegisterCustomerRequest(string Id, string Password, string CustomerZipCodePrefix, string CustomerCity, string CustomerState, string FirstName, string LastName, string EmailAddress, string StreetAddress);
        public record RegisterSellerRequest(string Id, string Password, string SellerZipCodePrefix, string SellerCity, string SellerState);

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

        // POST: auth/register/customer
        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Id and Password are required.");

            // Check if customer ID already exists
            var existingCustomer = await _context.Customer
                .FirstOrDefaultAsync(c => c.CustomerId == request.Id);

            if (existingCustomer != null)
                return BadRequest("Customer ID already exists.");

            // Create new Customer object
            var customer = new Customer
            {
                CustomerId = request.Id,
                CustomerPassword = request.Password,
                CustomerZipCodePrefix = request.CustomerZipCodePrefix,
                CustomerCity = request.CustomerCity,
                CustomerState = request.CustomerState,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailAddress = request.EmailAddress,
                StreetAddress = request.StreetAddress
            };

            _context.Customer.Add(customer);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(customer.CustomerId, "customer");
            return Ok(new LoginResponse(token, customer.CustomerId, "customer"));

        }

        // POST: auth/register/seller
        [HttpPost("register/seller")]
        public async Task<IActionResult> RegisterSeller([FromBody] RegisterSellerRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Id and Password are required.");

            // Check if seller ID already exists
            var existingSeller = await _context.Seller
                .FirstOrDefaultAsync(s => s.SellerId == request.Id);

            if (existingSeller != null)
                return BadRequest("Seller ID already exists.");

            // Create new Seller object
            var seller = new Seller
            {
                SellerId = request.Id,
                SellerZipCodePrefix = request.SellerZipCodePrefix,
                SellerCity = request.SellerCity,
                SellerState = request.SellerState
            };

            _context.Seller.Add(seller);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(seller.SellerId, "seller");
            return Ok(new LoginResponse(token, seller.SellerId, "seller"));
        }


    }
}