namespace CampusRouteLab.Services;

public interface IAppInfoService
{
    Guid AppInstanceId { get; }
    DateTime StartedAt { get; }
}