namespace CampusRouteLab.Services;

public interface ITransientMarkerService
{
    Guid MarkerId { get; }
    DateTime CreatedAt { get; }
}