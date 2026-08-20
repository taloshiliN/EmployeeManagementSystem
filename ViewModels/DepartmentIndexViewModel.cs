using EmployeeManagementSystem.DTOs;

namespace EmployeeManagementSystem.ViewModels;

public class DepartmentIndexViewModel
{
    public string? Keyword { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalRecords { get; set; }

    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(TotalRecords / (double)PageSize);

    public List<DepartmentListItemDto> Departments { get; set; } = new();
}
