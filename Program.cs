using Microsoft.AspNetCore.Routing;
using CampusRouteLab.Services;
using CampusRouteLab.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCampusServices();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseRequestAudit();

// GET / - стартовая страница
app.MapGet("/", () => Results.Json(new
{
    service = "CampusRouteLab",
    description = "Учебный диагностический веб-сервис для демонстрации маршрутизации и DI",
    sections = new
    {
        business = new[] { "/students", "/students/{group}", "/students/{group}/{id}", "/reports/{section?}", "/portal/{module=home}/{page=index}/{id?}", "/files/{**path}" },
        diagnostic = new[] { "/routes", "/diag/lifetimes", "/diag/lifetimes/check", "/diag/request-services", "/diag/app-services" }
    }
}));

// GET /students - список всех групп
app.MapGet("/students", (IStudentCatalogService catalog) =>
{
    var groups = catalog.GetAllGroups();
    var result = groups.Select(g => new
    {
        group = g.Key,
        studentCount = g.Value.Count,
        students = g.Value.Select(s => new { s.Id, s.FirstName, s.LastName, s.Email })
    });
    return Results.Json(result);
});

// GET /students/{group}
app.MapGet("/students/{group}", (string group, IStudentCatalogService catalog) =>
{
    var groupInfo = catalog.GetGroup(group);
    if (groupInfo == null)
        return Results.NotFound(new { error = $"Группа '{group}' не найдена" });

    return Results.Json(new
    {
        group = groupInfo.GroupName,
        studentCount = groupInfo.StudentCount,
        students = groupInfo.Students.Select(s => new { s.Id, s.FirstName, s.LastName, s.Email })
    });
});

// GET /students/{group}/{id}
app.MapGet("/students/{group}/{id}", (string group, int id, IStudentCatalogService catalog) =>
{
    var student = catalog.GetStudent(group, id);
    if (student == null)
        return Results.NotFound(new { error = $"Студент с ID {id} в группе '{group}' не найден" });

    return Results.Json(student);
});

// GET /reports/{section?}
app.MapGet("/reports/{section?}", (string? section) =>
{
    var actualSection = section ?? "overview";
    var reportData = new
    {
        section = actualSection,
        isDefault = section == null,
        timestamp = DateTime.Now,
        content = actualSection switch
        {
            "overview" => "Общий отчет о работе сервиса",
            "students" => "Отчет о студентах: всего 8 студентов в 3 группах",
            "performance" => "Отчет о производительности: среднее время ответа < 10мс",
            _ => $"Раздел '{actualSection}' находится в разработке"
        }
    };
    return Results.Json(reportData);
});

// GET /portal/{module=home}/{page=index}/{id?}
app.MapGet("/portal/{module=home}/{page=index}/{id?}", (string module, string page, string? id) =>
{
    return Results.Json(new
    {
        module,
        page,
        id = id ?? "(не указан)",
        isDefaultModule = module == "home",
        isDefaultPage = page == "index"
    });
});

