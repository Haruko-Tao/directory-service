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
    private readonly ILogger<EfDepartmentsRepository> _logger;
    

    public EfDepartmentsRepository(AppDbContext dbContext, ILogger<EfDepartmentsRepository> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task<UnitResult<Error>> AddAsync(Department department, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Departments.AddAsync(department, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить отдел с {DepartmentId}", department.Id);
            return Error.Internal("department.not.save", "Не удалось сохранить отдел");
        }
    }

    public async Task<UnitResult<Error>> AddDepartmentLocationAsync(DepartmentLocation departmentLocation, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.DepartmentLocations.AddAsync(departmentLocation, cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить связь с id {DepartmentLocation}", departmentLocation.Id);
            return Error.Internal("departmentlocation.not.save", "Не удалось сохранить связь отдела и локации");
        }
    }

    public async Task<Result<Department, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var departmentResult = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (departmentResult is null)
            return Error.NotFound("department.not.found", $"Отдела с {id} не существует");
        
        return departmentResult;
    }

    public async Task<UnitResult<Error>> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EF Core не удалось сохранить изменения в БД");

            return Error.Internal("ef.core.not.save", "EF CORE не смог сохранить данные в БД");
        }
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

        if (departmentLocation is not null)
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

    public async Task<bool> IsSlugTakenAsync(string slug, Guid? parentId, CancellationToken cancellationToken)
    {
        var slugResult = Slug.Create(slug);

        return await _dbContext.Departments.AnyAsync(d => d.Slug == slugResult.Value! && d.ParentId == parentId, cancellationToken);
    }
}