using PBL3_HK4.Entity;

namespace PBL3_HK4.Models
{
    public class ReviewsViewModel
    {
        public Customer Customer { get; set; }
        public List<Product> Products { get; set; }
        public Guid BillId { get; set; }
    }
}
