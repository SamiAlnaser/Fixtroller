using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Entities.AIChat;
using Fixtroller.DAL.Entities.Announcements;
using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using Fixtroller.DAL.Entities.TechnicianCategoryEntity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<TechnicianCategory> Tcategories { get; set; }
        public DbSet<ProblemType> PTypes { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<MaintenanceRequestImage> MaintenanceRequestImages { get; set; }
        public DbSet<MaintenanceNote> MaintenanceNotes { get; set; }
        public DbSet<WorkTimeEntry> WorkTimeEntries { get; set; }
        public DbSet<MaintenanceRequestTechnician> MaintenanceRequestTechnicians { get; set; }
        public DbSet<AiEmployeeChatSettingsEntity> AiEmployeeChatSettings { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementImage> AnnouncementImages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TechnicianCategory>(e =>
            {
                e.ToTable("TechnicianCategory");   
                e.HasKey(x => x.Id);
            });

            builder.Entity<ProblemType>(e =>
            {
                e.ToTable("ProblemType");   
                e.HasKey(x => x.Id);
            });

            builder.Entity<MaintenanceRequest>(e =>
            {
                // من أنشأ الطلب فعلاً
                e.HasOne(m => m.CreatedByUser)
                 .WithMany(u => u.SubmittedRequests)
                 .HasForeignKey(m => m.CreatedByUserId)
                 .OnDelete(DeleteBehavior.Restrict);

                // صاحب الطلب (الموظف)
                e.HasOne(m => m.OwnerUser)
                 .WithMany(u => u.OwnedRequests)
                 .HasForeignKey(m => m.OwnerUserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(m => m.CreatedByUserId);
                e.HasIndex(m => m.OwnerUserId);

                e.Property(m => m.Title).IsRequired().HasMaxLength(200);
                e.Property(m => m.Address).HasMaxLength(300);

                e.Property(m => m.Latitude).HasColumnType("decimal(9,6)");
                e.Property(m => m.Longitude).HasColumnType("decimal(9,6)");
            });




            builder.Entity<ApplicationUser>(u =>
            {
                u.HasOne(x => x.TechnicianCategory)
                 .WithMany(c => c.Technicians)
                 .HasForeignKey(x => x.TechnicianCategoryId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<MaintenanceNote>(e =>
            {
                e.ToTable("MaintenanceNotes");
                e.HasKey(x => x.Id);
                e.Property(x => x.Text).IsRequired().HasMaxLength(2000);
                e.Property(x => x.Type).IsRequired();
                e.Property(x => x.Author).IsRequired();

                e.HasOne(x => x.MaintenanceRequest)
                 .WithMany(r => r.Notes)
                 .HasForeignKey(x => x.MaintenanceRequestId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.CreatedByUser)
                 .WithMany()
                 .HasForeignKey(x => x.CreatedByUserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<MaintenanceRequestImage>(e =>
            {
                e.ToTable("MaintenanceRequestImages");
                e.HasKey(x => x.Id);
                e.Property(x => x.FileName).IsRequired().HasMaxLength(300);
                e.HasOne(x => x.MaintenanceRequest)
                 .WithMany(r => r.Images)
                 .HasForeignKey(x => x.MaintenanceRequestId)
                 .OnDelete(DeleteBehavior.Cascade);
            });


            builder.Entity<WorkTimeEntry>(e =>
            {
                e.ToTable("WorkTimeEntry");
                e.HasKey(x => x.Id);

                e.HasOne(x => x.Request)
                 .WithMany()
                 .HasForeignKey(x => x.RequestId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.TechnicianUserId).IsRequired();
                e.Property(x => x.StartedAt).IsRequired();

                // عدّاد واحد نشط لكل (Request, Technician)
                e.HasIndex(x => new { x.RequestId, x.TechnicianUserId })
                 .IsUnique()
                 .HasFilter("[StoppedAt] IS NULL");
            });

            builder.Entity<MaintenanceRequestTechnician>(e =>
            {
                e.ToTable("MaintenanceRequestTechnician");
                e.HasKey(x => x.Id);

                e.HasOne(x => x.Request)
                 .WithMany(r => r.Technicians)
                 .HasForeignKey(x => x.RequestId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.TechnicianUserId).IsRequired();
                e.Property(x => x.AssignedAtUtc).IsRequired();

                // منع تكرار تعيين نشط لنفس (Request, Technician)
                e.HasIndex(x => new { x.RequestId, x.TechnicianUserId })
                 .IsUnique()
                 .HasFilter("[UnassignedAtUtc] IS NULL");

    

            });

            builder.Entity<Announcement>(b =>
            {
                b.HasOne(a => a.CreatedByUser)
                 .WithMany()
                 .HasForeignKey(a => a.CreatedByUserId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.Property(a => a.Title)
                 .IsRequired()
                 .HasMaxLength(200);

                b.Property(a => a.Content)
                 .IsRequired();
            });

            builder.Entity<AnnouncementImage>(b =>
            {
                b.HasOne(i => i.Announcement)
                 .WithMany(a => a.Images)
                 .HasForeignKey(i => i.AnnouncementId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.Property(i => i.FileName)
                 .IsRequired()
                 .HasMaxLength(255);
            });



            // تغيير أسماء الجداول الافتراضية
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UsersRoles");

            // تجاهل بعض الجداول الافتراضية
            builder.Ignore<IdentityUserClaim<string>>();
            builder.Ignore<IdentityUserLogin<string>>();
            builder.Ignore<IdentityUserToken<string>>();
            builder.Ignore<IdentityRoleClaim<string>>();

            builder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();


        }



    }
}
