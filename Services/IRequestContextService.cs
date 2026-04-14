namespace CampusRouteLab.Services;

public interface IRequestContextService
{
    Guid RequestId { get; }
    DateTime CreatedAt { get; }
}