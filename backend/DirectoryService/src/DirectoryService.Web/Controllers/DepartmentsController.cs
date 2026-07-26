using DirectoryService.Contracts.Departments;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("departments")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    { 
        var id = Guid.NewGuid();
        return CreatedAtAction(nameof(GetById), new { id }, id);
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

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id, CancellationToken cancellationToken)
    {
        return NoContent();
    }
}