namespace CampusRouteLab.Services;

public class DiagnosticsReportService
{
    private readonly IAppInfoService _appInfoService;
    private readonly IRequestContextService _requestContextService;
    private readonly ITransientMarkerService _transientMarkerService;

    public DiagnosticsReportService(
        IAppInfoService appInfoService,
        IRequestContextService requestContextService,
        ITransientMarkerService transientMarkerService)
    {
        _appInfoService = appInfoService;
        _requestContextService = requestContextService;
        _transientMarkerService = transientMarkerService;
    }

    public object GetReport()
    {
        return new
        {
            singleton = new
            {
                type = "Singleton",
                instanceId = _appInfoService.AppInstanceId,
                createdAt = _appInfoService.StartedAt
            },
            scoped = new
            {
                type = "Scoped",
                requestId = _requestContextService.RequestId,
                createdAt = _requestContextService.CreatedAt
            },
            transient = new
            {
                type = "Transient",
                markerId = _transientMarkerService.MarkerId,
                createdAt = _transientMarkerService.CreatedAt
            },
            message = "Через DiagnosticsReportService (конструкторная инъекция)"
        };
    }
}