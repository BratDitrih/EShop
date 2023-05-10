using System.Collections.Generic;
using System;

namespace E_shopClient.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public bool Completed { get; set; }
        public ICollection<ProductAtCart> Products { get; set; }
    }
}
