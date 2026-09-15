using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

/// <summary>
/// СГЕНЕРИРОВАНО AI по образцу трёх ручных тестов из LocationTests.
/// Подлежит ревью (пункт 7 задания). Удаляется одним файлом.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public class LocationsGeneratedTests : IAsyncLifetime
{
    // Зеркальные типы под форму ответа: Envelope<T> и Failure пока не разбираются обратно.
    private sealed record DataResponse<T>(T Data);

    private sealed record ErrorItem(string Code, string Message, string Type);

    private sealed record ErrorResponse(ErrorItem[] Errors);

    private readonly HttpClient _httpClient;
    private readonly IntegrationTestWebFactory _webFactory;

    public LocationsGeneratedTests(IntegrationTestWebFactory webFactory)
    {
        _httpClient = webFactory.HttpClient;
        _webFactory = webFactory;
    }

    public async Task InitializeAsync() => await _webFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------- вспомогательные методы подготовки данных ----------

    private static AddressDto SampleAddress() => new("Воронеж", "Мира", "1", null);

    private async Task<Guid> CreateLocationAsync(string name)
    {
        var request = new CreateLocationRequest(name, SampleAddress());

        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/locations", UriKind.Relative), request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<Guid>>();
        Assert.NotNull(body);

        return body.Data;
    }

    private async Task<Guid> CreateDepartmentAsync(string name, string slug, params Guid[] locationIds)
    {
        var request = new CreateDepartmentRequest(name, slug, null, locationIds);

        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/departments", UriKind.Relative), request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<Guid>>();
        Assert.NotNull(body);

        return body.Data;
    }

    private static AppDbContext DbContext(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // ---------- CREATE ----------

    [Fact]
    public async Task Create_location_with_empty_name_returns_400()
    {
        // Arrange
        var request = new CreateLocationRequest(string.Empty, SampleAddress());

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/locations", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "name.not.space");

        using var scope = _webFactory.Services.CreateScope();
        var anyLocation = await DbContext(scope).Locations.AnyAsync();
        Assert.False(anyLocation);
    }

    [Fact]
    public async Task Create_location_with_empty_city_returns_400()
    {
        // Arrange
        var address = new AddressDto(string.Empty, "Мира", "1", null);
        var request = new CreateLocationRequest("Офис без города", address);

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/locations", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "city.not.empty");
    }

    [Fact]
    public async Task Create_location_with_taken_name_returns_409()
    {
        // Arrange
        const string name = "Офис на Мира";
        await CreateLocationAsync(name);

        var duplicate = new CreateLocationRequest(name, SampleAddress());

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/locations", UriKind.Relative), duplicate);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "is.name.taken");

        using var scope = _webFactory.Services.CreateScope();
        var count = await DbContext(scope).Locations.CountAsync();
        Assert.Equal(1, count);
    }

    // ---------- GET BY ID ----------

    [Fact]
    public async Task Get_location_by_id_returns_200_and_card()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на Мира");

        // Act
        var response = await _httpClient.GetAsync(
            new Uri($"/locations/{locationId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<LocationResponse>>();
        Assert.NotNull(body);
        Assert.Equal(locationId, body.Data.Id);
        Assert.Equal("Офис на Мира", body.Data.Name);
        Assert.Equal("Воронеж", body.Data.Address.City);
        Assert.Null(body.Data.Address.Apartment);
    }

    // ---------- LIST ----------

    [Fact]
    public async Task Get_locations_returns_200_and_all_rows()
    {
        // Arrange
        await CreateLocationAsync("Альфа");
        await CreateLocationAsync("Бета");

        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/locations", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<DataResponse<PagedResult<LocationListItemDto>>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Data.TotalCount);
        Assert.Equal(2, body.Data.Items.Count);
        Assert.Contains(body.Data.Items, i => i.Name == "Альфа");
        Assert.Contains(body.Data.Items, i => i.Name == "Бета");
    }

    [Fact]
    public async Task Get_locations_filters_by_search()
    {
        // Arrange
        await CreateLocationAsync("Альфа");
        await CreateLocationAsync("Бета");

        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/locations?search=аль", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<DataResponse<PagedResult<LocationListItemDto>>>();

        Assert.NotNull(body);
        Assert.Equal(1, body.Data.TotalCount);
        Assert.Equal("Альфа", body.Data.Items.Single().Name);
    }

    [Fact]
    public async Task Get_locations_counts_linked_departments()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на Мира");
        await CreateDepartmentAsync("Продажи", "prodazhi", locationId);

        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/locations", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<DataResponse<PagedResult<LocationListItemDto>>>();

        Assert.NotNull(body);
        Assert.Equal(1, body.Data.Items.Single().DepartmentCount);
    }

    [Fact]
    public async Task Get_locations_with_unknown_sort_by_returns_400()
    {
        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/locations?sortBy=bogus", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "sort.by.invalid");
    }

    [Fact]
    public async Task Get_locations_with_zero_page_returns_400()
    {
        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/locations?page=0", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "page.invalid");
    }

    // ---------- UPDATE ----------

    [Fact]
    public async Task Update_location_changes_name_and_address()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Старое имя");
        var newAddress = new AddressDto("Москва", "Тверская", "10", "5");
        var request = new UpdateLocationRequest("Новое имя", newAddress);

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/locations/{locationId}", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        var saved = await DbContext(scope).Locations
            .FirstOrDefaultAsync(l => l.Id == locationId);

        Assert.NotNull(saved);
        Assert.Equal("Новое имя", saved.Name.Value);
        Assert.Equal("Москва", saved.Address.City);
        Assert.Equal("5", saved.Address.Apartment);
    }

    [Fact]
    public async Task Update_location_with_unknown_id_returns_404()
    {
        // Arrange
        var request = new UpdateLocationRequest("Новое имя", SampleAddress());

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/locations/{Guid.NewGuid()}", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_location_with_empty_name_returns_400_and_keeps_old_value()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Старое имя");
        var request = new UpdateLocationRequest(string.Empty, SampleAddress());

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/locations/{locationId}", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        var saved = await DbContext(scope).Locations
            .FirstOrDefaultAsync(l => l.Id == locationId);

        Assert.NotNull(saved);
        Assert.Equal("Старое имя", saved.Name.Value);
    }

    // ---------- DELETE ----------

    [Fact]
    public async Task Delete_location_without_links_marks_as_deleted()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на снос");

        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/locations/{locationId}", UriKind.Relative));

        // Assert
        //status this Operations
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);


        using var scope = _webFactory.Services.CreateScope();
        var stillExists = await DbContext(scope).Locations
            .AnyAsync(l => l.Id == locationId);

        Assert.False(stillExists);
        
        //Строка в БД есть и помечена
        var marksAsDeletedEntity =
            await DbContext(scope).Locations.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == locationId);


        Assert.NotNull(marksAsDeletedEntity);

        Assert.True(marksAsDeletedEntity.IsDeleted);

        Assert.NotNull(marksAsDeletedEntity.DeletedAt);

        //чтение наружу не видно
        var responseMarksAsDeletedEntity = await _httpClient.GetAsync(
            new Uri($"/locations/{locationId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, responseMarksAsDeletedEntity.StatusCode);
    }

    [Fact]
    public async Task Delete_location_with_unknown_id_returns_404()
    {
        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/locations/{Guid.NewGuid()}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- TOP ----------

    [Fact]
    public async Task Get_top_locations_orders_by_department_count()
    {
        // Arrange
        var popular = await CreateLocationAsync("Популярный офис");
        var quiet = await CreateLocationAsync("Тихий офис");

        await CreateDepartmentAsync("Продажи", "prodazhi", popular);
        await CreateDepartmentAsync("Склад", "sklad", popular);

        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/locations/top", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<DataResponse<TopLocationsResponse[]>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Data.Length);

        Assert.Equal(popular, body.Data[0].Id);
        Assert.Equal(2, body.Data[0].DepartmentCount);

        Assert.Equal(quiet, body.Data[1].Id);
        Assert.Equal(0, body.Data[1].DepartmentCount);
    }
}