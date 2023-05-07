using E_shopDAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace E_shopDAL
{
    public class EShopDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set;}
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }

        public EShopDbContext(DbContextOptions<EShopDbContext> options) : base(options) 
        {
            Database.EnsureCreated();
        }
    }
}