using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
namespace PBL3_HK4.Entity
{
    public class User
    {
        [Key]
        public Guid UserID { get; set; }

        [StringLength(40)]
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(10)]
        public string? Sex { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [StringLength(40)]
        public string? UserName { get; set; }

        [StringLength(500)]
        public string? PassWord { get; set; }
        public string? Role { get; set; }

        [NotMapped]
        public string? NewPassWord { get; set; }

        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }
    }
}