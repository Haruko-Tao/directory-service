using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class LocationTests : IAsyncLifetime
{
    
    private sealed record CreatedResponse(Guid Data);

    private sealed record ErrorItem(string Code, string Message, string Type);

    private sealed record ErrorResponse(ErrorItem[] Errors);
    
    private readonly HttpClient _httpClient;

    private readonly IntegrationTestWebFactory _webFactory;

    public LocationTests(IntegrationTestWebFactory webFactory)
    {
        _httpClient = webFactory.HttpClient;
        _webFactory = webFactory;
    }

    public async Task InitializeAsync()
    {
        await _webFactory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get_by_unknown_id_returns_404()
    {
        var response = await _httpClient.GetAsync(new Uri($"/locations/{Guid.NewGuid()}", UriKind.Relative));
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_location_returns_201_and_saves_row()
    {
        var address = new AddressDto("Воронеж","Мира", "1", null);
        var request = new CreateLocationRequest("Офис на Мира", address);

        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/locations", UriKind.Relative), request);
        
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<CreatedResponse>();

        Assert.NotNull(envelope);
        
        Assert.NotEqual(Guid.Empty, envelope.Data);

        using var scope = _webFactory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var saved = await dbContext.Locations.FirstOrDefaultAsync(l => l.Id == envelope.Data);

        Assert.NotNull(saved);

        Assert.Equal(request.Name, saved.Name.Value);
    }

    [Fact]
    public async Task Delete_location_with_departments_return_409()
    {
        //Arrange
        var address = new AddressDto("Воронеж", "Урицкого", "2", null);
        var requestLocation = new CreateLocationRequest("Офис в Подвале", address);

        var responseLocation =
            await _httpClient.PostAsJsonAsync(new Uri($"/locations", UriKind.Relative), requestLocation);

        var envelopeLocationId = await responseLocation.Content.ReadFromJsonAsync<CreatedResponse>();
        
        Assert.NotNull(envelopeLocationId);

        var requestDepartment = new CreateDepartmentRequest("Сортировка", "sortirovka", null, [envelopeLocationId.Data]);

        var responseDepartment =
            await _httpClient.PostAsJsonAsync(new Uri($"/departments", UriKind.Relative), requestDepartment);
        
        Assert.Equal(HttpStatusCode.Created, responseDepartment.StatusCode);
        
        //Act

        var responseDelete = await 
            _httpClient.DeleteAsync(new Uri($"/locations/{envelopeLocationId.Data}", UriKind.Relative));
        
        //Assert
        
        Assert.Equal(HttpStatusCode.Conflict, responseDelete.StatusCode);

        var error = await responseDelete.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        
        Assert.NotEmpty(error.Errors);
        
        Assert.Equal("CONFLICT", error.Errors[0].Type);

        using var scope = _webFactory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var linkStillExitsts = await dbContext.DepartmentLocations.AnyAsync(dl => dl.LocationId == envelopeLocationId.Data);

        Assert.True(linkStillExitsts);
        
        var locationStillExtists = await dbContext.Locations.AnyAsync(l => l.Id == envelopeLocationId.Data);

        Assert.True(locationStillExtists);


    }
}