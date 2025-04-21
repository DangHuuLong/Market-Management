using PBL3_HK4.Entity;

namespace PBL3_HK4.Models
{
    public class CustomerOrderModelView
    {
        public Customer Customer {  get; set; }
        public IEnumerable<Bill> Bills { get; set; }
        public IEnumerable<BillDetail> BillDetails { get; set; }
        public IEnumerable<Product> Products { get; set; }  
    }
}
