using CampusRouteLab.Models;

namespace CampusRouteLab.Services;

public class StudentCatalogService : IStudentCatalogService
{
    private readonly Dictionary<string, List<Student>> _groups;

    public StudentCatalogService()
    {
        // Инициализация тестовых данных
        _groups = new Dictionary<string, List<Student>>
        {
            ["CS-101"] = new List<Student>
            {
                new() { Id = 1, FirstName = "Иван", LastName = "Иванов", Email = "ivanov@university.ru" },
                new() { Id = 2, FirstName = "Петр", LastName = "Петров", Email = "petrov@university.ru" },
                new() { Id = 3, FirstName = "Мария", LastName = "Сидорова", Email = "sidorova@university.ru" }
            },
            ["CS-102"] = new List<Student>
            {
                new() { Id = 4, FirstName = "Анна", LastName = "Кузнецова", Email = "kuznetsova@university.ru" },
                new() { Id = 5, FirstName = "Дмитрий", LastName = "Соколов", Email = "sokolov@university.ru" }
            },
            ["DS-201"] = new List<Student>
            {
                new() { Id = 6, FirstName = "Елена", LastName = "Волкова", Email = "volkova@university.ru" },
                new() { Id = 7, FirstName = "Алексей", LastName = "Морозов", Email = "morozov@university.ru" },
                new() { Id = 8, FirstName = "Татьяна", LastName = "Новикова", Email = "novikova@university.ru" }
            }
        };
    }

    public Dictionary<string, List<Student>> GetAllGroups() => _groups;

    public GroupInfo? GetGroup(string groupName)
    {
        if (!_groups.ContainsKey(groupName))
            return null;

        return new GroupInfo
        {
            GroupName = groupName,
            StudentCount = _groups[groupName].Count,
            Students = _groups[groupName]
        };
    }

    public Student? GetStudent(string groupName, int studentId)
    {
        if (!_groups.ContainsKey(groupName))
            return null;

        return _groups[groupName].FirstOrDefault(s => s.Id == studentId);
    }

    public int GetStudentCount(string groupName)
    {
        return _groups.ContainsKey(groupName) ? _groups[groupName].Count : 0;
    }
}