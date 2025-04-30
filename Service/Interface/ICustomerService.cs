using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Service.Interface
{
    public interface ICustomerService
    {
        public Task AddCustomerAsync(Customer customer);
        public Task UpdateCustomerAsync(Customer customer);
        public Task<Customer> GetCustomerByIdAsync(Guid customerId);
        public Task<Customer> GetCustomerByUserNameAsync(string name);
        public Task<IEnumerable<Customer>> GetAllCustomerAsync();
    }
}