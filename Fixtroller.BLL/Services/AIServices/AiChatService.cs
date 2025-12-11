using Fixtroller.DAL.Data;
using Fixtroller.DAL.Data.DTOs.AIDTOs;
using Fixtroller.DAL.Entities.AICHAT;
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
            private readonly string _model;
            private readonly ApplicationDbContext _db;

            public AiChatService(
                IConfiguration configuration,
                ApplicationDbContext db)
            {
                _db = db;

                var apiKey = configuration["OpenAI:ApiKey"]
                    ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured");

                _model = configuration["OpenAI:Model"] ?? "gpt-4.1-mini";

                _chatClient = new ChatClient(
                    model: _model,
                    apiKey: apiKey
                );
            }

        // ============================
        // 1) تشات عام (مدير / فني / موظف)
        // ============================
        public async Task<string> SendAsync(
 string userId,
 string userRole,
 string message,
 CancellationToken ct = default)
        {
            var roleDescription = userRole switch
            {
                "MaintenanceManager" => "أنت مساعد ذكي لمدير الصيانة. ركّز على إدارة المهام، توزيع الفنيين، متابعة الحالة، والتوصية بالخطوات التالية.",
                "Technician" => "أنت مساعد ذكي لفني الصيانة. ركّز على خطوات الإصلاح العملية، الأدوات المطلوبة، وتحذيرات السلامة.",
                "Employee" => "أنت مساعد ذكي لموظف يواجه مشكلة صيانة ويريد فهمها أو وصفها أو متابعة طلبه.",
                "Admin" => "أنت مساعد ذكي لمشرف النظام. ركّز على نظرة شمولية، مؤشرات الأداء، وإدارة المستخدمين والأقسام.",
                _ => "أنت مساعد ذكي في نظام صيانة. أجب بإيجاز ووضوح."
            };

            var systemMessage =
                "تحدّث بالعربية البسيطة والواضحة." +
                "\nلا تذكر أنك نموذج ذكاء اصطناعي إلا إذا سُئلت مباشرة." +
                "\nأجِب بإجابات قصيرة ومباشرة قدر الإمكان." +
                $"\nوصف الدور: {roleDescription}";

            ChatMessage[] messages =
            {
        new SystemChatMessage(systemMessage),
        new UserChatMessage(message)
    };

            var result = await _chatClient.CompleteChatAsync(
                messages,
                new ChatCompletionOptions
                {
                    MaxOutputTokens = 512
                },
                ct);

            return result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
        }

        // ==================================
        // 2) تشات الموظف (يراعي الإعداد IsEnabled)
        // ==================================
        public async Task<AiEmployeeChatResponseDTO> SendEmployeeAsync(
                string userId,
                string message,
                CancellationToken ct = default)
            {
                // نقرأ إعداد تفعيل/تعطيل التشات
                var settings = await _db.AiEmployeeChatSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);

                var isEnabled = settings?.IsEnabled ?? false;
                if (!isEnabled)
                {
                    return new AiEmployeeChatResponseDTO
                    {
                        IsEnabled = false,
                        Reply = "ميزة المساعد الذكي للموظفين غير مفعّلة حالياً من قبل الإدارة."
                    };
                }

                // نستخدم نفس SendAsync لكن نمرر الدور Employee
                var reply = await SendAsync(
                    userId,
                    "Employee",
                    message,
                    ct);

                return new AiEmployeeChatResponseDTO
                {
                    IsEnabled = true,
                    Reply = reply
                };
            }

            // ========================
            // 3) إعدادات تشات الموظف
            // ========================
            public async Task<AiEmployeeChatSettingsDTO> GetEmployeeSettingsAsync(
                CancellationToken ct = default)
            {
                var s = await _db.AiEmployeeChatSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);

                return new AiEmployeeChatSettingsDTO
                {
                    IsEnabled = s?.IsEnabled ?? false
                };
            }

            public async Task<AiEmployeeChatSettingsDTO> UpdateEmployeeSettingsAsync(
                bool isEnabled,
                CancellationToken ct = default)
            {
                var s = await _db.AiEmployeeChatSettings.FirstOrDefaultAsync(ct);

                if (s is null)
                {
                    s = new AiEmployeeChatSettings
                    {
                        IsEnabled = isEnabled
                    };
                    _db.AiEmployeeChatSettings.Add(s);
                }
                else
                {
                    s.IsEnabled = isEnabled;
                }

                await _db.SaveChangesAsync(ct);

                return new AiEmployeeChatSettingsDTO
                {
                    IsEnabled = s.IsEnabled
                };
            }
        }
    }

