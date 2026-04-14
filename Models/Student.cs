namespace CampusRouteLab.Models;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class GroupInfo
{
    public string GroupName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public List<Student> Students { get; set; } = new();
}