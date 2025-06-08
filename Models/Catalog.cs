using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Entity
{
    public class Catalog
    {
        [Key]
        [Required]
        public Guid CatalogID { get; set; }
        [StringLength(40)]
        public string? CatalogName { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
