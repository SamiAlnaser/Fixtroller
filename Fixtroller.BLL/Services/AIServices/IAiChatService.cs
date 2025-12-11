using Fixtroller.DAL.Data.DTOs.AIDTOs;
using System.Threading;
using System.Threading.Tasks;



namespace Fixtroller.BLL.Services.AiServices
{

    public interface IAiChatService
    {
        // 1) تشات عام (مدير، فني، موظف... إلخ)
        Task<string> SendAsync(
            string userId,
            string userRole,
            string message,
            CancellationToken ct = default);

        // 2) تشات الموظف (يرجع DTO فيها IsEnabled)
        Task<AiEmployeeChatResponseDTO> SendEmployeeAsync(
            string userId,
            string message,
            CancellationToken ct = default);

        Task<AiEmployeeChatSettingsDTO> GetEmployeeSettingsAsync(
            CancellationToken ct = default);

        Task<AiEmployeeChatSettingsDTO> UpdateEmployeeSettingsAsync(
            bool isEnabled,
            CancellationToken ct = default);
    }
}
