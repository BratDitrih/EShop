namespace E_shopWebApi.Models
{
    public class OrderModel
    {
        public int CustomerId { get; set; }
        public ICollection<Products> Products { get; set; }
    }
}
