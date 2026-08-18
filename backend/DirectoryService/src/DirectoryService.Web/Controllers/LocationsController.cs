using CSharpFunctionalExtensions;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Locations.Features.CreateLocation;
using DirectoryService.Core.Locations.Features.GetLocations;
using DirectoryService.Core.Locations.Features.UpdateLocations;
using DirectoryService.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("locations")]
public class LocationsController : ControllerBase
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
    public  IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return NotFound();
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
    public IActionResult Delete(Guid id, CancellationToken cancellationToken)
    {
        return NoContent();
    }
}