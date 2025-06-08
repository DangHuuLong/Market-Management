using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
namespace PBL3_HK4.Entity
{
    public class Customer: User
    {
        [StringLength(200)]
        public string? Address { get; set; }
       
        public int? EarnedPoint { get; set; }
        public virtual ShoppingCart? ShoppingCart { get; set; }
        public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }
}
