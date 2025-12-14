using Fixtroller.BLL.Services.AiServices;
using Fixtroller.BLL.Services.AuthenticationServices;
using Fixtroller.BLL.Services.FileService;
using Fixtroller.BLL.Services.MaintenanceRequestServices;
using Fixtroller.BLL.Services.NotificationServices;
using Fixtroller.BLL.Services.NumbersServices;
using Fixtroller.BLL.Services.ProblemTypesServices;
using Fixtroller.BLL.Services.ReportsServices;
using Fixtroller.BLL.Services.TCategoryServices;
using Fixtroller.BLL.Services.TechnicianServices;
using Fixtroller.BLL.Services.UserServices;
using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.AIChatRepositories;
using Fixtroller.DAL.Repositories.MaintenanceRequestRepositories;
using Fixtroller.DAL.Repositories.NotificationRepositories;
using Fixtroller.DAL.Repositories.NumbersRepositories;
using Fixtroller.DAL.Repositories.ProblemTypeRepositories;
using Fixtroller.DAL.Repositories.TCategoryRepositories;
using Fixtroller.DAL.Repositories.UserRepository;
using Fixtroller.DAL.Repositories.UserRepository.TechnicianRepositorirs;
using Fixtroller.DAL.UnitOfWork;
using Fixtroller.DAL.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Fixtroller.PL.Services.Notifications;
using Scalar;
using Scalar.AspNetCore;
using System.Globalization;
using System.Text;
using Fixtroller.PL.Services.Notifications.Push;
using Fixtroller.PL.Services.Notifications.Email;

namespace Fixtroller.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            QuestPDF.Settings.EnableDebugging = true;
            QuestPDF.Settings.License = LicenseType.Community;

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




            // Add services to the container.
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            var connectionStringName = builder.Environment.IsDevelopment()
                ? "DevConnection"
                : "DefaultConnection";

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString(connectionStringName)));
            builder.Services.AddScoped<ITechnicianRepository, TechnicianRepository>();
            builder.Services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
            builder.Services.AddScoped<ITCategoryRepository, TCategorRepository>();
            builder.Services.AddScoped<ITCategoryService, TCategoryService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IMaintenanceRequestService, MaintenanceRequestService>();
            builder.Services.AddScoped<IProblemTypesService, ProblemTypesService>();
            builder.Services.AddScoped<IProblemTypeRepository, ProblemTypeRepository>();
            builder.Services.AddScoped<ISeedData, SeedData>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<ITechnicianService, TechnicianService>();
            builder.Services.AddScoped<IMaintenanceNoteRepository, MaintenanceNoteRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IWorkTimeRepository, WorkTimeRepository>();
            builder.Services.AddScoped<IMaintenanceRequestTechnicianRepository, MaintenanceRequestTechnicianRepository>();
            builder.Services.AddScoped<IMetricsRepository, MetricsRepository>();
            builder.Services.AddScoped<IMetricsService, MetricsService>();
            builder.Services.AddScoped<IAiEmployeeChatSettingsRepository, AiEmployeeChatSettingsRepository>();
            builder.Services.AddScoped<IAiChatService, AiChatService>();



            builder.Services.AddScoped<IMaintenanceReportsService, MaintenanceReportsService>();

            builder.Services.AddScoped<IUserservice, UserService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            // Email settings
            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("Email"));

            // Email + Notifications
            builder.Services.AddScoped<IAppEmailSender, SmtpEmailSender>();
            builder.Services.AddScoped<IPushNotificationSender, NoopPushNotificationSender>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<INotificationMessageBuilder,LocalizerNotificationMessageBuilder>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            var userPolicy = "UserPolicy";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: userPolicy, policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });



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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("jwtOptions")["SecretKey"]))
            };
        });
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }


            var scope = app.Services.CreateScope();
            var objectOfSeedData = scope.ServiceProvider.GetRequiredService<ISeedData>();
            await objectOfSeedData.IdentityDataSeedingAsync();
            await objectOfSeedData.DataSeedingAsync();

            app.UseDeveloperExceptionPage();


            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseCors(userPolicy);

            app.UseAuthorization();
            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
            app.UseStaticFiles();
            app.MapControllers();

            app.Run();

        }
    }
}
