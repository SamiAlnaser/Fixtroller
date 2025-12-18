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
            // 0) تأكد من المايغريشن

            // 1) فئات الفنيين (5 كاتيجوري) + الترجمات
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
            }
        };

                await _context.Tcategories.AddRangeAsync(categories);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var types = ex.Entries
                        .Select(e => $"{e.Entity.GetType().Name} ({e.State})")
                        .Distinct()
                        .ToList();

                    Console.WriteLine("Concurrency on: " + string.Join(", ", types));
                    throw;
                }
            }

            // 2) أنواع المشاكل (20 نوع) + الترجمات (منها "أخرى / Other")
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
                    new ProblemTypeTranslation { Language = "en", Name = "Lighting Issue" }
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
                    new ProblemTypeTranslation { Language = "ar", Name = "صوت مزعج من المكيف" },
                    new ProblemTypeTranslation { Language = "en", Name = "AC Noise" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "ضغط مياه ضعيف" },
                    new ProblemTypeTranslation { Language = "en", Name = "Low Water Pressure" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "عطل في السخان" },
                    new ProblemTypeTranslation { Language = "en", Name = "Water Heater Issue" }
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
                    new ProblemTypeTranslation { Language = "ar", Name = "نافذة مكسورة" },
                    new ProblemTypeTranslation { Language = "en", Name = "Broken Window" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "جدار يحتاج دهان" },
                    new ProblemTypeTranslation { Language = "en", Name = "Wall Repainting" }
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
                    new ProblemTypeTranslation { Language = "ar", Name = "بطء في الجهاز" },
                    new ProblemTypeTranslation { Language = "en", Name = "Slow Computer" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "مشكلة في الشبكة" },
                    new ProblemTypeTranslation { Language = "en", Name = "Network Issue" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "مشكلة في الطابعة" },
                    new ProblemTypeTranslation { Language = "en", Name = "Printer Issue" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "طلب تنظيف عادي" },
                    new ProblemTypeTranslation { Language = "en", Name = "Regular Cleaning" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "تنظيف عميق" },
                    new ProblemTypeTranslation { Language = "en", Name = "Deep Cleaning" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "رائحة كريهة في الغرفة" },
                    new ProblemTypeTranslation { Language = "en", Name = "Bad Odor in Room" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "تسريب من المكيف" },
                    new ProblemTypeTranslation { Language = "en", Name = "AC Water Leak" }
                }
            },
            new ProblemType
            {
                Translations = new List<ProblemTypeTranslation>
                {
                    new ProblemTypeTranslation { Language = "ar", Name = "أخرى" },
                    new ProblemTypeTranslation { Language = "en", Name = "Other" }
                }
            }
        };

                await _context.PTypes.AddRangeAsync(problemTypes);
                await _context.SaveChangesAsync();
            }

            // 3) توزيع الكاتيجوري على الفنين (بدون SaveChanges على context مباشرة)
            var categoriesList = await _context.Tcategories.OrderBy(c => c.Id).ToListAsync();
            var technicians = (await _userManager.GetUsersInRoleAsync("Technician"))
                .OrderBy(u => u.Id)
                .ToList();

            if (categoriesList.Count > 0 && technicians.Count > 0)
            {
                for (int i = 0; i < technicians.Count; i++)
                {
                    var cat = categoriesList[i % categoriesList.Count];
                    technicians[i].TechnicianCategoryId = cat.Id;

                    // 👈 نستخدم UserManager للتحديث، مش _context.SaveChangesAsync
                    await _userManager.UpdateAsync(technicians[i]);
                }
            }

            // 4) طلبات الصيانة (100 طلب) – ندخلها فقط إذا الجدول فاضي
            if (await _context.MaintenanceRequests.AnyAsync())
                return;

            var problemTypesAll = await _context.PTypes
                .Include(p => p.Translations)
                .OrderBy(p => p.Id)
                .ToListAsync();

            var employees = (await _userManager.GetUsersInRoleAsync("Employee"))
                .OrderBy(u => u.Id)
                .ToList();

            var techsForRequests = technicians;

            var adminUser = await _userManager.FindByIdAsync("admin-001");
            var managerUser = await _userManager.FindByIdAsync("manager-001");

            if (problemTypesAll.Count == 0 || employees.Count == 0 || techsForRequests.Count == 0)
                return;

            var caseTypes = new[]
            {
        CaseType.Submitted,
        CaseType.ManagerReview,
        CaseType.Processing,
        CaseType.ResourcesNeeded,
        CaseType.Modified,
        CaseType.Processed,
        CaseType.Reopened,
        CaseType.Completed,
        CaseType.Cancelled
    };

            var priorities = new[] { Priority.Low, Priority.Medium, Priority.High };

            var arabicAddresses = new[]
            {
        "المقر الرئيسي - الطابق الأول",
        "المقر الرئيسي - الطابق الثاني",
        "فرع A - الطابق الأرضي",
        "فرع A - مكتب 10",
        "فرع B - الطابق الثالث"
    };

            var englishAddresses = new[]
            {
        "HQ - Floor 1",
        "HQ - Floor 2",
        "Branch A - Ground Floor",
        "Branch A - Office 10",
        "Branch B - Floor 3"
    };

            var now = DateTime.UtcNow;
            var random = new Random(123);

            var requests = new List<MaintenanceRequest>();
            var workEntries = new List<WorkTimeEntry>();

            for (int i = 0; i < 100; i++)
            {
                bool isEnglish = i < 30; // أول 30 طلب بالإنجليزي

                var owner = employees[i % employees.Count];

                string createdByUserId;
                if (i % 10 == 0 && managerUser != null)
                    createdByUserId = managerUser.Id;
                else if (i % 15 == 0 && adminUser != null)
                    createdByUserId = adminUser.Id;
                else if (i % 7 == 0)
                    createdByUserId = techsForRequests[i % techsForRequests.Count].Id;
                else
                    createdByUserId = owner.Id;

                var pt = problemTypesAll[i % problemTypesAll.Count];
                var caseType = caseTypes[i % caseTypes.Length];
                var priority = priorities[i % priorities.Length];

                var arName = pt.Translations.FirstOrDefault(t => t.Language == "ar")?.Name
                             ?? pt.Translations.FirstOrDefault()?.Name
                             ?? "مشكلة عامة";

                var enName = pt.Translations.FirstOrDefault(t => t.Language == "en")?.Name
                             ?? pt.Translations.FirstOrDefault()?.Name
                             ?? "General Issue";

                string title;
                string description;
                string address;

                if (isEnglish)
                {
                    title = $"{enName} - Request #{i + 1}";
                    description = $"Auto-seeded maintenance request #{i + 1} in status '{caseType}'.";
                    address = englishAddresses[i % englishAddresses.Length];
                }
                else
                {
                    title = $"{arName} - طلب رقم {i + 1}";
                    description = $"طلب صيانة تجريبي رقم {i + 1} بالحالة {caseType} لاختبار النظام.";
                    address = arabicAddresses[i % arabicAddresses.Length];
                }

                var createdAt = now
                    .AddDays(-random.Next(1, 60))
                    .AddHours(-random.Next(0, 8));

                var request = new MaintenanceRequest
                {
                    Title = title,
                    Description = description,
                    Address = address,
                    Priority = priority,
                    ProblemTypeId = pt.Id,
                    CaseType = caseType,
                    OwnerUserId = owner.Id,
                    CreatedByUserId = createdByUserId,
                    CreatedAt = createdAt
                };

                // صورة أساسية
                request.Images.Add(new MaintenanceRequestImage
                {
                    FileName = isEnglish
                        ? $"seed-en-{i + 1:000}.jpg"
                        : $"seed-ar-{i + 1:000}.jpg",
                    IsPrimary = true,
                    Source = MaintenanceRequestImageSource.RequestCreation
                });

                // صورة إضافية لبعض الطلبات
                if (i % 5 == 0)
                {
                    request.Images.Add(new MaintenanceRequestImage
                    {
                        FileName = $"seed-extra-{i + 1:000}.jpg",
                        IsPrimary = false,
                        Source = MaintenanceRequestImageSource.StaffAttachment
                    });
                }

                // ملاحظات
                if (caseType == CaseType.Reopened)
                {
                    request.Notes.Add(new MaintenanceNote
                    {
                        Text = isEnglish
                            ? "Ticket reopened because the issue is still happening."
                            : "تم إعادة فتح الطلب لأن المشكلة ما زالت مستمرة.",
                        Type = NoteType.ReopenReason,
                        Author = NoteAuthor.Owner,
                        CreatedByUserId = owner.Id
                    });
                }
                else if (caseType == CaseType.ResourcesNeeded)
                {
                    request.Notes.Add(new MaintenanceNote
                    {
                        Text = isEnglish
                            ? "Additional materials or another technician are required to continue."
                            : "نحتاج إلى مواد إضافية أو فني آخر لاستكمال العمل.",
                        Type = NoteType.HelpRequest,
                        Author = NoteAuthor.Owner,
                        CreatedByUserId = owner.Id
                    });
                }
                else if (i % 6 == 0)
                {
                    request.Notes.Add(new MaintenanceNote
                    {
                        Text = isEnglish
                            ? "General note added for testing purposes."
                            : "ملاحظة عامة مضافة لأغراض الاختبار.",
                        Type = NoteType.General,
                        Author = NoteAuthor.Owner,
                        CreatedByUserId = owner.Id
                    });
                }

                // تعيين فني (مش كل الطلبات)
                bool assignTechnician =
                    caseType != CaseType.Submitted &&
                    caseType != CaseType.Cancelled &&
                    (i % 4 != 0);

                if (assignTechnician)
                {
                    var tech = techsForRequests[i % techsForRequests.Count];
                    var assignedAt = createdAt.AddHours(1);

                    var link = new MaintenanceRequestTechnician
                    {
                        TechnicianUserId = tech.Id,
                        AssignedAtUtc = assignedAt,
                        ExpectedDuration = 2 + (i % 6)
                    };

                    if (i % 9 == 0 &&
                        caseType != CaseType.Completed &&
                        caseType != CaseType.Processed)
                    {
                        link.UnassignedAtUtc = assignedAt.AddHours(2);
                    }

                    request.Technicians.Add(link);

                    // WorkTimeEntries للحالات اللي فيها شغل حقيقي
                    if (caseType == CaseType.Processing ||
                        caseType == CaseType.Processed ||
                        caseType == CaseType.Completed)
                    {
                        var start = new DateTimeOffset(assignedAt);
                        DateTimeOffset? stop = null;

                        if (caseType == CaseType.Processed || caseType == CaseType.Completed)
                        {
                            stop = start.AddHours(1 + (i % 4));
                        }

                        workEntries.Add(new WorkTimeEntry
                        {
                            Request = request,
                            TechnicianUserId = tech.Id,
                            StartedAt = start,
                            StoppedAt = stop
                        });
                    }
                }

                if (caseType == CaseType.Completed ||
                    caseType == CaseType.Cancelled ||
                    caseType == CaseType.Processed)
                {
                    request.ClosedAtUtc = createdAt
                        .AddDays(random.Next(0, 5))
                        .AddHours(random.Next(1, 4));
                }

                requests.Add(request);
            }

            await _context.MaintenanceRequests.AddRangeAsync(requests);

            if (workEntries.Count > 0)
                await _context.WorkTimeEntries.AddRangeAsync(workEntries);

            await _context.SaveChangesAsync();
        }





        public async Task IdentityDataSeedingAsync()
        {

            // 1) Ensure roles (4 roles only)
            var roles = new[] { "Admin", "Technician", "Employee", "MaintenanceManager" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2) Seed users only if DB is empty
            if (!await _userManager.Users.AnyAsync())
            {
                const string adminPassword = "Admin#12345";
                const string managerPassword = "Manager#12345";
                const string technicianPassword = "Tech#12345";
                const string employeePassword = "Employee#12345";

                // Admin - يوسف دراغمة
                var admin = new ApplicationUser
                {
                    Id = "admin-001",
                    UserName = "admin",
                    Email = "admin@fixtroller.com",
                    EmailConfirmed = true,
                    PhoneNumber = "0590000001",
                    FullNameAr = "يوسف دراغمة",
                    FullNameEn = "Yousef Daraghmeh",
                    Location = "المقر الرئيسي",
                    Department = "الإدارة"
                };
                var r1 = await _userManager.CreateAsync(admin, adminPassword);
                if (!r1.Succeeded) throw new Exception(string.Join(" | ", r1.Errors.Select(e => e.Description)));

                var rr1 = await _userManager.AddToRoleAsync(admin, "Admin");
                if (!rr1.Succeeded) throw new Exception(string.Join(" | ", rr1.Errors.Select(e => e.Description)));

                // Maintenance Manager - حسن دراغمة
                var manager = new ApplicationUser
                {
                    Id = "manager-001",
                    UserName = "manager",
                    Email = "manager@fixtroller.com",
                    EmailConfirmed = true,
                    PhoneNumber = "0590000002",
                    FullNameAr = "حسن دراغمة",
                    FullNameEn = "Hassan Daraghmeh",
                    Location = "المقر الرئيسي",
                    Department = "قسم الصيانة"
                };

                var m1 = await _userManager.CreateAsync(manager, managerPassword);
                if (!m1.Succeeded) throw new Exception(string.Join(" | ", m1.Errors.Select(e => e.Description)));

                var mm1 = await _userManager.AddToRoleAsync(manager, "MaintenanceManager");
                if (!mm1.Succeeded) throw new Exception(string.Join(" | ", mm1.Errors.Select(e => e.Description)));

                // 20 Technicians (6 أسماء حقيقية + 14 توليد تلقائي)
                var technicians = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = "tech-001",
                UserName = "sami.alnser",
                Email = "samialnser@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "0590000101",
                FullNameAr = "سامي النصر",
                FullNameEn = "Sami Al-Nser",
                Location = "فرع A",
                Department = "الصيانة"
            },
            new ApplicationUser
            {
                Id = "tech-002",
                UserName = "ahmed.joaidi",
                Email = "ajaidi258@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "0590000102",
                FullNameAr = "احمد جعيدي",
                FullNameEn = "Ahmed Juaidi",
                Location = "فرع A",
                Department = "الصيانة"
            },
            new ApplicationUser
            {
                Id = "tech-003",
                UserName = "anas.abuhamed",
                Email = "anasabuhamed07@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "0590000103",
                FullNameAr = "انس ابوحامد",
                FullNameEn = "Anas Abuhamed",
                Location = "فرع B",
                Department = "الصيانة"
            },
            new ApplicationUser
            {
                Id = "tech-004",
                UserName = "abed.hejazi",
                Email = "hejaziabed4@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "0590000104",
                FullNameAr = "عبد الرحمن حجازي",
                FullNameEn = "Abed Alrahman Hejazi",
                Location = "فرع B",
                Department = "الصيانة"
            },
            new ApplicationUser
            {
                Id = "tech-005",
                UserName = "mohammad.nour",
                Email = "Mohammadnour20212003@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "0590000105",
                FullNameAr = "محمد نور",
                FullNameEn = "Mohammad Nour",
                Location = "فرع A",
                Department = "الصيانة"
            },
            new ApplicationUser
            {
                Id = "tech-006",
                UserName = "anas.malhem",
                Email = "anas.malhem@fixtroller.com",
                EmailConfirmed = true,
                PhoneNumber = "0590000106",
                FullNameAr = "انس ملحم",
                FullNameEn = "Anas Malhem",
                Location = "فرع B",
                Department = "الصيانة"
            }
        };

                // Generate remaining technicians
                for (int i = 7; i <= 20; i++)
                {
                    technicians.Add(new ApplicationUser
                    {
                        Id = $"tech-{i:000}",
                        UserName = $"technician{i}",
                        Email = $"technician{i}@fixtroller.com",
                        EmailConfirmed = true,
                        PhoneNumber = $"0590000{i + 100}",
                        FullNameAr = $"فني {i}",
                        FullNameEn = $"Technician {i}",
                        Location = (i % 2 == 0) ? "فرع A" : "فرع B",
                        Department = "الصيانة"
                    });
                }

                foreach (var tech in technicians)
                {
                    var t1 = await _userManager.CreateAsync(tech, technicianPassword);
                    if (!t1.Succeeded) throw new Exception(string.Join(" | ", t1.Errors.Select(e => e.Description)));

                    var tt1 = await _userManager.AddToRoleAsync(tech, "Technician");
                    if (!tt1.Succeeded) throw new Exception(string.Join(" | ", tt1.Errors.Select(e => e.Description)));
                }

                // 100 Employees
                var employees = new List<ApplicationUser>();
                for (int i = 1; i <= 100; i++)
                {
                    employees.Add(new ApplicationUser
                    {
                        Id = $"emp-{i:000}",
                        UserName = $"employee{i}",
                        Email = $"employee{i}@fixtroller.com",
                        EmailConfirmed = true,
                        PhoneNumber = $"0590002{i:000}",
                        FullNameAr = $"موظف {i}",
                        FullNameEn = $"Employee {i}",
                        Location = (i <= 50) ? "فرع A" : "فرع B",
                        Department = "الموارد البشرية"
                    });
                }

                foreach (var emp in employees)
                {
                    var e1 = await _userManager.CreateAsync(emp, employeePassword);
                    if (!e1.Succeeded) throw new Exception(string.Join(" | ", e1.Errors.Select(e => e.Description)));

                    var ee1 = await _userManager.AddToRoleAsync(emp, "Employee");
                    if (!ee1.Succeeded) throw new Exception(string.Join(" | ", ee1.Errors.Select(e => e.Description)));
                }

            }

        }


    }

}