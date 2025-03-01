using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LMS_API.Controllers.Departments.Commands;
using LMS_API.Controllers.Departments.ViewModels;

public class DepartmentService
{
    private readonly IMapper _mapper;
    private readonly DBContext _context;
    private readonly ILogger<DepartmentService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DepartmentService(
        IMapper mapper,
        DBContext context,
        ILogger<DepartmentService> logger,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _mapper = mapper;
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<DepartmentVM>> GetAllDepartments()
    {
        return await _context.Departments
            .Select(d => _mapper.Map<DepartmentVM>(d))
            .ToListAsync();
    }

    public async Task<DepartmentVM> GetDepartmentById(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        return department == null ? null : _mapper.Map<DepartmentVM>(department);
    }

    public async Task<DepartmentVM> CreateDepartment(CreateDepartmentCommand command)
    {
        var supervisor = await _userManager.FindByIdAsync(command.SupervisorId);

        if (supervisor == null || !(await _userManager.IsInRoleAsync(supervisor, "Supervisor")))
        {
            _logger.LogWarning("Attempt to create a department with a non-supervisor user as supervisor.");
            throw new UnauthorizedAccessException("The specified user is not a supervisor.");
        }

        var department = _mapper.Map<Department>(command);
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return _mapper.Map<DepartmentVM>(department);
    }

    public async Task<bool> EditDepartment(int id, EditDepartmentCommand command)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null) return false;

        _mapper.Map(command, department);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDepartment(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null) return false;

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DepartmentTranslationVM>> GetAllTranslations()
    {
        return await _context.DepartmentTranslations
            .Select(dt => _mapper.Map<DepartmentTranslationVM>(dt))
            .ToListAsync();
    }

    public async Task<DepartmentTranslationVM> GetTranslationById(int id)
    {
        var translation = await _context.DepartmentTranslations.FindAsync(id);
        return translation == null ? null : _mapper.Map<DepartmentTranslationVM>(translation);
    }

    public async Task<DepartmentTranslationVM> CreateTranslation(CreateDepartmentTranslationCommand command)
    {
        var translation = _mapper.Map<DepartmentTranslation>(command);
        _context.DepartmentTranslations.Add(translation);
        await _context.SaveChangesAsync();
        return _mapper.Map<DepartmentTranslationVM>(translation);
    }

    public async Task<bool> EditTranslation(int id, EditDepartmentTranslationCommand command)
    {
        var translation = await _context.DepartmentTranslations.FindAsync(id);
        if (translation == null) return false;

        _mapper.Map(command, translation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTranslation(int id)
    {
        var translation = await _context.DepartmentTranslations.FindAsync(id);
        if (translation == null) return false;

        _context.DepartmentTranslations.Remove(translation);
        await _context.SaveChangesAsync();
        return true;
    }
}
