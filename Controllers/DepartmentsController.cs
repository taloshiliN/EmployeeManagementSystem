using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Application")]
public class DepartmentsController : Controller
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index(string? keyword, int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        var (departments, totalRecords) =
            await _departmentService.GetPagedDepartmentsAsync(keyword, pageNumber, pageSize);

        var viewModel = new DepartmentIndexViewModel
        {
            Keyword = keyword,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            Departments = departments
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var department = await _departmentService.GetDepartmentWithEmployeesAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    public IActionResult Create()
    {
        return View(new CreateDepartmentViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDepartmentViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var department = new Department
        {
            DepartmentName = viewModel.DepartmentName,
            Description = viewModel.Description,
        };

        try
        {
            await _departmentService.CreateDepartmentAsync(department);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(viewModel);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        var viewModel = new EditDepartmentViewModel
        {
            Id = department.Id,
            DepartmentName = department.DepartmentName,
            Description = department.Description,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditDepartmentViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var department = new Department
        {
            Id = viewModel.Id,
            DepartmentName = viewModel.DepartmentName,
            Description = viewModel.Description,
        };

        try
        {
            var updated = await _departmentService.UpdateDepartmentAsync(id, department);
            if (!updated)
            {
                return NotFound();
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(viewModel);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var department = await _departmentService.GetDepartmentWithEmployeesAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var deleted = await _departmentService.DeleteDepartmentAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }
}