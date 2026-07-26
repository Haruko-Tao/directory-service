using DirectoryService.Contracts.Positions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("positions")]
public class PositionsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreatePositionRequest request, CancellationToken cancellationToken)
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
        return Ok(Array.Empty<PositionResponse>());
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] UpdatePositionRequest request, CancellationToken cancellationToken)
    {
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id, CancellationToken cancellationToken)
    {
        return NoContent();
    }
}