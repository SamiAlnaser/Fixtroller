using Fixtroller.BLL.Reports;
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
using Fixtroller.PL.Services.Notifications;
using Fixtroller.PL.Services.Notifications.Email;
using Fixtroller.PL.Services.Notifications.Push;
using Fixtroller.PL.Services.Reports;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Fixtroller.PL
{
    internal static class AppConfiguration
    {
        internal static void AddConfig(this IServiceCollection services)
        {
            services.AddScoped<ITechnicianRepository, TechnicianRepository>();
            services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
            services.AddScoped<ITCategoryRepository, TCategorRepository>();
            services.AddScoped<ITCategoryService, TCategoryService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IMaintenanceRequestService, MaintenanceRequestService>();
            services.AddScoped<IProblemTypesService, ProblemTypesService>();
            services.AddScoped<IProblemTypeRepository, ProblemTypeRepository>();
            services.AddScoped<ISeedData, SeedData>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITechnicianService, TechnicianService>();
            services.AddScoped<IMaintenanceNoteRepository, MaintenanceNoteRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IWorkTimeRepository, WorkTimeRepository>();
            services.AddScoped<IMaintenanceRequestTechnicianRepository, MaintenanceRequestTechnicianRepository>();
            services.AddScoped<IMetricsRepository, MetricsRepository>();
            services.AddScoped<IMetricsService, MetricsService>();
            services.AddScoped<IAiEmployeeChatSettingsRepository, AiEmployeeChatSettingsRepository>();
            services.AddScoped<IAiChatService, AiChatService>();
            services.AddScoped<IReportsTextBuilder, LocalizerReportsTextBuilder>();
            services.AddScoped<IMaintenanceReportsService, MaintenanceReportsService>();
            services.AddScoped<IUserservice, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            // Email + Notifications
            services.AddScoped<IAppEmailSender, SmtpEmailSender>();
            services.AddScoped<IPushNotificationSender, NoopPushNotificationSender>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<INotificationMessageBuilder, LocalizerNotificationMessageBuilder>();



        }
    }
}
