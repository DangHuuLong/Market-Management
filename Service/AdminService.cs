﻿using System.ComponentModel.DataAnnotations;
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
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAdminAsync(Admin admin)
        {
            var currentUser = await _context.Users
                .Where(u => u.UserID == admin.UserID && u.Role == "Admin")
                .FirstOrDefaultAsync();

            if (currentUser != null)
            {
                throw new InvalidOperationException($"Admin with ID {admin.UserID} already exists.");
            }

            await _context.Users.AddAsync(admin);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAdminAsync(Admin admin)
        {
            var currentUser = await _context.Users
                .Where(u => u.UserID == admin.UserID && u.Role == "Admin")
                .FirstOrDefaultAsync();

            if (currentUser == null)
            {
                throw new KeyNotFoundException($"Admin with ID {admin.UserID} not found.");
            }

            currentUser.Name = admin.Name;
            currentUser.Email = admin.Email;
            currentUser.Sex = admin.Sex;
            currentUser.Phone = admin.Phone;
            currentUser.DateOfBirth = admin.DateOfBirth;

            await _context.SaveChangesAsync();
        }

        public async Task<Admin> GetAdminByIdAsync(Guid adminId)
        {
            var admin = await _context.Users
                .Where(u => u.UserID == adminId && u.Role == "Admin")
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                throw new KeyNotFoundException($"Admin with ID {adminId} not found.");
            }

            return admin as Admin;
        }

        public async Task<Admin> GetAdminByUserNameAsync(string name)
        {
            var admin = await _context.Users
                .Where(u => u.UserName == name && u.Role == "Admin")
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                throw new KeyNotFoundException($"Admin with name {name} not found.");
            }

            if (admin is Admin)
            {
                return admin as Admin;
            }
            else
            {
                Admin mappedAdmin = new Admin
                {
                    UserID = admin.UserID,
                    Name = admin.Name,
                    Email = admin.Email,
                    Sex = admin.Sex,
                    Phone = admin.Phone,
                    DateOfBirth = admin.DateOfBirth,
                    UserName = admin.UserName,
                    PassWord = admin.PassWord,
                    Role = admin.Role
                };
                return mappedAdmin;
            }
        }
    }
}