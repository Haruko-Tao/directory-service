using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using DirectoryService.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;


namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("departments")]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentsService _departmentsService;
    
    public DepartmentsController(DepartmentsService departmentsService)
    {
        _departmentsService = departmentsService;
    }
    
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var departmentIdResult = await _departmentsService.Create(request, cancellationToken);

        return departmentIdResult.ToApiResult(StatusCodes.Status201Created);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return NotFound();
    }

    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(Array.Empty<DepartmentResponse>());
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var updateResult = await _departmentsService.Update(id, request, cancellationToken);
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
    public async Task<IResult> RemoveLocation(Guid departmentId, Guid locationId, CancellationToken cancellationToken)
    {
        var removeResult = await _departmentsService.RemoveLocation(locationId, departmentId, cancellationToken);

        return removeResult.ToApiResult();
    }

    [HttpPost("{departmentId:guid}/locations/{locationId:guid}")]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<Envelope<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IResult> AddLocation(Guid departmentId, Guid locationId,
        CancellationToken cancellationToken)
    {
        var addLocationResult = await _departmentsService.AddLocation(departmentId, locationId, cancellationToken);

        return addLocationResult.ToApiResult();
    }
}