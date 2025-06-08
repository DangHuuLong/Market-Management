using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Entity
{
    public class Discount
    {
        [Key]
        public Guid DiscountID { get; set; }
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Describe { get; set; }
        [Range(0, 100)]
        public double? DiscountRate { get; set; }
        public string? ApplicableProduct { get; set; }

        [StringLength(500)]
        public string? Requirement { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
