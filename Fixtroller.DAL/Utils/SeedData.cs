using Fixtroller.DAL.Data;
using Fixtroller.DAL.Data.Migrations;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Utils
{
    public class SeedData : ISeedData
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public SeedData(ApplicationDbContext context,
                        RoleManager<IdentityRole> roleManager,
                        UserManager<ApplicationUser> userManager
            )
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task DataSeedingAsync()
        {
            //// تطبيق أي Migrations معلّقة
            //if ((await _context.Database.GetPendingMigrationsAsync()).Any())
            //{
            //    _context.Database.Migrate();
            //}


            //if (!await _context.Tcategories.AnyAsync())
            //{
            //    await _context.Tcategories.AddRangeAsync(
            //   
            //
            //           (   Tcategories    مكان الداتا الاولية لل )
            //
            //
            //     );
            //}

            //await _context.SaveChangesAsync();
        }

        public async Task IdentityDataSeedingAsync()
        {
            if (_context.Database.GetPendingMigrations().Any())
            {
                _context.Database.Migrate();
            }

            // إنشاء الأدوار إذا لم تكن موجودة
            if (!await _roleManager.Roles.AnyAsync())
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("Technician"));
                await _roleManager.CreateAsync(new IdentityRole("Employee"));
                await _roleManager.CreateAsync(new IdentityRole("MaintenanceManager"));
                await _roleManager.CreateAsync(new IdentityRole("SpecialEmployee"));
            }

            if (!await _userManager.Users.AnyAsync())
            {
                // Admin
                var admin = new ApplicationUser
                {
                    Id = "admin-001",
                    UserName = "admin",
                    Email = "admin@sys.com",
                    PhoneNumber = "0590000001",
                    FullName = "System Administrator",
                    Location = "HQ"
                };

                await _userManager.CreateAsync(admin);
                await _userManager.AddToRoleAsync(admin, "Admin");

                // Maintenance Manager
                var manager = new ApplicationUser
                {
                    Id = "manager-001",
                    UserName = "maint.manager",
                    Email = "manager@sys.com",
                    PhoneNumber = "0590000002",
                    FullName = "Maintenance Manager",
                    Location = "HQ"
                };

                await _userManager.CreateAsync(manager);
                await _userManager.AddToRoleAsync(manager, "MaintenanceManager");

                // 5 Technicians
                for (int i = 1; i <= 5; i++)
                {
                    var technician = new ApplicationUser
                    {
                        Id = $"tech-{i:000}",
                        UserName = $"technician{i}",
                        Email = $"technician{i}@sys.com",
                        PhoneNumber = $"05900000{i + 2}",
                        FullName = $"Technician {i}",
                        Location = "Main Branch"
                    };

                    await _userManager.CreateAsync(technician);
                    await _userManager.AddToRoleAsync(technician, "Technician");
                }

                // 10 Employees
                for (int i = 1; i <= 10; i++)
                {
                    var employee = new ApplicationUser
                    {
                        Id = $"emp-{i:000}",
                        UserName = $"employee{i}",
                        Email = $"employee{i}@sys.com",
                        PhoneNumber = $"05900001{i + 10}",
                        FullName = $"Employee {i}",
                        Location = (i <= 5 ? "Branch A" : "Branch B")
                    };

                    await _userManager.CreateAsync(employee);
                    await _userManager.AddToRoleAsync(employee, "Employee");
                }

                //  موظف مميز
                var specialUser = new ApplicationUser
                {
                    Id = "special011",
                    Email = "specialemployee@sys.com",
                    UserName = "specialemployee",
                    PhoneNumber = "0590000020",
                    FullName = "Special Employee",
                    Location = "Main Branch"
                };
                await _userManager.CreateAsync(specialUser);
                await _userManager.AddToRoleAsync(specialUser, "Employee");
                await _userManager.AddToRoleAsync(specialUser, "SpecialEmployee");
            }

            await _context.SaveChangesAsync();
        }

    }

}