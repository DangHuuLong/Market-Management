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
    public interface IBillService
    {
        public Task<IEnumerable<Bill>> GetBillByUserIdAsync(Guid userId);
        public Task<IEnumerable<Bill>> GetAllBillsAsync();
        Task<Bill> GetBillByIdAsync(Guid billId);
        public Task<IEnumerable<Bill>> GetConfirmedBillsAsync();
        public Task<Bill> GetConfirmedBillByIdAsync(Guid billId);
        public Task<IEnumerable<Bill>> GetConfirmedBillsByCustomerIdAsync(Guid customerId);
        public Task<IEnumerable<Bill>> GetConfirmedBillsByDateAsync(DateTime date);
        public Task AddBillAsync(Bill bill);
        public Task UpdateBillAsync(Bill bill);
        public Task DeleteBillAsync(Guid billId);
        public Task ConfirmBillAsync(Guid billid);
        public Task<IEnumerable<Bill>> GetUnconfirmedBillAsync();
        public Task UpdateBillConfirmedAsync(Guid billId);
        public Task UpdateBillCanceledAsync(Guid billId);
        public Task UpdateBillReceivedAsync(Guid billId);
        public Task UpdateBillReviewedAsync(Guid billId);
    }
}
