using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Utils;
using Fixtroller.PL.GlobalException;
using Fixtroller.PL.Services.Notifications.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Globalization;
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
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")

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

            var connectionStringName = builder.Environment.IsDevelopment()
                                    ? "DevConnection"
                                    : "DefaultConnection";
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString(connectionStringName)));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            builder.Services.AddConfig();


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
            QuestPDF.Settings.License = LicenseType.Community;


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
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwtOptions:SecretKey"]!))
                   };
                 });



            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();


            var app = builder.Build();

            app.UseExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
            //var scope = app.Services.CreateScope();
                try
                {
                    Log.Information("Starting database seeding...");

                    using (var scope = app.Services.CreateScope())
                    {
                        var seed = scope.ServiceProvider.GetRequiredService<ISeedData>();
                        await seed.IdentityDataSeedingAsync();
                        await seed.DataSeedingAsync();
                    }

                    Log.Information("Database seeding finished successfully.");
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Error occurred while seeding the database.");
                    throw; // خليه يفجّر عشان تعرف إن الخلل من seeding
                }

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
