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
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            var currentUser = await _context.Users.OfType<Customer>().FirstOrDefaultAsync(u => u.UserID == customer.UserID);
            if (currentUser != null)
            {
                throw new InvalidOperationException($"Customer with ID {customer.UserID} already exists.");
            }
            await _context.Users.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = (Customer)(await _context.Users.FindAsync(customer.UserID));
            if (existingCustomer != null)
            {
                if (customer.Name != null)
                    existingCustomer.Name = customer.Name;

                if (customer.Email != null)
                    existingCustomer.Email = customer.Email;

                if (customer.Phone != null)
                    existingCustomer.Phone = customer.Phone;

                if (customer.DateOfBirth != null)
                    existingCustomer.DateOfBirth = customer.DateOfBirth;

                if (customer.Sex != null)
                    existingCustomer.Sex = customer.Sex;

                if (customer.Address != null)
                    existingCustomer.Address = customer.Address;

                if (customer.PassWord != null)
                    existingCustomer.PassWord = customer.PassWord;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(Guid customerId)
        {
            var customer = await _context.Users.OfType<Customer>().Where(u => u.UserID == customerId).FirstOrDefaultAsync();
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
            }
            return customer;
        }

        public async Task<Customer> GetCustomerByUserNameAsync(string name)
        {
            var customer = await _context.Users.OfType<Customer>().Where(u => u.UserName == name).FirstOrDefaultAsync();
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with name {name} not found.");
            }
            return customer;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomerAsync()
        {
            var customers = await _context.Users.OfType<Customer>().ToListAsync();
            if (customers.Count == 0)
            {
                throw new InvalidOperationException("No customer account created");
            }
            return customers;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingCustomer = await _context.Users.FindAsync(user.UserID);
            if (existingCustomer != null)
            {
                existingCustomer.Name = user.Name;
                existingCustomer.Email = user.Email;
                existingCustomer.Phone = user.Phone;
                existingCustomer.DateOfBirth = user.DateOfBirth;
                existingCustomer.Sex = user.Sex;

                if (user.PassWord != null) existingCustomer.PassWord = user.PassWord;
            }

            await _context.SaveChangesAsync();
        }
    }
}