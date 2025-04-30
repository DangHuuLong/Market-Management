using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PBL3_HK4.Entity;

namespace PBL3_HK4.Service.Interface
{
    public interface IBillDetailService
    {
        public Task<IEnumerable<BillDetail>> GetAllBillDetailsAsync();
        public Task<BillDetail> GetBillDetailByIdAsync(Guid id);
        public Task AddBillDetailAsync(BillDetail billDetail);
        public Task UpdateBillDetailAsync(BillDetail billDetail);
        public Task DeleteBillDetailAsync(Guid id);
        public Task<IEnumerable<BillDetail>> GetBillDetailsByBillIdAsync(Guid billId);
    }
}
