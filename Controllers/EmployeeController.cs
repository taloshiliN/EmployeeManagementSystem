using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers;
[Authorize(AuthenticationSchemes = "Identity.Application")]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentservice;
    private readonly IJobTitleService _jobTitleService;
    public EmployeesController(
        IEmployeeService employeeService,
        IDepartmentService departmentService,
        IJobTitleService jobTitleService
        )
    {
        _employeeService = employeeService;
        _departmentservice = departmentService;
        _jobTitleService = jobTitleService;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return View(employees);
    }

    public async Task<IActionResult> Details(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);

        if (employee == null)
        {
            return NotFound();
        }
        return View(employee);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateEmployeeViewModel();
        await PopulateDropDownsAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropDownsAsync(viewModel);
            return View(viewModel);
        }

        var employee = new Employee
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            Email = viewModel.Email,
            Position = viewModel.Position,
            DepartmentId = viewModel.DepartmentId,
            JobTitleId = viewModel.JobTitleId,
            Salary = viewModel.Salary
        };

        try
        {
            await _employeeService.CreateEmployeeAsync(employee);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropDownsAsync(viewModel);
            return View(viewModel);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        var viewModel = new EditEmployeeViewModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Position = employee.Position,
            DepartmentId = employee.DepartmentId,
            JobTitleId = employee.JobTitleId,
            Salary = employee.Salary
        };

        await PopulateDropDownsAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditEmployeeViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropDownsAsync(viewModel);
            return View(viewModel);
        }

        var employee = new Employee
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            Email = viewModel.Email,
            Position = viewModel.Position,
            DepartmentId = viewModel.DepartmentId,
            JobTitleId = viewModel.JobTitleId,
            Salary = viewModel.Salary
        };

        try
        {
            var updated = await _employeeService.UpdateEmployeeAsync(id, employee);
            if (!updated)
            {
                return NotFound();
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropDownsAsync(viewModel);
            return View(viewModel);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _employeeService.DeleteEmployeeAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropDownsAsync(EmployeeFormViewModel viewModel)
    {
        var departments = await _departmentservice.GetAllDepartmentsAsync();
        var jobtitles = await _jobTitleService.GetAllJobTitlesAsync();

        viewModel.Departments = departments.Select(d => new SelectListItem
        {
            Value = d.Id.ToString(),
            Text = d.DepartmentName
        }).ToList();

        viewModel.JobTitles = jobtitles.Select(j => new SelectListItem
        {
            Value = j.Id.ToString(),
            Text = j.Title
        }).ToList();
    }
}
