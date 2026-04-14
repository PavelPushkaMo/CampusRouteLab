namespace CampusRouteLab.Services;

public class RequestContextService : IRequestContextService
{
    public Guid RequestId { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.Now;

    public RequestContextService()
    {
        Console.WriteLine($"[Scoped] RequestContextService создан. RequestId: {RequestId}");
    }
}