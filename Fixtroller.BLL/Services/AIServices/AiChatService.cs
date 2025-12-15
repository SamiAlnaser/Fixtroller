using Fixtroller.DAL.Data;
using Fixtroller.DAL.Data.DTOs.AIDTOs.Requests;
using Fixtroller.DAL.Data.DTOs.AIDTOs.Responses;
using Fixtroller.DAL.Entities.AIChat;
using Fixtroller.DAL.Repositories.AIChatRepositories;
using Fixtroller.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Fixtroller.BLL.Services.AiServices
{


    public sealed class AiChatService : IAiChatService
    {
        private readonly ChatClient _chatClient;
        private readonly IAiEmployeeChatSettingsRepository _settingsRepo;
        private readonly IUnitOfWork _uow;

        public AiChatService(
            IConfiguration configuration,
            IAiEmployeeChatSettingsRepository settingsRepo,
            IUnitOfWork uow)
        {
            _settingsRepo = settingsRepo;
            _uow = uow;

            var apiKey = configuration["OpenAI:ApiKey"]
                ?? throw new System.InvalidOperationException("OpenAI:ApiKey is not configured");

            var model = configuration["OpenAI:Model"] ?? "gpt-4.1-mini";

            _chatClient = new ChatClient(model, apiKey);
        }

        // 1) ميثود موحّدة للإرسال لكل الأدوار
        public async Task<AiEmployeeChatResponseDTO> SendAsync(
            string userRole,
            string message,
            List<AiChatHistoryItemDTO>? history,
            CancellationToken ct = default)
        {
            // 1) نجيب الإعدادات (تفعيل الموظف/الفني) من الريبو بدل الـ DbContext
            var settings = await _settingsRepo.GetAsync(ct);

            bool isEnabled = true;
            string? disabledMessage = null;

            switch (userRole)
            {
                case "Employee":
                    isEnabled = settings?.IsEmployeeEnabled ?? false;
                    disabledMessage = "ميزة المساعد الذكي للموظفين غير مفعّلة حالياً من قبل الإدارة.";
                    break;

                case "Technician":
                    isEnabled = settings?.IsTechnicianEnabled ?? false;
                    disabledMessage = "ميزة المساعد الذكي للفنيين غير مفعّلة حالياً من قبل الإدارة.";
                    break;

                default:
                    isEnabled = true;
                    break;
            }

            if (!isEnabled)
            {
                return new AiEmployeeChatResponseDTO
                {
                    IsEnabled = false,
                    Reply = disabledMessage ?? "ميزة المساعد الذكي غير مفعّلة حالياً."
                };
            }

            // 2) نبني الـ System message حسب الـ role
            var roleDescription = userRole switch
            {
                "MaintenanceManager" =>
                    "أنت مساعد ذكي لمدير الصيانة. ركّز على إدارة المهام، توزيع الفنيين، متابعة الحالة، والتوصية بالخطوات التالية.",
                "Technician" =>
                    "أنت مساعد ذكي لفني الصيانة. ركّز على خطوات الإصلاح العملية، الأدوات المطلوبة، وتحذيرات السلامة.",
                "Employee" =>
                    "أنت مساعد ذكي لموظف يواجه مشكلة صيانة ويريد فهمها أو وصفها أو متابعة طلبه.",
                "Admin" =>
                    "أنت مساعد ذكي لمشرف النظام. ركّز على نظرة شمولية، مؤشرات الأداء، وإدارة المستخدمين والأقسام.",
                _ =>
                    "أنت مساعد ذكي في نظام صيانة. أجب بإيجاز ووضوح."
            };

            var systemMessage =
                "تحدّث بالعربية البسيطة والواضحة." +
                "\nلا تذكر أنك نموذج ذكاء اصطناعي إلا إذا سُئلت مباشرة." +
                "\nأجِب بإجابات قصيرة ومباشرة قدر الإمكان." +
                $"\nوصف الدور: {roleDescription}";

            var chatMessages = new List<ChatMessage>
        {
            new SystemChatMessage(systemMessage)
        };

            // 3) نضيف تاريخ المحادثة القادم من الفرونت
            if (history is not null)
            {
                foreach (var h in history)
                {
                    if (string.Equals(h.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                    {
                        chatMessages.Add(new AssistantChatMessage(h.Content));
                    }
                    else
                    {
                        // الافتراضي user
                        chatMessages.Add(new UserChatMessage(h.Content));
                    }
                }
            }

            // 4) نضيف رسالة المستخدم الحالية في الآخر
            chatMessages.Add(new UserChatMessage(message));

            // 5) نرسل للـ OpenAI
            var result = await _chatClient.CompleteChatAsync(
                chatMessages,
                new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 512
                },
                ct);

            var replyText = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            return new AiEmployeeChatResponseDTO
            {
                IsEnabled = true,
                Reply = replyText
            };
        }

        // 2) قراءة إعدادات الموظف + الفني
        public async Task<AiEmployeeChatSettingsDTO> GetSettingsAsync(
            CancellationToken ct = default)
        {
            var s = await _settingsRepo.GetAsync(ct);

            return new AiEmployeeChatSettingsDTO
            {
                IsEmployeeEnabled = s?.IsEmployeeEnabled ?? false,
                IsTechnicianEnabled = s?.IsTechnicianEnabled ?? false
            };
        }

        // 3) تحديث إعدادات الموظف + الفني
        public async Task<AiEmployeeChatSettingsDTO> UpdateSettingsAsync(
            bool isEmployeeEnabled,
            bool isTechnicianEnabled,
            CancellationToken ct = default)
        {
            var s = await _settingsRepo.GetAsync(ct);

            if (s is null)
            {
                s = new AiEmployeeChatSettingsEntity
                {
                    IsEmployeeEnabled = isEmployeeEnabled,
                    IsTechnicianEnabled = isTechnicianEnabled
                };

                await _settingsRepo.AddAsync(s, ct);
            }
            else
            {
                s.IsEmployeeEnabled = isEmployeeEnabled;
                s.IsTechnicianEnabled = isTechnicianEnabled;
            }

            // هون نستعمل الـ UoW بدال _db.SaveChangesAsync
            await _uow.SaveAndCommitAsync(ct);

            return new AiEmployeeChatSettingsDTO
            {
                IsEmployeeEnabled = s.IsEmployeeEnabled,
                IsTechnicianEnabled = s.IsTechnicianEnabled
            };
        }
    }
}

