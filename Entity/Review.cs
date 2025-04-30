using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Entity
{
    public class Review
    {
        [Key]
        public Guid ReviewID { get; set; }

        [ForeignKey("Product")]
        public Guid? ProductID { get; set; }
        public virtual Product? Product { get; set; }

        [ForeignKey("Customer")]
        public Guid? UserID { get; set; }
        public virtual Customer? Customer { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [StringLength(100)]
        public string? Text { get; set; }

        [Required]
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
