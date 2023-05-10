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
    [Authorize]
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
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Customer newCustomer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    Customer customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == newCustomer.Email);

                    if (customer != null) return Conflict("Пользователь с таким Email уже существует");

                    _dbContext.Customers.Add(newCustomer);
                    await _dbContext.SaveChangesAsync();

                    var token = GenerateJwtToken(await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == newCustomer.Email));

                    await transaction.CommitAsync();

                    return Ok(new { Token = token });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Внутрення ошибка сервера: " + ex.Message);
                }
            }
        }

        private string GenerateJwtToken(Customer customer)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        [HttpGet]
        [Route("/products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _dbContext.Products.ToListAsync();
            return Ok(products);
        }

        [HttpPost]
        [Route("/makeOrder")]
        public async Task<IActionResult> MakeOrder([FromBody] OrderModel model)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == model.CustomerId);

                    var order = new Order
                    {
                        OrderDate = DateTime.UtcNow,
                        Completed = false,
                        Customer = customer
                    };
                    _dbContext.Orders.Add(order);
                    await _dbContext.SaveChangesAsync();

                    foreach (var item in model.Products)
                    {
                        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                        if (product.QuantityInStock < item.Quantity)
                        {
                            return BadRequest("Недостаточно товара на складе");
                        }
                        var orderDetail = new OrderDetail
                        {
                            Product = product,
                            Quantity = item.Quantity,
                            Order = order
                        };
                        _dbContext.OrderDetails.Add(orderDetail);

                        product.QuantityInStock -= item.Quantity;
                    }
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Внутрення ошибка сервера: " + ex.Message);
                }
            }
        }
    }
}