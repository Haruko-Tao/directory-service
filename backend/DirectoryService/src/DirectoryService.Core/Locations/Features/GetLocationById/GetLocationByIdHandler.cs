using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Locations.Features.GetLocationById;

public sealed class GetLocationByIdHandler : IQueryHandler<GetLocationByIdQuery, LocationResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetLocationByIdHandler> _logger;

    public GetLocationByIdHandler(IReadDbContext readDbContext,
        ILogger<GetLocationByIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }
    
    public async Task<Result<LocationResponse, Failure>> Handle(GetLocationByIdQuery query, CancellationToken cancellationToken)
    {
        var locationResponse = await _readDbContext.Locations.Where(l => l.Id == query.Id).Select(l =>
            new LocationResponse(l.Id,
                l.Name.Value,
                new AddressDto(l.Address.City, 
                    l.Address.Street,
                    l.Address.House,
                    l.Address.Apartment),
                l.CreatedAt,
                l.UpdatedAt)).FirstOrDefaultAsync(cancellationToken);

        if (locationResponse is null)
        {
            _logger.LogWarning("Локация с {LocationId} не была найдена", query.Id);
            return Error.NotFound("location.not.found", $"Не найдена локация c {query.Id}").ToFailure();
        }
        
        return locationResponse;
    }
}