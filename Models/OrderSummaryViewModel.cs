using PBL3_HK4.Entity;

namespace PBL3_HK4.Models
{
    public class OrderSummaryViewModel
    {
        public Customer Customer {  get; set; }
        public ShoppingCart ShoppingCart { get; set; }
        public IEnumerable<Product> Products {  get; set; }
        public IEnumerable<Discount> Discounts { get; set; }
        public Product GetProductByCartId(Guid productId)
        {
            var product = Products.FirstOrDefault(p => p.ProductID == productId);
            return product;
        }

    }
}
