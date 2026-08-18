using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Departments.Features.AddLocation;
using DirectoryService.Core.Departments.Features.CreateDepartment;
using DirectoryService.Core.Departments.Features.GetDepartments;
using DirectoryService.Core.Departments.Features.RemoveLocation;
using DirectoryService.Core.Departments.Features.UpdateDepartments;
using DirectoryService.Core.Locations.Features.UpdateLocations;
using DirectoryService.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;


namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("departments")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Create([FromBody] CreateDepartmentRequest request,
        [FromServices] ICommandHandler<CreateDepartmentCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request.Name, request.Slug, request.ParentId,
            request.LocationIds);
        
        var departmentIdResult = await handler.Handle(command, cancellationToken);

        return departmentIdResult.ToApiResult(StatusCodes.Status201Created);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return NotFound();
    }

    [HttpGet]
    public async Task<IResult> GetAll([FromQuery] GetDepartmentsRequest request,
        [FromServices] IQueryHandler<GetDepartmentsQuery, IReadOnlyCollection<DepartmentResponse>> query,
        CancellationToken cancellationToken)
    {
        var command = new GetDepartmentsQuery(request.Page, request.PageSize);
        
        var getAllResult = await query.Handle(command, cancellationToken);

        return getAllResult.ToApiResult();
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request,
        [FromServices] ICommandHandler<UpdateDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentCommand(id ,request.Name);
        
        var updateResult = await handler.Handle(command, cancellationToken);
        return updateResult.ToApiResult();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id, CancellationToken cancellationToken)
    {
        return NoContent();
    }

    [HttpDelete("{departmentId:guid}/locations/{locationId:guid}")]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    public async Task<IResult> RemoveLocation([FromServices] ICommandHandler<RemoveLocationCommand> handler,
        Guid departmentId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveLocationCommand(departmentId, locationId);
        
        var removeResult = await handler.Handle(command, cancellationToken);

        return removeResult.ToApiResult();
    }

    [HttpPost("{departmentId:guid}/locations/{locationId:guid}")]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IResult> AddLocation([FromServices] ICommandHandler<AddLocationCommand> handler,
        Guid departmentId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var command = new AddLocationCommand(departmentId, locationId);
        
        var addLocationResult = await handler.Handle(command, cancellationToken);

        return addLocationResult.ToApiResult();
    }
}