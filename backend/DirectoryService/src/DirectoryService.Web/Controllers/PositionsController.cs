using DirectoryService.Contracts;
using DirectoryService.Contracts.Positions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Positions.Features.CreatePosition;
using DirectoryService.Core.Positions.Features.DeletePosition;
using DirectoryService.Core.Positions.Features.UpdatePosition;
using DirectoryService.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("positions")]
public sealed class PositionsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Create([FromServices] ICommandHandler<CreatePositionCommand, Guid> handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request.Name);

        var result = await handler.Handle(command, cancellationToken);

        return result.ToApiResult(StatusCodes.Status201Created);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return NotFound();
    }

    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(Array.Empty<PositionResponse>());
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Update(Guid id,
        [FromServices] ICommandHandler<UpdatePositionCommand> handler,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePositionCommand(id, request.Name);

        var result = await handler.Handle(command, cancellationToken);

        return result.ToApiResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Delete(Guid id,
        [FromServices] ICommandHandler<DeletePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new DeletePositionCommand(id);

        var result = await handler.Handle(command, cancellationToken);

        return result.ToApiResult();
    }
}