using Fixtroller.DAL.Data.DTOs.NotificationDTOs;
using Fixtroller.DAL.Entities;
using Fixtroller.DAL.Repositories.NotificationRepository;
using Fixtroller.DAL.Repositories.UserRepository;
using Fixtroller.DAL.UnitOfWork;

namespace Fixtroller.BLL.Services.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly IAppEmailSender _emailSender;
        private readonly IPushNotificationSender _pushSender;

        public NotificationService(
            INotificationRepository notificationRepo,
            IUserRepository userRepo,
            IUnitOfWork uow,
            IAppEmailSender emailSender,
            IPushNotificationSender pushSender)
        {
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _uow = uow;
            _emailSender = emailSender;
            _pushSender = pushSender;
        }

        public async Task<int> CreateAsync(
            NotificationCreateModel model,
            CancellationToken ct = default)
        {
            var entity = new Notification
            {
                UserId = model.UserId,
                Title = model.Title,
                Body = model.Body,
                MaintenanceRequestId = model.MaintenanceRequestId,
                Type = model.Type,
                Severity = model.Severity,
                Channels = model.Channels,
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            };


            await _notificationRepo.AddAsync(entity, ct);
            await _uow.SaveAndCommitAsync(ct);



            if (model.Channels.HasFlag(NotificationChannel.Email))
            {
                var user = await _userRepo.GetByIdAsync(model.UserId);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    await _emailSender.SendAsync(
                        user.Email,
                        model.Title,
                        model.Body,
                        ct);

                    entity.EmailSent = true;
                    await _notificationRepo.UpdateAsync(entity, ct);
                    await _uow.SaveAndCommitAsync(ct);
                }
            }

            // 3) Push (للمستقبل)
            if (model.Channels.HasFlag(NotificationChannel.MobilePush))
            {
                await _pushSender.SendAsync(model.UserId, model.Title, model.Body, ct);
            }

            return entity.Id;
        }

        public async Task<IReadOnlyList<NotificationListItemDTO>> GetForUserAsync(
            string userId,
            bool onlyUnread,
            CancellationToken ct = default)
        {
            var list = await _notificationRepo.GetForUserAsync(userId, onlyUnread, ct);

            return list
                .Select(n => new NotificationListItemDTO
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    IsRead = n.IsRead,
                    CreatedAtUtc = n.CreatedAtUtc,
                    Type = n.Type,
                    Severity = n.Severity,
                    MaintenanceRequestId = n.MaintenanceRequestId
                })
                .ToList();
        }

        public async Task MarkAsReadAsync(int id, string userId, CancellationToken ct = default)
        {
            var notif = await _notificationRepo.GetForUserByIdAsync(id, userId, asTracking: true, ct);
            if (notif == null) return;

            notif.IsRead = true;
            await _notificationRepo.UpdateAsync(notif, ct);
            await _uow.SaveAndCommitAsync(ct);
        }

        public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
        {
            var unread = await _notificationRepo.GetUnreadForUserAsync(userId, ct);
            if (unread.Count == 0) return;

            foreach (var n in unread)
                n.IsRead = true;

            await _uow.SaveAndCommitAsync(ct);
        }
    }
}
