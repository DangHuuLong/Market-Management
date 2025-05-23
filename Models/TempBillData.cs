namespace PBL3_HK4.Models
{
    public class TempBillData
    {
        public Guid? DiscountId { get; set; }
        public List<TempCartItem> CartItems { get; set; }
        public double TotalAmount { get; set; }
    }

    public class TempCartItem
    {
        public Guid ProductID { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }

}