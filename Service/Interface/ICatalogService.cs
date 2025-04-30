using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Service.Interface
{
    public interface ICatalogService
    {
        public Task AddCatalogAsync(Catalog catalog);
        public Task UpdateCatalogAsync(Catalog catalog);
        public Task DeleteCatalogAsync(Guid catalogId);
        public Task<Catalog> GetCatalogByIdAsync(Guid catalogId);
        public Task<Catalog> GetCatalogByNameAsync(string catalogName);
        public Task<IEnumerable<Catalog>> GetAllCatalogsAsync();
    }
}
