namespace EmployeeManagementSystem.DTOs;

public class DepartmentListItemDto
{
    public int Id { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int EmployeeCount { get; set; }
}