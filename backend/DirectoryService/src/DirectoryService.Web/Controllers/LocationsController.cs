using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Locations;
using DirectoryService.SharedKernel;
using DirectoryService.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Controllers;

[ApiController]
[Route("locations")]
public class LocationsController : ControllerBase
{
    private readonly LocationsService _locationsService;

    public LocationsController(LocationsService locationsService)
    {
        _locationsService = locationsService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLocationRequest request, CancellationToken cancellationToken)
    {
        var locationIdResult = await _locationsService.Create(request, cancellationToken);

        if (locationIdResult.IsFailure)
            return locationIdResult.Error.ToActionResult();
        return CreatedAtAction(nameof(GetById), new { id = locationIdResult.Value }, locationIdResult.Value);
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

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        var updateResult = await _locationsService.Update(id, request, cancellationToken);

        if (updateResult.IsFailure)
            return updateResult.Error.ToActionResult();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id, CancellationToken cancellationToken)
    {
        return NoContent();
    }
}