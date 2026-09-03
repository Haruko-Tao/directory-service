using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Core.Locations.Features.GetTopLocations;

public sealed class GetTopLocationsHandler : IQueryHandler<GetTopLocationsQuery, IReadOnlyCollection<TopLocationsResponse>>
{
    private readonly IReadDbContext _readDbContext;

    public GetTopLocationsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<IReadOnlyCollection<TopLocationsResponse>, Failure>> Handle(GetTopLocationsQuery query,
        CancellationToken cancellationToken)
    {
        return await _readDbContext.Locations.GroupJoin(_readDbContext.DepartmentLocations, l => l.Id,
            dl => dl.LocationId, (location, departmentLinks) => new 
            { 
                Location = location,
                DepartmentCount = departmentLinks.Count()
            })
            .OrderByDescending(lp => lp.DepartmentCount)
            .Take(5)
            .Select(row => new TopLocationsResponse(row.Location.Id,
                row.Location.Name.Value,
                new AddressDto(row.Location.Address.City,
                    row.Location.Address.Street,
                    row.Location.Address.House,
                    row.Location.Address.Apartment), row.DepartmentCount))
            .ToListAsync(cancellationToken);
    }
}