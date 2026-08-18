using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Locations.Extensions;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Core.Locations.Features.GetLocations;

public sealed class GetLocationsHandler : IQueryHandler<GetLocationsQuery, IReadOnlyCollection<LocationResponse>>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<GetLocationsQuery> _validator;
    
    public GetLocationsHandler(ILocationsRepository locationsRepository,
        IValidator<GetLocationsQuery> validator)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
    }
    
    public async Task<Result<IReadOnlyCollection<LocationResponse>, Failure>> Handle(GetLocationsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));

        var locationResult = await _locationsRepository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

        return locationResult.Select(l => l.ToResponse()).ToList();
    }
}