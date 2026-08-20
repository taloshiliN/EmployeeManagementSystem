using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.ViewModels;

public class DepartmentFormViewModel
{
    [Required]
    public string DepartmentName { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

}
