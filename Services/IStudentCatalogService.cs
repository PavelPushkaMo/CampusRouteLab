using CampusRouteLab.Models;

namespace CampusRouteLab.Services;

public interface IStudentCatalogService
{
    Dictionary<string, List<Student>> GetAllGroups();
    GroupInfo? GetGroup(string groupName);
    Student? GetStudent(string groupName, int studentId);
    int GetStudentCount(string groupName);
}