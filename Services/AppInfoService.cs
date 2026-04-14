namespace CampusRouteLab.Services;

public class AppInfoService : IAppInfoService
{
    public Guid AppInstanceId { get; } = Guid.NewGuid();
    public DateTime StartedAt { get; } = DateTime.Now;

    public AppInfoService()
    {
        Console.WriteLine($"[Singleton] AppInfoService создан. InstanceId: {AppInstanceId}");
    }
}