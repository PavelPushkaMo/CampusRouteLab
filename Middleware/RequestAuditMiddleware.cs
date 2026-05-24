using CampusRouteLab.Services;

namespace CampusRouteLab.Middleware;

public class RequestAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAppInfoService _appInfoService;

    public RequestAuditMiddleware(RequestDelegate next, IAppInfoService appInfoService)
    {
        _next = next;
        _appInfoService = appInfoService;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IRequestContextService requestContextService,
        ITransientMarkerService transientMarkerService)
    {
        // Фиксируем начало обработки
        var startTime = DateTime.Now;
        Console.WriteLine($"[Audit] Начало запроса: {context.Request.Method} {context.Request.Path} at {startTime:HH:mm:ss.fff}");
        Console.WriteLine($"[Audit] RequestId: {requestContextService.RequestId}");
        Console.WriteLine($"[Audit] TransientId: {transientMarkerService.MarkerId}");
        Console.WriteLine($"[Audit] AppInstanceId: {_appInfoService.AppInstanceId}");

        // ДОБАВЛЯЕМ ЗАГОЛОВКИ ДО ВЫЗОВА next()
        // (пока ответ еще не начат)
        context.Response.Headers.Append("X-App-Instance", _appInfoService.AppInstanceId.ToString());
        context.Response.Headers.Append("X-Request-Id", requestContextService.RequestId.ToString());
        context.Response.Headers.Append("X-Transient-Id", transientMarkerService.MarkerId.ToString());

        // Выполняем следующий middleware
        await _next(context);

        // Для запросов в раздел /diag выводим дополнительную информацию
        if (context.Request.Path.StartsWithSegments("/diag"))
        {
            Console.WriteLine($"[Audit] DIAG запрос: {context.Request.Path}");
        }

        var endTime = DateTime.Now;
        var duration = (endTime - startTime).TotalMilliseconds;
        Console.WriteLine($"[Audit] Завершение запроса: {context.Request.Path} за {duration:F2} мс");
    }
}