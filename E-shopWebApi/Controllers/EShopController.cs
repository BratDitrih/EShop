using E_shopDAL;
using E_shopDAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace E_shopWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EShopController : ControllerBase
    {
        private readonly EShopDbContext _context;

        public EShopController(EShopDbContext context)
        {
            _context = context;
        }

        [HttpGet(Name = "GetAllCutomers")]
        public IEnumerable<Customer> Get()
        {
            return _context.Customers;
        }

        [HttpPost(Name = "AddCustomer")]
        public void Post([FromBody] Customer customer)
        {
            _context.Customers.Add(customer);
            _context.SaveChanges();
        }
    }
}