using System.ComponentModel.DataAnnotations;

namespace E_shopDAL.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public uint QuantityInStock { get; set; }
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
