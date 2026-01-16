using Fixtroller.BLL.Services.NotificationServices;
using Fixtroller.DAL.Data;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.UserRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Fixtroller.PL.Services.Notifications
{
    public sealed class NotificationEmailBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationEmailBackgroundService> _logger;
        private readonly NotificationEmailWorkerOptions _options;

        public NotificationEmailBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationEmailBackgroundService> logger,
            IOptions<NotificationEmailWorkerOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationEmailBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    var msgBuilder = scope.ServiceProvider.GetRequiredService<INotificationMessageBuilder>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IAppEmailSender>();

                    var pending = await db.Notifications
                        .Where(n =>
                            !n.EmailSent &&
                            (n.Channels & NotificationChannel.Email) == NotificationChannel.Email)
                        .OrderBy(n => n.CreatedAtUtc)
                        .Take(_options.BatchSize)
                        .ToListAsync(stoppingToken);

                    if (pending.Count > 0)
                    {
                        _logger.LogInformation(
                            "Found {Count} pending email notifications.",
                            pending.Count);
                    }

                    foreach (var n in pending)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        try
                        {
                            var user = await userRepo.GetByIdAsync(n.UserId, stoppingToken);
                            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                                continue;

                            object[]? titleArgs = string.IsNullOrWhiteSpace(n.TitleArgsJson)
                                ? null
                                : JsonSerializer.Deserialize<object[]>(n.TitleArgsJson);

                            object[]? bodyArgs = string.IsNullOrWhiteSpace(n.BodyArgsJson)
                                ? null
                                : JsonSerializer.Deserialize<object[]>(n.BodyArgsJson);

                            var language = string.IsNullOrWhiteSpace(n.Language)
                                ? "ar"
                                : n.Language;

                            var (title, body) = msgBuilder.Build(
                                n.TitleKey, titleArgs,
                                n.BodyKey, bodyArgs,
                                language);

                            var ok = await emailSender.SendAsync(
                                user.Email,
                                title,
                                body,
                                stoppingToken);

                            if (ok)
                            {
                                n.EmailSent = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Failed to send email notification. NotificationId={NotificationId}",
                                n.Id);

                            n.Channels &= ~NotificationChannel.Email;
                        }
                    }

                    if (pending.Count > 0)
                    {
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // التطبيق بطفي – تجاهل
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error in NotificationEmailBackgroundService loop.");


                }

                try
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(_options.PollIntervalSeconds),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
            }

            _logger.LogInformation("NotificationEmailBackgroundService stopped.");
        }
    }
}
