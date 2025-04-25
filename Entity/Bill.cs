using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
namespace PBL3_HK4.Entity
{
    public enum BillStatus
    {
        Unconfirmed = 0,
        Confirmed = 1,
        Cancelled = 2,
        Received = 3
    }
    public class Bill
    {
        [Key]
        public Guid BillID { get; set; }

        [ForeignKey("Customer")]
        public Guid? UserID { get; set; }
        public virtual Customer? Customer { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }


        [StringLength(200)]
        public string? Address { get; set; }

        public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();

        [Range(0.01, double.MaxValue)]
        public double TotalPrice
        {
            get => BillDetails?.Sum(b => b.Total) ?? 0;
            set { }
        }
        public BillStatus Status { get; set; } = BillStatus.Unconfirmed;
    }
}