using DirectoryService.Core.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres;

public class ReadDbContext : DbContext, IReadDbContext
{
    private readonly string _connectionString;
    private readonly ILoggerFactory _loggerFactory;
    
    public ReadDbContext(string connectionString,
        ILoggerFactory loggerFactory)
    {
        _connectionString = connectionString;
        _loggerFactory = loggerFactory;
    }

    public IQueryable<Location> Locations => Set<Location>().AsNoTracking();
    public IQueryable<Department> Departments => Set<Department>().AsNoTracking();
    public IQueryable<Position> Positions => Set<Position>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString).UseLoggerFactory(_loggerFactory);
    }
}