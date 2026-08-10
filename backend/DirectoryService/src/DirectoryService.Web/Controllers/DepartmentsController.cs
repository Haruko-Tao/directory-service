using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using DirectoryService.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var departmentIdResult = await _departmentsService.Create(request, cancellationToken);

        if (departmentIdResult.IsFailure)
            return departmentIdResult.Error.ToActionResult();
        
        return CreatedAtAction(nameof(GetById), new { id = departmentIdResult.Value }, departmentIdResult.Value);
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var updateResult = await _departmentsService.Update(id, request, cancellationToken);

        if (updateResult.IsFailure)
            return updateResult.Error.ToActionResult();
        
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id, CancellationToken cancellationToken)
    {
        return NoContent();
    }

    [HttpDelete("{departmentId:guid}/locations/{locationId:guid}")]
    public async Task<IActionResult> RemoveLocation(Guid departmentId, Guid locationId, CancellationToken cancellationToken)
    {
        var removeResult = await _departmentsService.RemoveLocation(locationId, departmentId, cancellationToken);

        if (removeResult.IsFailure)
            return removeResult.Error.ToActionResult();
        
        return NoContent();
    }

    [HttpPost("{departmentId:guid}/locations/{locationId:guid}")]
    public async Task<IActionResult> AddLocation(Guid departmentId, Guid locationId,
        CancellationToken cancellationToken)
    {
        var addLocationResult = await _departmentsService.AddLocation(departmentId, locationId, cancellationToken);

        if (addLocationResult.IsFailure)
            return addLocationResult.Error.ToActionResult();

        return NoContent();
    }
}