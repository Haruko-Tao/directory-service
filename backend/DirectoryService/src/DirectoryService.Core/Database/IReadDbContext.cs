using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Core.Database;

public interface IReadDbContext 
{
    public IQueryable<Location> Locations { get; }
    public IQueryable<Department> Departments { get; }
    public IQueryable<Position> Positions { get; }
}