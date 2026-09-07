using CSharpFunctionalExtensions;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Core.Locations.Features.GetLocations;

public sealed class GetLocationsHandler : IQueryHandler<GetLocationsQuery, PagedResult<LocationListItemDto>>
{
    private readonly ILocationsReadRepository _locationsReadRepository;
    private readonly IValidator<GetLocationsQuery> _validator;
    
    public GetLocationsHandler(ILocationsReadRepository locationsReadRepository,
        IValidator<GetLocationsQuery> validator)
    {
        _locationsReadRepository = locationsReadRepository;
        _validator = validator;
    }
    
    public async Task<Result<PagedResult<LocationListItemDto>, Failure>> Handle(GetLocationsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(s => (Error)s.CustomState!));
        
        return await _locationsReadRepository.GetPageAsync(query, cancellationToken);
    }
    
}