using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PBL3_HK4.Entity;
using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Service.Interface;

namespace PBL3_HK4.Service
{
    public class BillDetailService : IBillDetailService
    {
        private readonly ApplicationDbContext _context;

        public BillDetailService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BillDetail>> GetAllBillDetailsAsync()
        {
            var listBillDetail = await _context.BillDetails.ToListAsync();
            if (listBillDetail == null || listBillDetail.Count == 0)
            {
                throw new KeyNotFoundException("No bill details found.");
            }
            return listBillDetail;
        }

        public async Task<BillDetail> GetBillDetailByIdAsync(Guid billdetailId)
        {
            var billDetail = await _context.BillDetails.FirstOrDefaultAsync(b => b.BillDetailID == billdetailId);
            if (billDetail == null)
            {
                throw new KeyNotFoundException($"Bill detail with ID {billdetailId} not found.");
            }
            return billDetail;
        }

        public async Task<IEnumerable<BillDetail>> GetBillDetailsByBillIdAsync(Guid billid)
        {
            var billDetails = await _context.BillDetails.Where(b => b.BillID == billid).ToListAsync();
            if (billDetails == null || billDetails.Count == 0)
            {
                throw new KeyNotFoundException($"No bill details found for bill ID {billid}.");
            }
            return billDetails;
        }

        public async Task AddBillDetailAsync(BillDetail billDetail)
        {
            var existingBillDetail = await _context.BillDetails.FirstOrDefaultAsync(b => b.BillDetailID == billDetail.BillDetailID);
            if (existingBillDetail != null)
            {
                throw new InvalidOperationException($"Bill detail with ID {billDetail.BillDetailID} already exists.");
            }
            await _context.BillDetails.AddAsync(billDetail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBillDetailAsync(BillDetail billDetail)
        {
            var existingBillDetail = await _context.BillDetails.FirstOrDefaultAsync(b => b.BillDetailID == billDetail.BillDetailID);
            if (existingBillDetail == null)
            {
                throw new KeyNotFoundException($"Bill detail with ID {billDetail.BillDetailID} not found.");
            }
            _context.BillDetails.Update(billDetail);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBillDetailAsync(Guid billId)
        {
            var billDetail = await _context.BillDetails.FirstOrDefaultAsync(b => b.BillDetailID == billId);
            if (billDetail == null)
            {
                throw new KeyNotFoundException($"Bill detail with ID {billId} not found.");
            }
            _context.BillDetails.Remove(billDetail);
            await _context.SaveChangesAsync();
        }

    }
}

