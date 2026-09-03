using CSharpFunctionalExtensions;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Locations.Features.CreateLocation;
using DirectoryService.Core.Locations.Features.DeleteLocation;
using DirectoryService.Core.Locations.Features.GetLocationById;
using DirectoryService.Core.Locations.Features.GetLocations;
using DirectoryService.Core.Locations.Features.GetTopLocations;
using DirectoryService.Core.Locations.Features.UpdateLocation;
using DirectoryService.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("locations")]
public sealed class LocationsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Create([FromBody] CreateLocationRequest request,
        [FromServices] ICommandHandler<CreateLocationCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request.Name, request.Address);
        
        var result = await handler.Handle(command, cancellationToken);

        return result.ToApiResult(StatusCodes.Status201Created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Envelope<LocationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public  async Task<IResult> GetById(Guid id,
        [FromServices] IQueryHandler<GetLocationByIdQuery, LocationResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationByIdQuery(id);

        var queryResult = await handler.Handle(query, cancellationToken);
        
        return queryResult.ToApiResult();
    }

    [HttpGet]
    public  async Task<IResult> GetAll([FromQuery] GetLocationsRequest request,
        [FromServices]IQueryHandler<GetLocationsQuery, IReadOnlyCollection<LocationResponse>> query,
        CancellationToken cancellationToken)
    {
        var command = new GetLocationsQuery(request.Page, request.PageSize);
        
        var getAllResult = await query.Handle(command, cancellationToken);

        return getAllResult.ToApiResult();
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateLocationRequest request,
        [FromServices] ICommandHandler<UpdateLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(id, request.Name, request.Address);
        
        var updateResult = await handler.Handle(command, cancellationToken);

        return updateResult.ToApiResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Delete([FromServices] ICommandHandler<DeleteLocationCommand> handler, Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteLocationCommand(id);

        var deleteResult = await handler.Handle(command, cancellationToken);

        return deleteResult.ToApiResult();
    }

    [HttpGet("top")]
    [ProducesResponseType<Envelope<IReadOnlyCollection<TopLocationsResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetTop([FromServices] IQueryHandler<GetTopLocationsQuery, IReadOnlyCollection<TopLocationsResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetTopLocationsQuery();

        var result = await handler.Handle(query, cancellationToken);

        return result.ToApiResult();
    }
}