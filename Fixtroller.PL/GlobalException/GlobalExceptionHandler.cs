using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace Fixtroller.PL.GlobalException
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 1) نجيب TraceId يساعدنا في التتبع
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            // 2) نلوج بالتفاصيل الكاملة (stack + message + traceId)
            _logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}, Path: {Path}",
                traceId,
                httpContext.Request.Path);

            // 3) نجهّز الرد العام للمستخدم
            var error = new ErrorResponse
            {
                TraceId = traceId
            };

            switch (exception)
            {
                case BadHttpRequestException:
                    error.StatusCode = (int)HttpStatusCode.BadRequest;
                    error.Title = "Bad Request";
                    error.Message = "الطلب المرسَل غير صحيح. يرجى التحقق من البيانات والمحاولة مرة أخرى.";
                    break;

                case UnauthorizedAccessException:
                    error.StatusCode = StatusCodes.Status403Forbidden;
                    error.Title = "Forbidden";
                    error.Message = "غير مسموح لك بتنفيذ هذه العملية.";
                    break;

                // هنا ممكن تضيف AppException أو ValidationException لاحقاً لو عندك
                // case AppException ex:
                //     error.StatusCode = ex.StatusCode;
                //     error.Title = ex.Title;
                //     error.Message = ex.UserFriendlyMessage;
                //     break;

                default:
                    error.StatusCode = (int)HttpStatusCode.InternalServerError;
                    error.Title = "Internal Server Error";
                    error.Message = "حدث خطأ غير متوقع في النظام. إذا استمرت المشكلة، يرجى التواصل مع الدعم.";
                    break;
            }

            httpContext.Response.StatusCode = error.StatusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(error, cancellationToken);

            return true;
        }


        public sealed class ErrorResponse
        {
            public string Title { get; set; } = string.Empty;    // نوع الخطأ بشكل عام (مثلاً: Bad Request)
            public int StatusCode { get; set; }                  // 400 / 403 / 500 ...
            public string Message { get; set; } = string.Empty;  // رسالة ودّية للمستخدم
            public string? TraceId { get; set; }                 // اختياري: يساعدك تربط بين اللوج والرد
        }
    }
}
