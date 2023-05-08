using E_shopDAL;
using E_shopDAL.Models;
using E_shopWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace E_shopWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EShopController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EShopDbContext _dbContext;

        public EShopController(IConfiguration configuration, EShopDbContext dBcontext)
        {
            _configuration = configuration;
            _dbContext = dBcontext;
        }

        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                Customer customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == model.Email && c.Password == model.Password);

                if (customer == null) return NotFound("Неправильный логин или пароль");

                var token = GenerateJwtToken(customer);

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутрення ошибка сервера: " + ex.Message);
            }
        }

        [HttpPost]
        [Route("/register")]
        public async Task<IActionResult> Register([FromBody] Customer newCustomer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                Customer customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == newCustomer.Email);

                if (customer != null) return Conflict("Пользователь с таким Email уже существует");

                var token = GenerateJwtToken(newCustomer);

                _dbContext.Customers.Add(newCustomer);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутрення ошибка сервера: " + ex.Message);
            }
        }

        private string GenerateJwtToken(Customer customer)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, customer.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        [HttpGet]
        [Route("/products")]
        [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _dbContext.Products.ToListAsync();
            return Ok(products);
        }

        [HttpPost]
        [Route("/makeOrder")]
        [Authorize]
        public async Task<IActionResult> MakeOrder([FromBody] Order model)
        { 
            // TODO

            return Ok();
        }
    }
}