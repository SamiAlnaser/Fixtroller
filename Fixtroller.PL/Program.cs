using Fixtroller.BLL.Services.NotificationServices;
using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.AnnouncementRepositories;
using Fixtroller.DAL.Utils;
using Fixtroller.PL.GlobalException;
using Fixtroller.PL.Services.Notifications;
using Fixtroller.PL.Services.Notifications.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace Fixtroller.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");


            Log.Logger = new LoggerConfiguration()
                // أقل مستوى عالميًا
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()

                .WriteTo.Console(
                    restrictedToMinimumLevel:
                        env == "Development" ? LogEventLevel.Debug : LogEventLevel.Information,
                    outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} | UserId={UserId} Role={UserRole} | {Message:lj}{NewLine}{Exception}")

                .WriteTo.File(
                    path: "Logs/errors-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    restrictedToMinimumLevel: LogEventLevel.Error, 
                    outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")

                .CreateLogger();


            try
            {
                Log.Information("Starting up application");


                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

               

                builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

                builder.Services.Configure<FormOptions>(options =>
                {
                    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; 
                });

                var connectionStringName = builder.Environment.IsDevelopment()
                                    ? "DevConnection"
                                    : "DefaultConnection";
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString(connectionStringName)));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            builder.Services.AddConfig();
                builder.Services.Configure<NotificationEmailWorkerOptions>(
                    builder.Configuration.GetSection("NotificationEmailWorker"));

                builder.Services.AddHostedService<NotificationEmailBackgroundService>();

                const string defaultCulture = "ar";
            var supportedCultures = new[]
            {
                  new CultureInfo(defaultCulture),
                  new CultureInfo("en")
             };
            builder.Services.Configure<RequestLocalizationOptions>(options => {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

            });
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

                builder.Services.PostConfigure<EmailSettings>(s =>
                {
                    Console.WriteLine($"[EmailSettings] Host={s.SmtpHost} Port={s.SmtpPort} User={s.UserName} From={s.From} PassLen={(s.Password ?? "").Replace(" ", "").Length}");
                });
                QuestPDF.Settings.License = LicenseType.Community;



                var jwtSecret = builder.Configuration["jwtOptions:SecretKey"];

                if (string.IsNullOrWhiteSpace(jwtSecret))
                {
                    // لوج مفيد عشان لو نسيته في السيرفر
                    Log.Fatal("Configuration error: jwtOptions:SecretKey is missing or empty");
                    throw new InvalidOperationException("Missing configuration value: jwtOptions:SecretKey");
                }

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

                builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer(options =>
                 {
                  options.TokenValidationParameters = new TokenValidationParameters
                   {
                     ValidateIssuer = false,
                     ValidateAudience = false,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = signingKey,

                      RoleClaimType = ClaimTypes.Role,
                      NameClaimType = ClaimTypes.NameIdentifier
                  };
                 });



            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();


            var app = builder.Build();

            app.UseExceptionHandler();

                app.Use(async (context, next) =>
                {
                    var user = context.User;

                    var userId =
                        user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        user?.FindFirst("Id")?.Value ??
                        "Anonymous";

                    var email = user?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

                    var roles = user?.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToArray() ?? Array.Empty<string>();

                    var roleString = roles.Length == 0 ? "None" : string.Join(",", roles);

                    using (LogContext.PushProperty("UserId", userId))
                    using (LogContext.PushProperty("UserEmail", email))
                    using (LogContext.PushProperty("UserRole", roleString))
                    {
                        await next();
                    }
                });
                app.UseSerilogRequestLogging();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
                //try
                //{
                //    Log.Information("Starting database migration & seeding...");

                //    using var scope = app.Services.CreateScope();
                //    var services = scope.ServiceProvider;

                //    // ✅ 1) Migrate مرة واحدة
                //    var db = services.GetRequiredService<ApplicationDbContext>();
                //    await db.Database.MigrateAsync();

                //    // ✅ 2) Seed Identity (Roles + Users)
                //    var seed = services.GetRequiredService<ISeedData>();
                //    await seed.IdentityDataSeedingAsync();

                //    // ✅ 3) مهم جدًا: امسح أي Entities متتبعة من UserManager/RoleManager
                //    db.ChangeTracker.Clear();

                //    // ✅ 4) Seed باقي الداتا
                //    await seed.DataSeedingAsync();

                //    Log.Information("Database migration & seeding finished successfully.");
                //}
                //catch (Exception ex)
                //{
                //    Log.Fatal(ex, "Error occurred while seeding the database.");
                //    throw;
                //}

                Log.Information("Starting HTTP pipeline...");

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseCors();
            app.UseAuthorization();
            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
            app.UseStaticFiles();
            app.MapControllers();
            app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
