using HarukoTech.Shared.Core.Abstractions;

namespace DirectoryService.Core.Locations.Features.GetLocationById;

public sealed record GetLocationByIdQuery(Guid Id) : IQuery;