// GET /files/{**path}
app.MapGet("/files/{**path}", (string path) =>
{
    return Results.Text($@"
╔══════════════════════════════════════════════════════════════╗
║                    CATCH-ALL ROUTE                          ║
╠══════════════════════════════════════════════════════════════╣
║ Запрошенный путь: /files/{path}                              ║
╚══════════════════════════════════════════════════════════════╝
");
});

// GET /routes - упрощенная версия без HttpMethodMetadata
app.MapGet("/routes", (EndpointDataSource endpointDataSource) =>
{
    var endpoints = endpointDataSource.Endpoints
        .OfType<RouteEndpoint>()
        .Select(e => e.RoutePattern.RawText)
        .Where(p => p != null)
        .Distinct()
        .OrderBy(p => p);

    var result = string.Join("\n", endpoints.Select(p => $"GET      {p}"));
    
    return Results.Text($@"
╔══════════════════════════════════════════════════════════════╗
║                 ЗАРЕГИСТРИРОВАННЫЕ МАРШРУТЫ                  ║
╠══════════════════════════════════════════════════════════════╣
{result}
╚══════════════════════════════════════════════════════════════╝
");
});

// GET /diag/lifetimes
app.MapGet("/diag/lifetimes", (
    IAppInfoService appInfo,
    IRequestContextService requestContext,
    ITransientMarkerService transientMarker,
    DiagnosticsReportService reportService) =>
{
    return Results.Json(new
    {
        direct_resolution = new
        {
            singleton = new { type = "Singleton", instanceId = appInfo.AppInstanceId, createdAt = appInfo.StartedAt },
            scoped = new { type = "Scoped", requestId = requestContext.RequestId, createdAt = requestContext.CreatedAt },
            transient = new { type = "Transient", markerId = transientMarker.MarkerId, createdAt = transientMarker.CreatedAt }
        },
        via_diagnostics_report = reportService.GetReport(),
        explanation = new
        {
            singleton = "Один экземпляр на всё приложение",
            scoped = "Один экземпляр на HTTP запрос",
            transient = "Новый экземпляр при каждом запросе"
        }
    });
});

// GET /diag/lifetimes/check
app.MapGet("/diag/lifetimes/check", (
    ITransientMarkerService transient1,
    ITransientMarkerService transient2,
    ITransientMarkerService transient3,
    IRequestContextService requestContext) =>
{
    return Results.Json(new
    {
        transient_resolutions = new[]
        {
            new { order = 1, markerId = transient1.MarkerId, createdAt = transient1.CreatedAt },
            new { order = 2, markerId = transient2.MarkerId, createdAt = transient2.CreatedAt },
            new { order = 3, markerId = transient3.MarkerId, createdAt = transient3.CreatedAt }
        },
        scoped_id = requestContext.RequestId,
        all_transients_equal = transient1.MarkerId == transient2.MarkerId && transient2.MarkerId == transient3.MarkerId,
        conclusion = transient1.MarkerId == transient2.MarkerId && transient2.MarkerId == transient3.MarkerId
            ? "ОШИБКА: Transient сервисы имеют одинаковые значения!"
            : "OK: Transient сервисы создают новые экземпляры при каждом разрешении"
    });
});

// GET /diag/request-services
app.MapGet("/diag/request-services", (HttpContext context) =>
{
    var requestContext = context.RequestServices.GetRequiredService<IRequestContextService>();
    var transientMarker = context.RequestServices.GetRequiredService<ITransientMarkerService>();
    
    return Results.Json(new
    {
        method = "Через HttpContext.RequestServices.GetRequiredService<T>()",
        scoped = new { requestId = requestContext.RequestId, createdAt = requestContext.CreatedAt },
        transient = new { markerId = transientMarker.MarkerId, createdAt = transientMarker.CreatedAt }
    });
});

// GET /diag/app-services
app.MapGet("/diag/app-services", (IServiceProvider serviceProvider) =>
{
    var appInfo = serviceProvider.GetRequiredService<IAppInfoService>();
    
    return Results.Json(new
    {
        method = "Через app.Services.GetRequiredService<T>()",
        singleton = new
        {
            type = "IAppInfoService",
            instanceId = appInfo.AppInstanceId,
            startedAt = appInfo.StartedAt,
            message = "Этот сервис зарегистрирован как Singleton"
        }
    });
});

// GET /about - вынесено в отдельный метод
app.MapGet("/about", GetAboutInfo);

static IResult GetAboutInfo(IAppInfoService appInfo)
{
    return Results.Json(new
    {
        name = "CampusRouteLab",
        version = "1.0.0",
        appInstanceId = appInfo.AppInstanceId,
        startedAt = appInfo.StartedAt,
        endpoints_count = 13,
        description = "Учебный проект для демонстрации маршрутизации и DI в ASP.NET Core"
    });
}

app.Run();
