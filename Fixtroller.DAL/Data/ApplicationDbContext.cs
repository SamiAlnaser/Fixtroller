using Fixtroller.DAL.Entities;
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
                e.HasOne(m => m.CreatedByUser)
                 .WithMany(u => u.SubmittedRequests)
                 .HasForeignKey(m => m.CreatedByUserId)
                 .OnDelete(DeleteBehavior.Restrict); //  NoAction

                e.HasIndex(m => m.CreatedByUserId);
                e.Property(m => m.Title).IsRequired().HasMaxLength(200);
                e.Property(m => m.Address).IsRequired().HasMaxLength(300);
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
