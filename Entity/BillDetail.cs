using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Entity
{
    public class BillDetail
    {
        [Key]
        public Guid BillDetailID { get; set; }

        [ForeignKey("Product")]
        public Guid ProductID { get; set; }
        public virtual Product? Product { get; set; }

        [ForeignKey("Bill")]
        public Guid? BillID { get; set; }
        public virtual Bill? Bill { get; set; }

        [ForeignKey("Discount")]
        public Guid? DiscountID { get; set; }
        public virtual Discount? Discount { get; set; }
        public int? Quantity { get; set; }
        public double? Price { get; set; }
        public double Total { get; set; }
    }
}
