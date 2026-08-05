using DirectoryService.Core.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
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

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}