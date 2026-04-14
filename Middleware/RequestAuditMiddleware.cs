using CampusRouteLab.Services;

namespace CampusRouteLab.Middleware;

public class RequestAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAppInfoService _appInfoService;

    // Singleton сервис можно получать через конструктор
    public RequestAuditMiddleware(RequestDelegate next, IAppInfoService appInfoService)
    {
        _next = next;
        _appInfoService = appInfoService;
    }

    // Scoped и Transient сервисы получаем через параметры InvokeAsync!
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

        // Сохраняем информацию для добавления в заголовки после выполнения
        var requestId = requestContextService.RequestId;
        var transientId = transientMarkerService.MarkerId;
        var appInstanceId = _appInfoService.AppInstanceId;

        // Выполняем следующий middleware
        await _next(context);

        // Добавляем диагностические заголовки в ответ
        context.Response.Headers.Append("X-App-Instance", appInstanceId.ToString());
        context.Response.Headers.Append("X-Request-Id", requestId.ToString());
        context.Response.Headers.Append("X-Transient-Id", transientId.ToString());

        // Для запросов в раздел /diag выводим дополнительную информацию
        if (context.Request.Path.StartsWithSegments("/diag"))
        {
            Console.WriteLine($"[Audit] DIAG запрос: {context.Request.Path}");
            Console.WriteLine($"[Audit] Scoped RequestId: {requestId}");
            Console.WriteLine($"[Audit] Transient MarkerId: {transientId}");
        }

        var endTime = DateTime.Now;
        var duration = (endTime - startTime).TotalMilliseconds;
        Console.WriteLine($"[Audit] Завершение запроса: {context.Request.Path} за {duration:F2} мс");
    }
}