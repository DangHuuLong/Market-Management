using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PBL3_HK4.Interface;
using PBL3_HK4.Entity;
using Microsoft.EntityFrameworkCore;

namespace PBL3_HK4.Service
{
    public class BillService : IBillService
    {
        private readonly ApplicationDbContext _context;
        public BillService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Bill>> GetBillByUserIdAsync(Guid userId)
        {
            // Kiểm tra nếu userId rỗng
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            }

            // Truy vấn tất cả bill của user với userId
            var bills = await _context.Bills
                .Where(b => b.UserID == userId)
                .ToListAsync();

            return bills;
        }

        public async Task<Bill> GetBillByIdAsync(Guid billId)
        {
            var bill = await _context.Bills.FirstOrDefaultAsync(b => b.BillID == billId);
            if (bill == null)
            {
                throw new KeyNotFoundException($"Bill with ID {billId} not found.");
            }
            return bill;
        }

        public async Task<IEnumerable<Bill>> GetAllBillsAsync()
        {
            var listBilll = await _context.Bills.ToListAsync();
            if (listBilll == null)
            {
                throw new KeyNotFoundException("No bills found.");
            }
            return listBilll;
        }

        public async Task AddBillAsync(Bill bill)
        {
            var existingBill = await _context.Bills.FirstOrDefaultAsync(b => b.BillID == bill.BillID);
            if (existingBill != null)
            {
                throw new InvalidOperationException($"Bill with ID {bill.BillID} already exists.");
            }
            await _context.Bills.AddAsync(bill);
            await _context.SaveChangesAsync();
        }

        public async Task ConfirmBillAsync(Guid billid)
        {
            var bill = await _context.Bills.FindAsync(billid);
            if (bill != null)
            {
                if (bill.Status == BillStatus.Unconfirmed)
                {
                    bill.Status = BillStatus.Confirmed;
                }
            }
        }

        public async Task DeleteBillAsync(Guid billId)
        {
            var bill = await _context.Bills.FindAsync(billId);
            if (bill != null)
            {
                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Bill> GetConfirmedBillByIdAsync(Guid billId)
        {
            var bill = await _context.Bills.FirstOrDefaultAsync(b => b.BillID == billId && b.Status == BillStatus.Confirmed);
            if (bill != null)
            {
                return bill;
            }
            else
            {
                throw new KeyNotFoundException($"No confirmed bill with id: {billId} found");
            }
        }

        public async Task<IEnumerable<Bill>> GetConfirmedBillsAsync()
        {
            var Bills = await _context.Bills.Where(b => b.Status == BillStatus.Confirmed).ToListAsync();
            if (Bills == null)
            {
                throw new InvalidOperationException("No confirmed bills found.");
            }
            return Bills;
        }

        public async Task<IEnumerable<Bill>> GetConfirmedBillsByCustomerIdAsync(Guid customerId)
        {
            var bills = await _context.Bills.Where(b => b.UserID == customerId && b.Status == BillStatus.Confirmed).ToListAsync();
            if (bills == null)
            {
                throw new KeyNotFoundException($"Customer: {customerId} has not had any confirmed bills");
            }
            return bills;
        }

        public async Task<IEnumerable<Bill>> GetConfirmedBillsByDateAsync(DateTime date)
        {
            var bills = await _context.Bills.Where(b => b.Date == date && b.Status == BillStatus.Confirmed).ToListAsync();
            if (bills == null)
            {
                throw new KeyNotFoundException($"No confirmed bills found in {date}");
            }
            return bills;
        }

        public async Task<IEnumerable<Bill>> GetUnconfirmedBillAsync()
        {
            var bills = await _context.Bills.Where(b => b.Status == BillStatus.Unconfirmed).ToListAsync();
            if (bills == null)
            {
                throw new InvalidOperationException("No unconfirmed bills found.");
            }
            return bills;
        }

        public async Task UpdateBillAsync(Bill bill)
        {
            var existingBill = await _context.Bills.FirstOrDefaultAsync(b => b.BillID == bill.BillID);
            if (existingBill == null)
            {
                throw new KeyNotFoundException($"Bill with ID {bill.BillID} not found.");
            }
            _context.Bills.Update(bill);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBillConfirmedAsync(Guid billId)
        {
            var existingBill = await _context.Bills.FirstOrDefaultAsync(b => b.BillID == billId);
            if (existingBill == null)
            {
                throw new KeyNotFoundException($"Bill with ID {billId} not found.");
            }
            existingBill.Status = BillStatus.Confirmed;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBillCanceledAsync(Guid billId)
        {
            var existingBill = await _context.Bills.FirstOrDefaultAsync(b => b.BillID == billId);
            if (existingBill == null)
            {
                throw new KeyNotFoundException($"Bill with ID {billId} not found.");
            }
            existingBill.Status = BillStatus.Cancelled;
            await _context.SaveChangesAsync();
        }
    }
}
