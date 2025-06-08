using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Entity
{
    public class CartItem
    {
        [Key]
        public Guid ItemID { get; set; }
        [ForeignKey("Product")]
        public Guid ProductID { get; set; }
        public virtual Product? Product { get; set; }
        [ForeignKey("ShoppingCart")]
        public Guid? CartID { get; set; }
        public virtual ShoppingCart? ShoppingCart { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public double Price { get; set; }

        [NotMapped]
        public double TotalPrice
        {
            get
            {
                return this.Quantity * this.Price;
            }
            set { }
        }
    }
}
