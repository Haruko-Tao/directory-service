using DirectoryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public  IActionResult Create([FromBody] CreateLocationRequest request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    public  IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return NotFound();
    }

    [HttpGet]
    public  IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(Array.Empty<LocationResponse>());
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id, CancellationToken cancellationToken)
    {
        return NoContent();
    }
}