using Fixtroller.DAL.Data.DTOs.AIDTOs;
using System.Threading;
using System.Threading.Tasks;



namespace Fixtroller.BLL.Services.AiServices
{

    public interface IAiChatService
    {
        // 1) إرسال رسالة للـ AI (الموظف / الفني / المدير / ... إلخ)
        Task<AiEmployeeChatResponseDTO> SendAsync(
                    string userRole,
                    string message,
                    List<AiChatHistoryItemDTO>? history,
                    CancellationToken ct = default);

        // 2) جلب إعدادات تفعيل الـ AI (موظف + فني)
        Task<AiEmployeeChatSettingsDTO> GetSettingsAsync(
            CancellationToken ct = default);

        // 3) تحديث إعدادات التفعيل (موظف + فني)
        Task<AiEmployeeChatSettingsDTO> UpdateSettingsAsync(
            bool isEmployeeEnabled,
            bool isTechnicianEnabled,
            CancellationToken ct = default);
    }
}

