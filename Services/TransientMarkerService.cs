namespace CampusRouteLab.Services;

public class TransientMarkerService : ITransientMarkerService
{
    public Guid MarkerId { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.Now;

    public TransientMarkerService()
    {
        Console.WriteLine($"[Transient] TransientMarkerService создан. MarkerId: {MarkerId}");
    }
}