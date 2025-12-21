using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Utils
{
    public class SeedData : ISeedData
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public SeedData(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // =========================================================
        // 1) Seeding للهويّات (Roles + Users)
        // =========================================================
        public async Task IdentityDataSeedingAsync()
        {
            // الأدوار الأساسية
            var roles = new[] { "Admin", "MaintenanceManager", "Technician", "Employee" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // لو فيه يوزرز خلاص ما نعيد
            if (await _userManager.Users.AnyAsync())
                return;

            // باسورد موحّد لكل المستخدمين
            const string DefaultPassword = "Passw0rd!";

            // دالة مساعدة لتحويل الـ UserName لاسم إنجليزي بسيط
            string ToEn(string userName)
            {
                if (string.IsNullOrWhiteSpace(userName))
                    return userName;

                var parts = userName.Split('.', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    var p = parts[i];
                    if (p.Length == 0) continue;
                    parts[i] = char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : string.Empty);
                }
                return string.Join(" ", parts);
            }

            // -----------------------------------------------------
            // Admin: يوسف دراغمة
            // -----------------------------------------------------
            var admin = new ApplicationUser
            {
                UserName = "yousef.daraghmeh",
                Email = "yousef.daraghmeh@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "0590000001",
                FullNameAr = "يوسف دراغمة",
                FullNameEn = "Yousef Daraghmeh",
                Location = "المقر الرئيسي",
                Department = "الإدارة"
            };

            var adminResult = await _userManager.CreateAsync(admin, DefaultPassword);
            if (!adminResult.Succeeded)
                throw new Exception(string.Join(" | ", adminResult.Errors.Select(e => e.Description)));

            var adminRoleResult = await _userManager.AddToRoleAsync(admin, "Admin");
            if (!adminRoleResult.Succeeded)
                throw new Exception(string.Join(" | ", adminRoleResult.Errors.Select(e => e.Description)));

            // -----------------------------------------------------
            // Maintenance Manager: حسن دراغمة
            // -----------------------------------------------------
            var manager = new ApplicationUser
            {
                UserName = "hassan.daraghmeh",
                Email = "hassan.daraghmeh@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "0590000002",
                FullNameAr = "حسن دراغمة",
                FullNameEn = "Hassan Daraghmeh",
                Location = "المقر الرئيسي",
                Department = "قسم الصيانة"
            };

            var managerResult = await _userManager.CreateAsync(manager, DefaultPassword);
            if (!managerResult.Succeeded)
                throw new Exception(string.Join(" | ", managerResult.Errors.Select(e => e.Description)));

            var managerRoleResult = await _userManager.AddToRoleAsync(manager, "MaintenanceManager");
            if (!managerRoleResult.Succeeded)
                throw new Exception(string.Join(" | ", managerRoleResult.Errors.Select(e => e.Description)));

            // -----------------------------------------------------
            // Technicians (الفنيين)
            // -----------------------------------------------------
            var techniciansData = new[]
            {
                new { UserName = "ahmed.joaidi",      Email = "ahmed.joaidi@gmail.com",      FullNameAr = "احمد جعيدي" },
                new { UserName = "sami.shreim",       Email = "sami.shreim@gmail.com",       FullNameAr = "سامي شريم" },
                new { UserName = "anas.abuhamed",     Email = "anas.abuhamed@gmail.com",     FullNameAr = "انس ابو حامد" },
                new { UserName = "mohammad.nour",     Email = "mohammad.nour@gmail.com",     FullNameAr = "محمد نور" },
                new { UserName = "abdelrahman.hejazi",Email = "abdelrahman.hejazi@gmail.com",FullNameAr = "عبدالرحمن حجازي" },
                new { UserName = "mahmoud.hamdan",    Email = "mahmoud.hamdan@gmail.com",    FullNameAr = "محمود حمدان" },
                new { UserName = "jawad.qablawi",     Email = "jawad.qablawi@gmail.com",     FullNameAr = "جواد قبلاوي" },
                new { UserName = "obada.naffaa",      Email = "obada.naffaa@gmail.com",      FullNameAr = "عبادة نفاع" },
                new { UserName = "tareq.qashou",      Email = "tareq.qashou@gmail.com",      FullNameAr = "طارق قشوع" },
                new { UserName = "yaaqoub.sweilem",   Email = "yaaqoub.sweleim@gmail.com",   FullNameAr = "يعقوب سويلم" },
                new { UserName = "bahjat.yaseen",     Email = "bahjat.yaseen@gmail.com",     FullNameAr = "بهجت ياسين" },
                new { UserName = "mohammad.barham",   Email = "mohammad.barham@gmail.com",   FullNameAr = "محمد برهم" },
                new { UserName = "ahmed.dalil",       Email = "ahmed.dalil@gmail.com",       FullNameAr = "احمد دليل" },
                new { UserName = "islam.zed",         Email = "islam.zed@gmail.com",         FullNameAr = "اسلام زيد" },
                new { UserName = "ahmed.hamdan",      Email = "ahmed.hamdan@gmail.com",      FullNameAr = "احمد حمدان" },
                new { UserName = "yazen.joaidi",      Email = "yazen.joaidi@gmail.com",      FullNameAr = "يزن جعيدي" },
                new { UserName = "osaid.alhassan",    Email = "osaid.alhassan@gmail.com",    FullNameAr = "اسيد الحسن" },
                new { UserName = "qasem.alqasem",     Email = "qasem.alqasem@gmail.com",     FullNameAr = "قاسم القاسم" },
                new { UserName = "shareef.nazzal",    Email = "shareef.nazzal@gmail.com",    FullNameAr = "شريف نزال" },
                new { UserName = "majd.hijjawi",      Email = "majd.hijjawi@gmail.com",      FullNameAr = "مجد حجاوي" },
                new { UserName = "alaa.dawood",       Email = "alaa.dawood@gmail.com",       FullNameAr = "علاء داود" },
                new { UserName = "abood.eid",         Email = "abood.eid@gmail.com",         FullNameAr = "عبود عيد" },
                new { UserName = "mosa.zaben",        Email = "mosa.zaben@gmail.com",        FullNameAr = "موسى زبن" },
                new { UserName = "meshal.nazzal",     Email = "meshal.nazzal@gmail.com",     FullNameAr = "مشعل نزال" },
                new { UserName = "muhanad.shawahneh", Email = "muhanad.shawahneh@gmail.com", FullNameAr = "مهند شواهنة" },
                new { UserName = "ahmed.madani",      Email = "ahmed.madani@gmail.com",      FullNameAr = "احمد مدني" },
                new { UserName = "amro.jarrar",       Email = "amro.jarrar@gmail.com",       FullNameAr = "عمرو جرار" },
            };

            int phoneTechBase = 101;
            foreach (var t in techniciansData.Select((data, idx) => new { data, idx }))
            {
                var techUser = new ApplicationUser
                {
                    UserName = t.data.UserName,
                    Email = t.data.Email,
                    EmailConfirmed = true,
                    PhoneNumber = $"0590000{phoneTechBase + t.idx}",
                    FullNameAr = t.data.FullNameAr,
                    FullNameEn = ToEn(t.data.UserName),
                    Location = (t.idx % 2 == 0) ? "فرع A" : "فرع B",
                    Department = "قسم الصيانة"
                };

                var createRes = await _userManager.CreateAsync(techUser, DefaultPassword);
                if (!createRes.Succeeded)
                    throw new Exception(string.Join(" | ", createRes.Errors.Select(e => e.Description)));

                var roleRes = await _userManager.AddToRoleAsync(techUser, "Technician");
                if (!roleRes.Succeeded)
                    throw new Exception(string.Join(" | ", roleRes.Errors.Select(e => e.Description)));
            }

            // -----------------------------------------------------
            // Employees (الموظفين)
            // -----------------------------------------------------
            var employeesData = new[]
            {
                new { UserName = "anas.malhem",      Email = "anas.malhem@gmail.com",      FullNameAr = "انس ملحم" },
                new { UserName = "mohammad.khalil",  Email = "mohammad.khalil@gmail.com",  FullNameAr = "محمد خليل" },
                new { UserName = "moatamad.khateeb", Email = "moatamad.khateeb@gmail.com", FullNameAr = "معتمد خطيب" },
                new { UserName = "jaafar.masri",     Email = "jaafar.masri@gmail.com",     FullNameAr = "جعفر مصري" },
                new { UserName = "mohammad.aboali",  Email = "mohammad.aboali@gmail.com",  FullNameAr = "محمد ابو علي" },
                new { UserName = "mohammad.amer",    Email = "mohammad.amer@gmail.com",    FullNameAr = "محمد عامر" },
                new { UserName = "rami.dabie",       Email = "rami.dabie@gmail.com",       FullNameAr = "رامي دبعي" },
                new { UserName = "mofeed.alawneh",   Email = "mofeed.alawneh@gmail.com",   FullNameAr = "مفيد علاونة" },
                new { UserName = "hassan.khader",    Email = "hassan.khader@gmail.com",    FullNameAr = "حسن خضر" },
                new { UserName = "yazeed.sleit",     Email = "yazeed.sleit@gmail.com",     FullNameAr = "يزيد سليط" },
                new { UserName = "nadeem.shreim",    Email = "nadeem.shreim@gmail.com",    FullNameAr = "نديم شريم" },
                new { UserName = "mohammad.nofal",   Email = "mohammad.nofal@gmail.com",   FullNameAr = "محمد نوفل" },
                new { UserName = "yassin.ashqar",    Email = "yassin.ashqar@gmail.com",    FullNameAr = "ياسين اشقر" },
                new { UserName = "islam.mismar",     Email = "islam.mismar@gmail.com",     FullNameAr = "اسلام مسمار" },
                new { UserName = "alaa.rabayaa",     Email = "alaa.rabayaa@gmail.com",     FullNameAr = "علاء ربايعة" },
                new { UserName = "tareq.omar",       Email = "tareq.omar@gmail.com",       FullNameAr = "طارق عمر" },
                new { UserName = "yousef.jaber",     Email = "yousef.jaber@gmail.com",     FullNameAr = "يوسف جابر" },
                new { UserName = "ahmed.toama",      Email = "ahmed.toama@gmail.com",      FullNameAr = "احمد طعمة" },
                new { UserName = "yazen.mohammad",   Email = "yazen.mohammad@gmail.com",   FullNameAr = "يزن محمد" },
                new { UserName = "nour.faisal",      Email = "nour.faisal@gmail.com",      FullNameAr = "نور فيصل" },
                new { UserName = "akram.azzam",      Email = "akram.azzam@gmail.com",      FullNameAr = "اكرم عزام" },
                new { UserName = "mostafa.arafat",   Email = "mostafa.arafat@gmail.com",   FullNameAr = "مصطفى عرفات" },
                new { UserName = "baha.odeh",        Email = "baha.odeh@gmail.com",        FullNameAr = "بهاء عودة" },
                new { UserName = "amro.radwan",      Email = "amro.radwan@gmail.com",      FullNameAr = "عمرو رضوان" },
                new { UserName = "ahmed.barham",     Email = "ahmed.barham@gmail.com",     FullNameAr = "احمد برهم" },
                new { UserName = "odai.user",        Email = "odai.user@gmail.com",        FullNameAr = "عدي" },
            };

            int phoneEmpBase = 201;
            foreach (var e in employeesData.Select((data, idx) => new { data, idx }))
            {
                var empUser = new ApplicationUser
                {
                    UserName = e.data.UserName,
                    Email = e.data.Email,
                    EmailConfirmed = true,
                    PhoneNumber = $"0590002{phoneEmpBase + e.idx}",
                    FullNameAr = e.data.FullNameAr,
                    FullNameEn = ToEn(e.data.UserName),
                    Location = (e.idx < employeesData.Length / 2) ? "فرع A" : "فرع B",
                    Department = "قسم شؤون الموظفين"
                };

                var createRes = await _userManager.CreateAsync(empUser, DefaultPassword);
                if (!createRes.Succeeded)
                    throw new Exception(string.Join(" | ", createRes.Errors.Select(x => x.Description)));

                var roleRes = await _userManager.AddToRoleAsync(empUser, "Employee");
                if (!roleRes.Succeeded)
                    throw new Exception(string.Join(" | ", roleRes.Errors.Select(x => x.Description)));
            }
        }

        // =========================================================
        // 2) Seeding للداتا التشغيلية (كاتيجوريات + Problems + Requests + Notifications)
        // =========================================================
        public async Task DataSeedingAsync()
        {
            // -----------------------------------------------------
            // 1) Technician Categories (7) مع الترجمات
            // -----------------------------------------------------
            if (!await _context.Tcategories.AnyAsync())
            {
                var categories = new List<TechnicianCategory>
                {
                    new TechnicianCategory
                    {
                        Translations = new List<TechnicianCategoryTranslation>
                        {
                            new TechnicianCategoryTranslation { Language = "ar", Name = "الأعطال الكهربائية" },
                            new TechnicianCategoryTranslation { Language = "en", Name = "Electrical Faults" }
                        }
                    },
                    new TechnicianCategory
                    {
                        Translations = new List<TechnicianCategoryTranslation>
                        {
                            new TechnicianCategoryTranslation { Language = "ar", Name = "أعطال الشبكات والإنترنت" },
                            new TechnicianCategoryTranslation { Language = "en", Name = "Network & Internet Issues" }
                        }
                    },
                    new TechnicianCategory
                    {
                        Translations = new List<TechnicianCategoryTranslation>
                        {
                            new TechnicianCategoryTranslation { Language = "ar", Name = "صيانة الحاسوب والأنظمة" },
                            new TechnicianCategoryTranslation { Language = "en", Name = "Computer & Systems Maintenance" }
                        }
                    },
                    new TechnicianCategory
                    {
                        Translations = new List<TechnicianCategoryTranslation>
                        {
                            new TechnicianCategoryTranslation { Language = "ar", Name = "أعمال السباكة والصرف الصحي" },
                            new TechnicianCategoryTranslation { Language = "en", Name = "Plumbing & Drainage Works" }
                        }
                    },
                    new TechnicianCategory
                    {
                        Translations = new List<TechnicianCategoryTranslation>
                        {
                            new TechnicianCategoryTranslation { Language = "ar", Name = "أعمال التكييف والتبريد" },
                            new TechnicianCategoryTranslation { Language = "en", Name = "HVAC (Air Conditioning & Cooling)" }
                        }
                    },
                    new TechnicianCategory
                    {
                        Translations = new List<TechnicianCategoryTranslation>
                        {
                            new TechnicianCategoryTranslation { Language = "ar", Name = "صيانة المباني والإنشاءات" },
                            new TechnicianCategoryTranslation { Language = "en", Name = "Building & Construction Maintenance" }
                        }
                    },
                    new TechnicianCategory
                    {
                        Translations = new List<TechnicianCategoryTranslation>
                        {
                            new TechnicianCategoryTranslation { Language = "ar", Name = "الأثاث المكتبي" },
                            new TechnicianCategoryTranslation { Language = "en", Name = "Office Furniture" }
                        }
                    }
                };

                await _context.Tcategories.AddRangeAsync(categories);
                await _context.SaveChangesAsync();
            }

            // -----------------------------------------------------
            // 2) ProblemTypes (حسب القائمة) + ترجمات
            // -----------------------------------------------------
            if (!await _context.PTypes.AnyAsync())
            {
                var ptypes = new List<ProblemType>
                {
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "الأمن والسلامة" },
                        new ProblemTypeTranslation { Language = "en", Name = "Safety & Security" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "النظافة وإدارة المرافق" },
                        new ProblemTypeTranslation { Language = "en", Name = "Cleaning & Facilities Management" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "صيانة المختبرات والمعدات التقنية" },
                        new ProblemTypeTranslation { Language = "en", Name = "Lab & Technical Equipment Maintenance" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "أعطال تشغيلية" },
                        new ProblemTypeTranslation { Language = "en", Name = "Operational Faults" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "تسجيل وتأمين" },
                        new ProblemTypeTranslation { Language = "en", Name = "Logging & Securing" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "صيانة سيارات المؤسسة" },
                        new ProblemTypeTranslation { Language = "en", Name = "Institution Vehicles Maintenance" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "أجهزة حساسة (Sensors – PLC – Instruments)" },
                        new ProblemTypeTranslation { Language = "en", Name = "Sensitive Devices (Sensors – PLC – Instruments)" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "معدات مخبرية" },
                        new ProblemTypeTranslation { Language = "en", Name = "Laboratory Equipment" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "أجهزة القياس" },
                        new ProblemTypeTranslation { Language = "en", Name = "Measuring Devices" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "تجديد مواد استهلاكية" },
                        new ProblemTypeTranslation { Language = "en", Name = "Consumables Renewal" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "فحص أجهزة" },
                        new ProblemTypeTranslation { Language = "en", Name = "Devices Check" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "نفايات" },
                        new ProblemTypeTranslation { Language = "en", Name = "Waste Handling" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "ترتيب القاعات" },
                        new ProblemTypeTranslation { Language = "en", Name = "Halls Arrangement" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "نظافة عامة" },
                        new ProblemTypeTranslation { Language = "en", Name = "General Cleaning" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "إضاءة الطوارئ" },
                        new ProblemTypeTranslation { Language = "en", Name = "Emergency Lighting" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "نظام إنذار الحريق" },
                        new ProblemTypeTranslation { Language = "en", Name = "Fire Alarm System" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "مكاتب وكراسي" },
                        new ProblemTypeTranslation { Language = "en", Name = "Desks & Chairs" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "المصاعد" },
                        new ProblemTypeTranslation { Language = "en", Name = "Elevators" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "البلاط والأرضيات" },
                        new ProblemTypeTranslation { Language = "en", Name = "Tiles & Flooring" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "الجدران والدهان" },
                        new ProblemTypeTranslation { Language = "en", Name = "Walls & Painting" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "المكيفات" },
                        new ProblemTypeTranslation { Language = "en", Name = "Air Conditioners" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "وحدات التبريد" },
                        new ProblemTypeTranslation { Language = "en", Name = "Cooling Units" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "تسريب مياه" },
                        new ProblemTypeTranslation { Language = "en", Name = "Water Leakage" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "مجاري وصرف صحي" },
                        new ProblemTypeTranslation { Language = "en", Name = "Sewer & Drainage" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "شاشات وأجهزة إدخال" },
                        new ProblemTypeTranslation { Language = "en", Name = "Screens & Input Devices" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "الطابعات" },
                        new ProblemTypeTranslation { Language = "en", Name = "Printers" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "تمديدات كهربائية" },
                        new ProblemTypeTranslation { Language = "en", Name = "Electrical Wiring" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "مفاتيح وقواطع" },
                        new ProblemTypeTranslation { Language = "en", Name = "Switches & Breakers" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "إنارة" },
                        new ProblemTypeTranslation { Language = "en", Name = "Lighting" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "لوحات كهرباء" },
                        new ProblemTypeTranslation { Language = "en", Name = "Electrical Panels" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "تمديدات الشبكات" },
                        new ProblemTypeTranslation { Language = "en", Name = "Network Cabling" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "أجهزة الراوتر" },
                        new ProblemTypeTranslation { Language = "en", Name = "Routers" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "ضعف الشبكة" },
                        new ProblemTypeTranslation { Language = "en", Name = "Weak Network Signal" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "انقطاع الإنترنت" },
                        new ProblemTypeTranslation { Language = "en", Name = "Internet Outage" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "الأنظمة والبرامج" },
                        new ProblemTypeTranslation { Language = "en", Name = "Systems & Software" }
                    }},
                    new ProblemType { Translations = new List<ProblemTypeTranslation>
                    {
                        new ProblemTypeTranslation { Language = "ar", Name = "أجهزة الكمبيوتر" },
                        new ProblemTypeTranslation { Language = "en", Name = "Computers" }
                    }}
                };

                await _context.PTypes.AddRangeAsync(ptypes);
                await _context.SaveChangesAsync();
            }

            // -----------------------------------------------------
            // 3) توزيع الكاتيجوري على الفنيين
            // -----------------------------------------------------
            var categoriesList = await _context.Tcategories
                .OrderBy(c => c.Id)
                .ToListAsync();

            var technicians = (await _userManager.GetUsersInRoleAsync("Technician"))
                .OrderBy(u => u.Id)
                .ToList();

            if (categoriesList.Any() && technicians.Any())
            {
                for (int i = 0; i < technicians.Count; i++)
                {
                    var cat = categoriesList[i % categoriesList.Count];
                    technicians[i].TechnicianCategoryId = cat.Id;
                    await _userManager.UpdateAsync(technicians[i]);
                }
            }

            // -----------------------------------------------------
            // 4) طلبات الصيانة (100 طلب) – بدون صور – حالات مخلوطة
            // -----------------------------------------------------
            if (await _context.MaintenanceRequests.AnyAsync())
                return;

            var allProblemTypes = await _context.PTypes
                .Include(p => p.Translations)
                .ToListAsync();

            var employeesList = (await _userManager.GetUsersInRoleAsync("Employee"))
                .OrderBy(u => u.Id)
                .ToList();

            var techsForRequests = (await _userManager.GetUsersInRoleAsync("Technician"))
                .OrderBy(u => u.Id)
                .ToList();

            var adminUser = await _userManager.FindByNameAsync("yousef.daraghmeh");
            var managerUser = await _userManager.FindByNameAsync("hassan.daraghmeh");

            if (!allProblemTypes.Any() || !employeesList.Any() || !techsForRequests.Any())
                return;

            // قاموس من الاسم العربي -> ProblemTypeId
            var ptByAr = new Dictionary<string, int>();
            foreach (var p in allProblemTypes)
            {
                var ar = p.Translations.FirstOrDefault(t => t.Language == "ar")?.Name;
                if (!string.IsNullOrWhiteSpace(ar) && !ptByAr.ContainsKey(ar))
                    ptByAr.Add(ar, p.Id);
            }

            var addressesForRequests = new[]
            {
                "المقر الرئيسي - الطابق الأول - مكتب الموارد البشرية",
                "المقر الرئيسي - الطابق الثاني - قسم المالية",
                "المقر الرئيسي - الطابق الثالث - قسم تكنولوجيا المعلومات",
                "فرع A - الطابق الأرضي - الاستقبال",
                "فرع A - مكتب 10 - فريق المبيعات",
                "فرع B - الطابق الثالث - غرفة الاجتماعات",
                "فرع B - الطابق الثاني - قسم الصيانة",
                "مستودع المواد - المنطقة الصناعية",
                "ساحة المواقف - أمام المبنى الرئيسي",
                "مختبر الحاسوب - مبنى التدريب"
            };

            var problemNamesForRequests = new[]
            {
                "تمديدات كهربائية",
                "انقطاع الإنترنت",
                "الطابعات",
                "تسريب مياه",
                "المكيفات",
                "الجدران والدهان",
                "مكاتب وكراسي",
                "لوحات كهرباء",
                "ضعف الشبكة",
                "الأنظمة والبرامج",
                "مجاري وصرف صحي",
                "وحدات التبريد",
                "البلاط والأرضيات",
                "نظافة عامة",
                "إضاءة الطوارئ",
                "شاشات وأجهزة إدخال",
                "تمديدات الشبكات",
                "مفاتيح وقواطع",
                "أجهزة الراوتر",
                "أجهزة الكمبيوتر",
                "نظام إنذار الحريق",
                "ترتيب القاعات"
            };

            var now = DateTime.UtcNow;
            var random = new Random(777);

            var requests = new List<MaintenanceRequest>();
            var workEntries = new List<WorkTimeEntry>();

            // توزيع الحالات بنِسَب معيّنة، ثم خلطها
            var caseTypesPool = new List<CaseType>();
            caseTypesPool.AddRange(Enumerable.Repeat(CaseType.Submitted, 20));
            caseTypesPool.AddRange(Enumerable.Repeat(CaseType.ManagerReview, 15));
            caseTypesPool.AddRange(Enumerable.Repeat(CaseType.Processing, 25));
            caseTypesPool.AddRange(Enumerable.Repeat(CaseType.ResourcesNeeded, 10));
            caseTypesPool.AddRange(Enumerable.Repeat(CaseType.Modified, 5));
            caseTypesPool.AddRange(Enumerable.Repeat(CaseType.Reopened, 10));
            caseTypesPool.AddRange(Enumerable.Repeat(CaseType.Processed, 7));
            caseTypesPool.AddRange(Enumerable.Repeat(CaseType.Completed, 6));
            caseTypesPool.AddRange(Enumerable.Repeat(CaseType.Cancelled, 2));

            if (caseTypesPool.Count != 100)
                throw new Exception($"Expected 100 case types, found {caseTypesPool.Count}");

            // Shuffle ثابت
            for (int i = caseTypesPool.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                (caseTypesPool[i], caseTypesPool[j]) = (caseTypesPool[j], caseTypesPool[i]);
            }

            for (int i = 0; i < 100; i++)
            {
                var caseType = caseTypesPool[i];

                var owner = employeesList[i % employeesList.Count];
                var problemName = problemNamesForRequests[i % problemNamesForRequests.Length];

                if (!ptByAr.TryGetValue(problemName, out var problemTypeId))
                {
                    problemTypeId = allProblemTypes.First().Id;
                }

                // من أنشأ الطلب؟
                string createdByUserId;
                if (i % 20 == 0 && managerUser != null)
                    createdByUserId = managerUser.Id;
                else if (i % 33 == 0 && adminUser != null)
                    createdByUserId = adminUser.Id;
                else if (i % 9 == 0)
                    createdByUserId = techsForRequests[i % techsForRequests.Count].Id;
                else
                    createdByUserId = owner.Id;

                var address = addressesForRequests[i % addressesForRequests.Length];

                // 👈 شيلنا رقم الطلب من التايتل والـ Description
                var title = problemName;
                var description =
                    $"طلب صيانة بخصوص {problemName} في {address}. " +
                    $"الحالة الحالية للطلب: {caseType}. تم توليد هذا الطلب كبيانات واقعية " +
                    $"لاختبار تدفق العمل، الإشعارات، التقارير ولوحة المتابعة.";

                // تاريخ الإنشاء حسب الحالة
                DateTime createdAt;
                if (caseType == CaseType.Submitted)
                {
                    createdAt = now
                        .AddDays(-random.Next(1, 7))
                        .AddHours(-random.Next(0, 6));
                }
                else if (caseType == CaseType.ManagerReview)
                {
                    createdAt = now
                        .AddDays(-random.Next(5, 20))
                        .AddHours(-random.Next(0, 8));
                }
                else
                {
                    createdAt = now
                        .AddDays(-random.Next(10, 60))
                        .AddHours(-random.Next(0, 10));
                }

                var request = new MaintenanceRequest
                {
                    Title = title,
                    Description = description,
                    Address = address,
                    Priority = (Priority)(i % 3), // Low/Medium/High
                    ProblemTypeId = problemTypeId,
                    CaseType = caseType,
                    OwnerUserId = owner.Id,
                    CreatedByUserId = createdByUserId,
                    CreatedAt = createdAt
                };

                // Notes (مع ضمان وجود ملاحظة واحدة على الأقل)
                if (caseType == CaseType.Reopened)
                {
                    request.Notes.Add(new MaintenanceNote
                    {
                        Text = "تم إعادة فتح الطلب لأن المشكلة ما زالت مستمرة من وجهة نظر صاحب الطلب.",
                        Type = NoteType.ReopenReason,
                        Author = NoteAuthor.Owner,
                        CreatedByUserId = owner.Id
                    });
                }
                else if (caseType == CaseType.ResourcesNeeded)
                {
                    request.Notes.Add(new MaintenanceNote
                    {
                        Text = "يحتاج الفني إلى مواد/أدوات إضافية أو فني آخر لاستكمال العمل.",
                        Type = NoteType.HelpRequest,
                        Author = NoteAuthor.Technician,
                        CreatedByUserId = createdByUserId
                    });
                }
                else if (i % 7 == 0)
                {
                    request.Notes.Add(new MaintenanceNote
                    {
                        Text = "ملاحظة متابعة عامة حول حالة الموقع وطبيعة العطل.",
                        Type = NoteType.General,
                        Author = NoteAuthor.Owner,
                        CreatedByUserId = owner.Id
                    });
                }

                // 🔹 ضمان وجود ملاحظة واحدة على الأقل لكل طلب
                if (request.Notes.Count == 0)
                {
                    request.Notes.Add(new MaintenanceNote
                    {
                        Text = "تمت إضافة هذه الملاحظة ضمن بيانات الاختبار لتوضيح استخدام نظام الملاحظات.",
                        Type = NoteType.General,
                        Author = NoteAuthor.Owner,
                        CreatedByUserId = owner.Id
                    });
                }

                // تعيين فنيين
                bool shouldAssignTech =
                    caseType != CaseType.Submitted &&
                    (caseType != CaseType.ManagerReview || i % 2 == 0);

                if (caseType == CaseType.Cancelled && i % 2 == 0)
                    shouldAssignTech = true;

                DateTime? lastWorkTime = null;

                if (shouldAssignTech)
                {
                    var tech = techsForRequests[i % techsForRequests.Count];
                    var assignedAt = createdAt.AddHours(random.Next(1, 6));

                    var link = new MaintenanceRequestTechnician
                    {
                        TechnicianUserId = tech.Id,
                        AssignedAtUtc = assignedAt,
                        ExpectedDuration = 2 + (i % 6)
                    };

                    if ((caseType == CaseType.Modified || caseType == CaseType.Cancelled) && i % 3 == 0)
                    {
                        link.UnassignedAtUtc = assignedAt.AddHours(2);
                    }

                    request.Technicians.Add(link);

                    // فني إضافي أحياناً في ResourcesNeeded
                    if (caseType == CaseType.ResourcesNeeded && i % 2 == 0)
                    {
                        var tech2 = techsForRequests[(i + 5) % techsForRequests.Count];
                        if (tech2.Id != tech.Id)
                        {
                            request.Technicians.Add(new MaintenanceRequestTechnician
                            {
                                TechnicianUserId = tech2.Id,
                                AssignedAtUtc = assignedAt.AddMinutes(30),
                                ExpectedDuration = 1 + (i % 4)
                            });
                        }
                    }

                    // WorkTimeEntries للحالات اللي فيها شغل فعلي
                    if (caseType == CaseType.Processing ||
                        caseType == CaseType.Reopened ||
                        caseType == CaseType.Processed ||
                        caseType == CaseType.Completed)
                    {
                        var start = new DateTimeOffset(assignedAt);
                        DateTimeOffset? stop = null;

                        if (caseType == CaseType.Processed || caseType == CaseType.Completed)
                        {
                            stop = start.AddHours(2 + (i % 4));
                        }
                        else if (i % 4 == 0)
                        {
                            stop = start.AddHours(1 + (i % 3));
                        }

                        workEntries.Add(new WorkTimeEntry
                        {
                            Request = request,
                            TechnicianUserId = tech.Id,
                            StartedAt = start,
                            StoppedAt = stop
                        });

                        lastWorkTime = stop?.UtcDateTime ?? start.UtcDateTime;
                    }
                }

                // ClosedAtUtc للحالات المغلقة
                bool isClosed =
                    caseType == CaseType.Processed ||
                    caseType == CaseType.Completed ||
                    caseType == CaseType.Cancelled;

                if (isClosed)
                {
                    var closedAt = createdAt
                        .AddDays(random.Next(0, 5))
                        .AddHours(random.Next(1, 6));

                    if (closedAt > now)
                        closedAt = now.AddMinutes(-random.Next(5, 60));

                    request.ClosedAtUtc = closedAt;
                }

                // UpdatedAt (آخر تعديل)
                DateTime? updatedAt = null;

                if (request.ClosedAtUtc.HasValue)
                {
                    // الطلبات النهائية: آخر تعديل = وقت الإغلاق
                    updatedAt = request.ClosedAtUtc.Value;
                }
                else
                {
                    bool hasTech = request.Technicians.Count > 0;
                    bool hasNotes = request.Notes.Count > 0;

                    if (lastWorkTime.HasValue)
                    {
                        updatedAt = lastWorkTime.Value;
                    }
                    else if (caseType != CaseType.Submitted || hasTech || hasNotes)
                    {
                        var candidate = createdAt.AddHours(random.Next(1, 72));
                        if (candidate > now) candidate = now.AddMinutes(-random.Next(10, 120));
                        updatedAt = candidate;
                    }
                    else if (i % 4 == 0)
                    {
                        var candidate = createdAt.AddHours(random.Next(1, 24));
                        if (candidate > now) candidate = createdAt;
                        updatedAt = candidate;
                    }
                }

                request.UpdatedAt = updatedAt;

                requests.Add(request);
            }

            await _context.MaintenanceRequests.AddRangeAsync(requests);

            if (workEntries.Count > 0)
                await _context.WorkTimeEntries.AddRangeAsync(workEntries);

            await _context.SaveChangesAsync();

            // ============================
            // 5) Notifications (Owner + Techs + Managers + Admins)
            // ============================

            var managersList = (await _userManager.GetUsersInRoleAsync("MaintenanceManager")).ToList();
            var adminsList = (await _userManager.GetUsersInRoleAsync("Admin")).ToList();

            var managerIds = managersList
                .Select(m => m.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();

            var adminIds = adminsList
                .Select(a => a.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();

            var notifications = new List<Notification>();

            foreach (var r in requests)
            {
                // 1) إشعار لصاحب الطلب "طلب تم إنشاؤه لك"
                if (!string.IsNullOrWhiteSpace(r.OwnerUserId))
                {
                    notifications.Add(new Notification
                    {
                        UserId = r.OwnerUserId,
                        MaintenanceRequestId = r.Id,
                        Type = NotificationType.RequestStatusChanged,
                        Severity = NotificationSeverity.Info,

                        TitleKey = "NOTIF_REQUEST_CREATED_FOR_YOU_TITLE",
                        BodyKey = "NOTIF_REQUEST_CREATED_FOR_YOU_BODY",
                        TitleArgsJson = null,
                        BodyArgsJson = JsonSerializer.Serialize(new object[] { r.Id }),

                        Channels = NotificationChannel.InApp | NotificationChannel.Email,
                        CreatedAt = r.CreatedAt,
                        CreatedAtUtc = r.CreatedAt,
                        IsRead = false,
                        EmailSent = false
                    });
                }

                // 2) إشعار عام للمدير + الأدمن عن طلب جديد
                if (managerIds.Count > 0 || adminIds.Count > 0)
                {
                    foreach (var mid in managerIds)
                    {
                        notifications.Add(new Notification
                        {
                            UserId = mid,
                            MaintenanceRequestId = r.Id,
                            Type = NotificationType.RequestStatusChanged,
                            Severity = NotificationSeverity.Info,

                            TitleKey = "NOTIF_REQUEST_CREATED_TITLE",
                            BodyKey = "NOTIF_REQUEST_CREATED_BODY",
                            TitleArgsJson = null,
                            BodyArgsJson = JsonSerializer.Serialize(new object[] { r.Id }),

                            Channels = NotificationChannel.InApp | NotificationChannel.Email,
                            CreatedAt = r.CreatedAt,
                            CreatedAtUtc = r.CreatedAt,
                            IsRead = false,
                            EmailSent = false
                        });
                    }

                    foreach (var aid in adminIds)
                    {
                        notifications.Add(new Notification
                        {
                            UserId = aid,
                            MaintenanceRequestId = r.Id,
                            Type = NotificationType.RequestStatusChanged,
                            Severity = NotificationSeverity.Info,

                            TitleKey = "NOTIF_REQUEST_CREATED_TITLE",
                            BodyKey = "NOTIF_REQUEST_CREATED_BODY",
                            TitleArgsJson = null,
                            BodyArgsJson = JsonSerializer.Serialize(new object[] { r.Id }),

                            Channels = NotificationChannel.InApp | NotificationChannel.Email,
                            CreatedAt = r.CreatedAt,
                            CreatedAtUtc = r.CreatedAt,
                            IsRead = false,
                            EmailSent = false
                        });
                    }
                }

                // 3) إشعار إسناد للفنيين
                if (r.Technicians is { Count: > 0 })
                {
                    var techLinks = r.Technicians
                        .Where(t => !string.IsNullOrWhiteSpace(t.TechnicianUserId))
                        .Select(t => new { t.TechnicianUserId, t.AssignedAtUtc })
                        .ToList();

                    foreach (var t in techLinks)
                    {
                        var assignedAt = t.AssignedAtUtc == default
                            ? r.CreatedAt.AddHours(1)
                            : t.AssignedAtUtc;

                        notifications.Add(new Notification
                        {
                            UserId = t.TechnicianUserId!,
                            MaintenanceRequestId = r.Id,
                            Type = NotificationType.RequestAssigned,
                            Severity = NotificationSeverity.Info,

                            TitleKey = "NOTIF_ASSIGNED_TITLE",
                            BodyKey = "NOTIF_ASSIGNED_BODY",
                            TitleArgsJson = null,
                            BodyArgsJson = JsonSerializer.Serialize(new object[] { r.Id }),

                            Channels = NotificationChannel.InApp | NotificationChannel.Email,
                            CreatedAt = assignedAt,
                            CreatedAtUtc = assignedAt,
                            IsRead = false,
                            EmailSent = false
                        });
                    }
                }

                // 4) إشعارات تغيير الحالة للطلبات المغلقة
                if (r.CaseType == CaseType.Completed ||
                    r.CaseType == CaseType.Processed ||
                    r.CaseType == CaseType.Cancelled)
                {
                    var recipients = new HashSet<string>(StringComparer.Ordinal);

                    if (!string.IsNullOrWhiteSpace(r.OwnerUserId))
                        recipients.Add(r.OwnerUserId);

                    var activeTechIds = r.Technicians
                        .Where(x => !string.IsNullOrWhiteSpace(x.TechnicianUserId)
                                    && x.UnassignedAtUtc == null)
                        .Select(x => x.TechnicianUserId!)
                        .ToList();

                    foreach (var tid in activeTechIds)
                        recipients.Add(tid);

                    foreach (var mid in managerIds)
                        recipients.Add(mid);

                    foreach (var aid in adminIds)
                        recipients.Add(aid);

                    if (recipients.Count > 0)
                    {
                        string titleKey;
                        string bodyKey;
                        object[] bodyArgs;
                        var type = r.CaseType == CaseType.Completed
                            ? NotificationType.RequestCompleted
                            : NotificationType.RequestStatusChanged;

                        var severity = r.CaseType == CaseType.Completed
                            ? NotificationSeverity.Success
                            : NotificationSeverity.Info;

                        if (r.CaseType == CaseType.Completed)
                        {
                            titleKey = "NOTIF_REQUEST_COMPLETED_TITLE";
                            bodyKey = "NOTIF_REQUEST_COMPLETED_BODY";
                            bodyArgs = new object[] { r.Id };
                        }
                        else
                        {
                            titleKey = "NOTIF_REQUEST_STATUS_CHANGED_TITLE";
                            bodyKey = "NOTIF_REQUEST_STATUS_CHANGED_BODY";
                            bodyArgs = new object[] { r.Id, r.CaseType.ToString() };
                        }

                        var when = r.ClosedAtUtc ?? r.UpdatedAt ?? r.CreatedAt.AddHours(2);

                        foreach (var uid in recipients)
                        {
                            notifications.Add(new Notification
                            {
                                UserId = uid,
                                MaintenanceRequestId = r.Id,
                                Type = type,
                                Severity = severity,

                                TitleKey = titleKey,
                                BodyKey = bodyKey,
                                TitleArgsJson = null,
                                BodyArgsJson = JsonSerializer.Serialize(bodyArgs),

                                Channels = NotificationChannel.InApp | NotificationChannel.Email,
                                CreatedAt = when,
                                CreatedAtUtc = when,
                                IsRead = false,
                                EmailSent = false
                            });
                        }
                    }
                }

                // 5) إشعارات "إضافة ملاحظة"
                var firstNote = r.Notes.FirstOrDefault();
                if (firstNote != null)
                {
                    var recipients = new HashSet<string>(StringComparer.Ordinal);
                    var authorUserId = firstNote.CreatedByUserId;

                    var trimmed = (firstNote.Text ?? string.Empty).Trim();
                    if (trimmed.Length > 120)
                        trimmed = trimmed.Substring(0, 120) + "...";

                    if (!string.IsNullOrWhiteSpace(r.OwnerUserId) &&
                        !string.Equals(r.OwnerUserId, authorUserId, StringComparison.Ordinal))
                    {
                        recipients.Add(r.OwnerUserId);
                    }

                    var techIdsAll = r.Technicians
                        .Where(t => !string.IsNullOrWhiteSpace(t.TechnicianUserId))
                        .Select(t => t.TechnicianUserId!)
                        .Distinct()
                        .ToList();

                    foreach (var tid in techIdsAll)
                    {
                        if (!string.Equals(tid, authorUserId, StringComparison.Ordinal))
                            recipients.Add(tid);
                    }

                    foreach (var mid in managerIds)
                    {
                        if (!string.Equals(mid, authorUserId, StringComparison.Ordinal))
                            recipients.Add(mid);
                    }

                    foreach (var aid in adminIds)
                    {
                        if (!string.Equals(aid, authorUserId, StringComparison.Ordinal))
                            recipients.Add(aid);
                    }

                    if (recipients.Count > 0)
                    {
                        var noteTime = firstNote.CreatedAt;
                        if (noteTime == default)
                            noteTime = r.UpdatedAt ?? r.CreatedAt.AddHours(2);

                        foreach (var uid in recipients)
                        {
                            notifications.Add(new Notification
                            {
                                UserId = uid,
                                MaintenanceRequestId = r.Id,
                                Type = NotificationType.RequestStatusChanged,
                                Severity = NotificationSeverity.Info,

                                TitleKey = "NOTIF_NOTE_ADDED_TITLE",
                                BodyKey = "NOTIF_NOTE_ADDED_BODY",
                                TitleArgsJson = null,
                                BodyArgsJson = JsonSerializer.Serialize(new object[] { r.Id, trimmed }),

                                Channels = NotificationChannel.InApp | NotificationChannel.Email,
                                CreatedAt = noteTime,
                                CreatedAtUtc = noteTime,
                                IsRead = false,
                                EmailSent = false
                            });
                        }
                    }
                }
            }

            if (notifications.Count > 0)
            {
                await _context.Notifications.AddRangeAsync(notifications);
                await _context.SaveChangesAsync();
            }
        }
    }
}
