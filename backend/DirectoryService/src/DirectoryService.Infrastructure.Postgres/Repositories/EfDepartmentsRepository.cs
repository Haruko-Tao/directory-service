using CSharpFunctionalExtensions;
using DirectoryService.Core.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public sealed class EfDepartmentsRepository : IDepartmentsRepository
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

    public Task RemoveAsync(Department department, CancellationToken cancellationToken)
    {
        _dbContext.Departments.Remove(department);

        return Task.CompletedTask;
    }

    public async Task AddDepartmentLocationAsync(DepartmentLocation departmentLocation, CancellationToken cancellationToken)
    {
        await _dbContext.DepartmentLocations.AddAsync(departmentLocation, cancellationToken);
    }

    public async Task<Result<Department, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var departmentResult = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return departmentResult ?? (Result<Department, Error>)Error.NotFound("department.not.found", $"Отдела с {id} не существует");
    }

    public async Task<bool> ExistsDepartmentLocationAsync(Guid locationId, Guid departmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.DepartmentLocations
            .AnyAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId,
                 cancellationToken);

    }

    public async Task<int> CountLinksForPositionAsync(Guid positionId, CancellationToken cancellationToken)
    {
        return await _dbContext.DepartmentPositions.
            CountAsync(dp => dp.PositionId == positionId, cancellationToken);
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

    public async Task AddDepartmentPositionAsync(DepartmentPosition departmentPosition, CancellationToken cancellationToken)
    {
        await _dbContext.DepartmentPositions.AddAsync(departmentPosition, cancellationToken);
    }

    public async Task<bool> ExistsDepartmentPositionAsync(Guid positionId, Guid departmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.DepartmentPositions.AnyAsync(
            dp => dp.PositionId == positionId && dp.DepartmentId == departmentId, cancellationToken);
    }

    public async Task<UnitResult<Error>> RemoveDepartmentPositionAsync(Guid positionId, Guid departmentId, CancellationToken cancellationToken)
    {
        var departmentPosition =
            await _dbContext.DepartmentPositions.FirstOrDefaultAsync(
                dp => dp.PositionId == positionId && dp.DepartmentId == departmentId, cancellationToken);

        if (departmentPosition is null)
            return Error.NotFound("department.position.not.found.link",
                $"Связь между отделом {departmentId} и позицией {positionId} не найдена");

        _dbContext.DepartmentPositions.Remove(departmentPosition);

        return UnitResult.Success<Error>();
    }

    public async Task<int> CountLinksForLocationAsync(Guid locationId, CancellationToken cancellationToken)
    {
        return await _dbContext.DepartmentLocations.CountAsync(dp => dp.LocationId == locationId, cancellationToken);
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
        return await _dbContext.Departments.AnyAsync(d => d.Slug == slug && d.ParentId == parentId, cancellationToken);
    }

    public async Task<int> CountChildrenAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.Departments.CountAsync(d => d.ParentId == departmentId, cancellationToken);
    }

    public async Task<int> CountLocationLinksForDepartmentAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.DepartmentLocations.CountAsync(dl => dl.DepartmentId == departmentId,
            cancellationToken);
    }

    public async Task<int> CountPositionLinksForDepartmentAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        return await _dbContext.DepartmentPositions.CountAsync(dp => dp.DepartmentId == departmentId,
            cancellationToken);
    }
}