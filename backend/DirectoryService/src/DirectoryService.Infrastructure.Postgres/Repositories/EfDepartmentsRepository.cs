using CSharpFunctionalExtensions;
using DirectoryService.Core.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class EfDepartmentsRepository : IDepartmentsRepository
{
    private readonly AppDbContext _dbContext;

    public EfDepartmentsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddAsync(Department department, CancellationToken cancellationToken)
    {
        await _dbContext.Departments.AddAsync(department, cancellationToken);
    }

    public async Task AddDepartmentLocationAsync(DepartmentLocation departmentLocation, CancellationToken cancellationToken)
    {
        await _dbContext.DepartmentLocations.AddAsync(departmentLocation, cancellationToken);
    }

    public async Task<Result<Department, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var departmentResult = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (departmentResult is null)
            return Error.NotFound("department.not.found", $"Отдела с {id} не существует");
        
        return departmentResult;
    }

    public async Task<bool> ExistsDepartmentLocationAsync(Guid locationId, Guid departmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.DepartmentLocations
            .AnyAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId,
                cancellationToken: cancellationToken);

    }

    public async Task<UnitResult<Error>> RemoveDepartmentLocationAsync(Guid locationId, Guid departmentId, CancellationToken cancellationToken)
    {
        var departmentLocation = await _dbContext.DepartmentLocations.FirstOrDefaultAsync(
                dl => dl.DepartmentId == departmentId && dl.LocationId == locationId, cancellationToken);

        if (departmentLocation is null)
            return Error.NotFound("department.location.not.found", "Связь не найдена");

        _dbContext.DepartmentLocations.Remove(departmentLocation);
        
        return UnitResult.Success<Error>(); 
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Departments
            .AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Departments
            .OrderBy(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSlugTakenAsync(Slug slug, Guid? parentId, CancellationToken cancellationToken)
    {
        return await _dbContext.Departments.AnyAsync(d => d.Slug.Value == slug.Value && d.ParentId == parentId, cancellationToken);
    }
    
}