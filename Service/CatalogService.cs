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
    public class CatalogService : ICatalogService
    {
        private readonly ApplicationDbContext _context;
        public CatalogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCatalogAsync(Catalog catalog)
        {
            var currentCatalog = await _context.Catalogs.FirstOrDefaultAsync(c => c.CatalogID == catalog.CatalogID);
            if (currentCatalog != null)
            {
                throw new InvalidOperationException($"Catalog with ID {catalog.CatalogID} already exists.");
            }
            await _context.Catalogs.AddAsync(catalog);
            await _context.SaveChangesAsync();
        }

        //public async Task UpdateCatalogAsync(Catalog catalog)
        //{
        //    var currentCatalog = await _context.Catalogs.FirstOrDefaultAsync(c => c.CatalogID == catalog.CatalogID);
        //    if (currentCatalog == null)
        //    {
        //        throw new KeyNotFoundException($"Catalog with ID {catalog.CatalogID} not found.");
        //    }
        //    _context.Catalogs.Update(catalog);
        //    await _context.SaveChangesAsync();
        //}

        public async Task UpdateCatalogAsync(Catalog catalog)
        {
            var currentCatalog = await _context.Catalogs.FirstOrDefaultAsync(c => c.CatalogID == catalog.CatalogID);
            if (currentCatalog == null)
            {
                throw new KeyNotFoundException($"Catalog with ID {catalog.CatalogID} not found.");
            }
            currentCatalog.CatalogName = catalog.CatalogName;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCatalogAsync(Guid catalogId)
        {
            var catalog = await _context.Catalogs.FirstOrDefaultAsync(c => c.CatalogID == catalogId);
            if (catalog == null)
            {
                throw new KeyNotFoundException($"Catalog with ID {catalogId} not found.");
            }
            _context.Catalogs.Remove(catalog);
            await _context.SaveChangesAsync();
        }

        public async Task<Catalog> GetCatalogByIdAsync(Guid catalogId)
        {
            var catalog = await _context.Catalogs.Where(c => c.CatalogID == catalogId).FirstOrDefaultAsync();
            if (catalog == null)
            {
                throw new KeyNotFoundException($"Catalog with ID:{catalogId} not found");
            }
            return catalog;
        }

        public async Task<Catalog> GetCatalogByNameAsync(string catalogName)
        {
            var catalog = await _context.Catalogs.Where(c => c.CatalogName == catalogName).FirstOrDefaultAsync();
            if (catalog == null)
            {
                throw new KeyNotFoundException($"Catalog with name:{catalogName} not found");
            }
            return catalog;
        }

        public async Task<IEnumerable<Catalog>> GetAllCatalogsAsync()
        {
            var listCatalog = await _context.Catalogs.ToListAsync();
            if (listCatalog == null)
            {
                throw new KeyNotFoundException("No catalog found");
            }
            return listCatalog;
        }

        //Task<IEnumerable<Catalog>> ICatalogService.GetAllCatalogsAsync()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
