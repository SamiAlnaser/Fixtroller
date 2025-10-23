using Fixtroller.DAL.Data;
using Fixtroller.DAL.Data.Migrations;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
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
            // تطبيق أي Migrations معلّقة
            if ((await _context.Database.GetPendingMigrationsAsync()).Any())
            {
                _context.Database.Migrate();
            }

            // 1) فئات الفنيين + الترجمات
            if (!await _context.Tcategories.AnyAsync())
            {
                var categories = new List<TechnicianCategory>
        {
            new TechnicianCategory
            {
                Translations = new List<TechnicianCategoryTranslation>
                {
                    new TechnicianCategoryTranslation { Language = "ar", Name = "كهرباء" },
                    new TechnicianCategoryTranslation { Language = "en", Name = "Electrical" }
                }
            },
            new TechnicianCategory
            {
                Translations = new List<TechnicianCategoryTranslation>
                {
                    new TechnicianCategoryTranslation { Language = "ar", Name = "سباكة" },
                    new TechnicianCategoryTranslation { Language = "en", Name = "Plumbing" }
                }
            },
            new TechnicianCategory
            {
                Translations = new List<TechnicianCategoryTranslation>
                {
                    new TechnicianCategoryTranslation { Language = "ar", Name = "تكييف وتبريد" },
                    new TechnicianCategoryTranslation { Language = "en", Name = "HVAC" }
                }
            },
            new TechnicianCategory
            {
                Translations = new List<TechnicianCategoryTranslation>
                {
                    new TechnicianCategoryTranslation { Language = "ar", Name = "نجارة" },
                    new TechnicianCategoryTranslation { Language = "en", Name = "Carpentry" }
                }
            },
            new TechnicianCategory
            {
                Translations = new List<TechnicianCategoryTranslation>
                {
                    new TechnicianCategoryTranslation { Language = "ar", Name = "دعم تقني" },
                    new TechnicianCategoryTranslation { Language = "en", Name = "IT Support" }
                }
            },
            new TechnicianCategory
            {
                Translations = new List<TechnicianCategoryTranslation>
                {
                    new TechnicianCategoryTranslation { Language = "ar", Name = "تنظيف" },
                    new TechnicianCategoryTranslation { Language = "en", Name = "Cleaning" }
                }
            }
        };

                await _context.Tcategories.AddRangeAsync(categories);
                await _context.SaveChangesAsync();
            }

            // 2) أنواع المشاكل + الترجمات
            if (!await _context.PTypes.AnyAsync())
            {
                var problemTypes = new List<ProblemType>
        {
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "انقطاع كهرباء" },
                    new ProblemTypeTranslation { Language = "en", Name = "Power Outage" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "عطل في الإنارة" },
                    new ProblemTypeTranslation { Language = "en", Name = "Light Fixture Issue" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "تسريب مياه" },
                    new ProblemTypeTranslation { Language = "en", Name = "Water Leakage" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "انسداد مجاري" },
                    new ProblemTypeTranslation { Language = "en", Name = "Clogged Drain" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "التكييف لا يبرّد" },
                    new ProblemTypeTranslation { Language = "en", Name = "AC Not Cooling" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "عطل باب" },
                    new ProblemTypeTranslation { Language = "en", Name = "Door Broken" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "كمبيوتر لا يعمل" },
                    new ProblemTypeTranslation { Language = "en", Name = "PC Not Booting" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "عطل طابعة" },
                    new ProblemTypeTranslation { Language = "en", Name = "Printer Jam" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "تنظيف عميق" },
                    new ProblemTypeTranslation { Language = "en", Name = "Deep Cleaning" }
                }
            }
        };

                await _context.PTypes.AddRangeAsync(problemTypes);
                await _context.SaveChangesAsync();
            }

            // 3) ربط الفنيين بفئاتهم (لو المستخدمين موجودين من الهوية)
            // يدوّي: tech-001..tech-005
            var cats = await _context.Tcategories
                                     .OrderBy(c => c.Id)
                                     .ToListAsync();

            var techUsers = await _userManager.Users
                .Where(u => u.Id.StartsWith("tech-"))
                .ToListAsync();

            if (techUsers.Count > 0 && cats.Count > 0)
            {
                for (int i = 0; i < techUsers.Count; i++)
                {
                    var cat = cats[i % cats.Count];
                    techUsers[i].TechnicianCategoryId = cat.Id;
                }
                await _context.SaveChangesAsync();
            }

            // 4) طلبات صيانة شاملة السيناريوهات
            if (!await _context.MaintenanceRequests.AnyAsync())
            {
                // جِب نوع مشكلة لكل فئة بشكل تقريبي
                var pt = await _context.PTypes
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                string Emp(int i) => $"emp-{i:000}";
                string Tech(int i) => $"tech-{i:000}";

                var now = DateTime.UtcNow;

                var requests = new List<MaintenanceRequest>
        {
            // Submitted (غير مسند)
            new MaintenanceRequest
            {
                Title = "انقطاع كهرباء في الطابق الثاني",
                Description = "القواطع تفصل عند تشغيل المكيفات",
                Priority = Priority.High,
                Address = "Branch A - Floor 2",
                ProblemTypeId = pt[0].Id, // Power Outage
                CaseType = CaseType.Submitted,
                CreatedByUserId = Emp(1),
                CreatedAt = now.AddDays(-5),
                Images = new List<MaintenanceRequestImage>
                {
                    new MaintenanceRequestImage { FileName = "seed-electric-1.jpg", IsPrimary = true }
                }
            },

            // ManagerReview
            new MaintenanceRequest
            {
                Title = "تسريب تحت المغسلة",
                Description = "تسريب بسيط يحتاج رباط",
                Priority = Priority.Medium,
                Address = "Branch A - Kitchen",
                ProblemTypeId = pt[2].Id, // Leakage
                CaseType = CaseType.ManagerReview,
                CreatedByUserId = Emp(2),
                CreatedAt = now.AddDays(-4),
                Images = new List<MaintenanceRequestImage>
                {
                    new MaintenanceRequestImage { FileName = "seed-plumbing-1.jpg", IsPrimary = true }
                }
            },

            // Processing (مُسند مع وقت إسناد)
            new MaintenanceRequest
            {
                Title = "المكيف لا يبرد",
                Description = "صوت مرتفع وتبريد ضعيف",
                Priority = Priority.High,
                Address = "Branch B - Server Room",
                ProblemTypeId = pt[4].Id, // AC Not Cooling
                CaseType = CaseType.Processing,
                CreatedByUserId = Emp(3),
                CreatedAt = now.AddDays(-3),
                AssignedTechnicianUserId = Tech(1),
                AssignedAtUtc = now.AddDays(-3).AddHours(2),
                Images = new List<MaintenanceRequestImage>
                {
                    new MaintenanceRequestImage { FileName = "seed-hvac-1.jpg", IsPrimary = true },
                    new MaintenanceRequestImage { FileName = "seed-hvac-2.jpg", IsPrimary = false }
                }
            },

            // ResourcesNeeded (مطلوب موارد)
            new MaintenanceRequest
            {
                Title = "كسر في باب المخزن",
                Description = "المفصلات متضررة وتحتاج تبديل",
                Priority = Priority.Medium,
                Address = "Branch B - Storage",
                ProblemTypeId = pt[5].Id, // Door Broken
                CaseType = CaseType.ResourcesNeeded,
                CreatedByUserId = Emp(4),
                CreatedAt = now.AddDays(-7),
                AssignedTechnicianUserId = Tech(2),
                AssignedAtUtc = now.AddDays(-7).AddHours(1),
                Images = new List<MaintenanceRequestImage>
                {
                    new MaintenanceRequestImage { FileName = "seed-carpentry-1.jpg", IsPrimary = true }
                }
            },

            // Processed
            new MaintenanceRequest
            {
                Title = "انسداد مجاري في الحمام",
                Description = "الانسداد متكرر، يرجى المعالجة الجذرية",
                Priority = Priority.High,
                Address = "Branch A - WC",
                ProblemTypeId = pt[3].Id, // Clogged Drain
                CaseType = CaseType.Processed,
                CreatedByUserId = Emp(5),
                CreatedAt = now.AddDays(-10),
                AssignedTechnicianUserId = Tech(3),
                AssignedAtUtc = now.AddDays(-10).AddHours(3),
                Images = new List<MaintenanceRequestImage>
                {
                    new MaintenanceRequestImage { FileName = "seed-plumbing-2.jpg", IsPrimary = true }
                }
            },

            // Modified (مالك عدّل الطلب)
            new MaintenanceRequest
            {
                Title = "عطل طابعة الطابق الأول",
                Description = "توقف التغذية بالأوراق",
                Priority = Priority.Low,
                Address = "Branch B - Floor 1",
                ProblemTypeId = pt[7].Id, // Printer Jam
                CaseType = CaseType.Modified,
                CreatedByUserId = Emp(6),
                CreatedAt = now.AddDays(-2),
                Images = new List<MaintenanceRequestImage>
                {
                    new MaintenanceRequestImage { FileName = "seed-it-1.jpg", IsPrimary = true }
                }
            },

            // Reopened
            new MaintenanceRequest
            {
                Title = "رجوع مشكلة الكهرباء بعد الإصلاح",
                Description = "المشكلة عادت عند تشغيل أجهزة إضافية",
                Priority = Priority.High,
                Address = "Branch A - Floor 3",
                ProblemTypeId = pt[1].Id, // Light Fixture Issue (تقريبًا كهرباء)
                CaseType = CaseType.Reopened,
                CreatedByUserId = Emp(7),
                CreatedAt = now.AddDays(-1),
                AssignedTechnicianUserId = Tech(1),
                AssignedAtUtc = now.AddDays(-1).AddHours(1),
                Images = new List<MaintenanceRequestImage>
                {
                    new MaintenanceRequestImage { FileName = "seed-electric-2.jpg", IsPrimary = true }
                }
            },

            // Completed
            new MaintenanceRequest
            {
                Title = "تنظيف عميق لقاعة الاجتماعات",
                Description = "تنظيف سجاد ونوافذ",
                Priority = Priority.Low,
                Address = "HQ - Meeting Room",
                ProblemTypeId = pt[8].Id, // Deep Cleaning
                CaseType = CaseType.Completed,
                CreatedByUserId = Emp(8),
                CreatedAt = now.AddDays(-12),
                AssignedTechnicianUserId = Tech(4),
                AssignedAtUtc = now.AddDays(-12).AddHours(2),
                Images = new List<MaintenanceRequestImage>
                {
                    new MaintenanceRequestImage { FileName = "seed-cleaning-1.jpg", IsPrimary = true }
                }
            },

            // Cancelled
            new MaintenanceRequest
            {
                Title = "كمبيوتر لا يعمل",
                Description = "الجهاز لا يقلع إطلاقًا",
                Priority = Priority.Medium,
                Address = "Branch B - Office 203",
                ProblemTypeId = pt[6].Id, // PC Not Booting
                CaseType = CaseType.Cancelled,
                CreatedByUserId = Emp(9),
                CreatedAt = now.AddDays(-6),
                Images = new List<MaintenanceRequestImage>
                {
                    new MaintenanceRequestImage { FileName = "seed-it-2.jpg", IsPrimary = true }
                }
            }
        };

                await _context.MaintenanceRequests.AddRangeAsync(requests);
                await _context.SaveChangesAsync();
            }
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