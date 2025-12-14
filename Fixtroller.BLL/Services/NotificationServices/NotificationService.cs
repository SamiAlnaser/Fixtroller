using Fixtroller.DAL.Data.DTOs.NotificationDTOs;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.NotificationRepositories;
using Fixtroller.DAL.Repositories.UserRepository;
using Fixtroller.DAL.UnitOfWork;
using System.Text.Json;
using Fixtroller.BLL.Mapping;

namespace Fixtroller.BLL.Services.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly IAppEmailSender _emailSender;
        private readonly IPushNotificationSender _pushSender;
        private readonly INotificationMessageBuilder _msgBuilder;

        public NotificationService(
            INotificationRepository notificationRepo,
            IUserRepository userRepo,
            IUnitOfWork uow,
            IAppEmailSender emailSender,
            IPushNotificationSender pushSender,
            INotificationMessageBuilder msgBuilder)
        {
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _uow = uow;
            _emailSender = emailSender;
            _pushSender = pushSender;
            _msgBuilder = msgBuilder;
        }

        public async Task<int> CreateAsync(NotificationCreateModel model, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            var entity = new Notification
            {
                UserId = model.UserId,

                TitleKey = model.TitleKey,
                BodyKey = model.BodyKey,
                TitleArgsJson = model.TitleArgs == null ? null : JsonSerializer.Serialize(model.TitleArgs),
                BodyArgsJson = model.BodyArgs == null ? null : JsonSerializer.Serialize(model.BodyArgs),

                MaintenanceRequestId = model.MaintenanceRequestId,
                Type = model.Type,
                Severity = model.Severity,
                Channels = model.Channels,

                IsRead = false,

                // ✅ ثبّت الوقت
                CreatedAtUtc = now,
                CreatedAt = now
            };

            await _notificationRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            // ✅ ابنِ نص مترجم للإيميل/البوش
            var (title, body) = _msgBuilder.Build(
                model.TitleKey, model.TitleArgs,
                model.BodyKey, model.BodyArgs,
                model.Language);

            if (model.Channels.HasFlag(NotificationChannel.Email))
            {
                var user = await _userRepo.GetByIdAsync(model.UserId, ct);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    var sent = await _emailSender.SendAsync(user.Email, title, body, ct);

                    if (sent)
                    {
                        entity.EmailSent = true;
                        await _notificationRepo.UpdateAsync(entity, ct);
                        await _uow.SaveChangesAsync(ct);
                    }
                }
            }

            if (model.Channels.HasFlag(NotificationChannel.MobilePush))
            {
                await _pushSender.SendAsync(model.UserId, title, body, ct);
            }

            return entity.Id;
        }

        public async Task<IReadOnlyList<NotificationListItemDTO>> GetForUserAsync(
          string userId, bool onlyUnread, string language = "ar", CancellationToken ct = default)
        {
            var list = await _notificationRepo.GetForUserAsync(userId, onlyUnread, ct);

            return list.Select(n =>
            {
                object[]? titleArgs = string.IsNullOrWhiteSpace(n.TitleArgsJson) ? null
                    : System.Text.Json.JsonSerializer.Deserialize<object[]>(n.TitleArgsJson);

                object[]? bodyArgs = string.IsNullOrWhiteSpace(n.BodyArgsJson) ? null
                    : System.Text.Json.JsonSerializer.Deserialize<object[]>(n.BodyArgsJson);

                var (title, body) = _msgBuilder.Build(
                    n.TitleKey ?? string.Empty, titleArgs,
                    n.BodyKey ?? string.Empty, bodyArgs,
                    language);

                return new NotificationListItemDTO
                {
                    Id = n.Id,
                    Title = title,
                    Body = body,

                    TitleKey = n.TitleKey,
                    BodyKey = n.BodyKey,
                    TitleArgsJson = n.TitleArgsJson,
                    BodyArgsJson = n.BodyArgsJson,

                    IsRead = n.IsRead,
                    CreatedAtUtc = n.CreatedAtUtc,
                    Type = n.Type,
                    Severity = n.Severity,
                    MaintenanceRequestId = n.MaintenanceRequestId
                };
            }).ToList();
        }

        public async Task MarkAsReadAsync(int id, string userId, CancellationToken ct = default)
        {
            var notif = await _notificationRepo.GetForUserByIdAsync(id, userId, asTracking: true, ct);
            if (notif == null) return;

            notif.IsRead = true;
            await _notificationRepo.UpdateAsync(notif, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
        {
            var unread = await _notificationRepo.GetUnreadForUserAsync(userId, ct);
            if (unread.Count == 0) return;

            foreach (var n in unread)
                n.IsRead = true;

            await _uow.SaveChangesAsync(ct);
        }


        public async Task<NotificationLoadMoreResponseDTO<NotificationListItemDTO>> GetForUserPageAsync(
    string userId,
    bool onlyUnread,
    int take,
    int? lastId,
    string language = "ar",
    CancellationToken ct = default)
        {
            // حماية بسيطة للـ page size
            if (take <= 0) take = 5;
            if (take > 50) take = 50;

            var list = await _notificationRepo.GetForUserPageAsync(userId, onlyUnread, take, lastId, ct);

            // هل في كمان بعد هاي الدفعة؟
            var hasMore = list.Count > take;
            if (hasMore)
                list = list.Take(take).ToList();

            var items = list.Select(n =>
            {
                object[]? titleArgs = string.IsNullOrWhiteSpace(n.TitleArgsJson)
                    ? null
                    : JsonSerializer.Deserialize<object[]>(n.TitleArgsJson);

                object[]? bodyArgs = string.IsNullOrWhiteSpace(n.BodyArgsJson)
                    ? null
                    : JsonSerializer.Deserialize<object[]>(n.BodyArgsJson);

                var (title, body) = _msgBuilder.Build(
                    n.TitleKey ?? string.Empty, titleArgs,
                    n.BodyKey ?? string.Empty, bodyArgs,
                    language);

                return new NotificationListItemDTO
                {
                    Id = n.Id,
                    Title = title,
                    Body = body,

                    TitleKey = n.TitleKey,
                    BodyKey = n.BodyKey,
                    TitleArgsJson = n.TitleArgsJson,
                    BodyArgsJson = n.BodyArgsJson,

                    IsRead = n.IsRead,
                    CreatedAtUtc = n.CreatedAtUtc,
                    Type = n.Type,
                    Severity = n.Severity,
                    MaintenanceRequestId = n.MaintenanceRequestId
                };
            }).ToList();

            return new NotificationLoadMoreResponseDTO<NotificationListItemDTO>
            {
                Items = items,
                HasMore = hasMore,
                NextLastId = items.LastOrDefault()?.Id
            };
        }


        public Task<int> GetUnreadCountAsync(
    string userId,
    CancellationToken ct = default)
        {
            return _notificationRepo.GetUnreadCountForUserAsync(userId, ct);
        }

    }
}
