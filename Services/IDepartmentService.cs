using EmployeeManagementSystem.DTOs;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Services;

public interface IDepartmentService
{
    Task<List<Department>> GetAllDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<Department?> GetDepartmentWithEmployeesAsync(int id);
    Task<Department> CreateDepartmentAsync(Department department);
    Task<bool> UpdateDepartmentAsync(int id, Department department);
    Task<bool> DeleteDepartmentAsync(int id);

    Task<List<Department>> FilterDepartmentsAsync(string? keyword);

    Task<(List<DepartmentListItemDto> Departments, int TotalRecords)> GetPagedDepartmentsAsync(
        string? keyword,
        int pageNumber,
        int pageSize
    );
}