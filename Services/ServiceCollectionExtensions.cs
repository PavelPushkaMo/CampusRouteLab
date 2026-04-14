namespace CampusRouteLab.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCampusServices(this IServiceCollection services)
    {
        // Singleton - один экземпляр на всё приложение
        services.AddSingleton<IStudentCatalogService, StudentCatalogService>();
        services.AddSingleton<IAppInfoService, AppInfoService>();
        
        // Scoped - один экземпляр на HTTP запрос
        services.AddScoped<IRequestContextService, RequestContextService>();
        
        // Transient - новый экземпляр при каждом запросе
        services.AddTransient<ITransientMarkerService, TransientMarkerService>();
        services.AddTransient<DiagnosticsReportService>();
        
        return services;
    }
}