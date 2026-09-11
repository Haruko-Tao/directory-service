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
/// СГЕНЕРИРОВАНО AI по образцу ручных тестов из LocationTests.
/// Подлежит ревью (пункт 7 задания). Удаляется одним файлом.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public class DepartmentsGeneratedTests : IAsyncLifetime
{
    private sealed record DataResponse<T>(T Data);

    private sealed record ErrorItem(string Code, string Message, string Type);

    private sealed record ErrorResponse(ErrorItem[] Errors);

    private readonly HttpClient _httpClient;
    private readonly IntegrationTestWebFactory _webFactory;

    public DepartmentsGeneratedTests(IntegrationTestWebFactory webFactory)
    {
        _httpClient = webFactory.HttpClient;
        _webFactory = webFactory;
    }

    public async Task InitializeAsync() => await _webFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------- подготовка данных ----------

    private async Task<Guid> CreateLocationAsync(string name)
    {
        var request = new CreateLocationRequest(name, new AddressDto("Воронеж", "Мира", "1", null));

        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/locations", UriKind.Relative), request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<Guid>>();
        Assert.NotNull(body);

        return body.Data;
    }

    private async Task<Guid> CreateDepartmentAsync(
        string name, string slug, Guid? parentId = null, params Guid[] locationIds)
    {
        var request = new CreateDepartmentRequest(name, slug, parentId, locationIds);

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
    public async Task Create_department_returns_201_and_saves_row()
    {
        // Arrange
        var request = new CreateDepartmentRequest("Продажи", "prodazhi", null, []);

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/departments", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<Guid>>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Data);

        using var scope = _webFactory.Services.CreateScope();
        var saved = await DbContext(scope).Departments
            .FirstOrDefaultAsync(d => d.Id == body.Data);

        Assert.NotNull(saved);
        Assert.Equal("Продажи", saved.Name.Value);
        Assert.Equal("prodazhi", saved.Slug.Value);
        Assert.Equal("prodazhi", saved.Path.Value);
        Assert.Null(saved.ParentId);
    }

    [Fact]
    public async Task Create_department_with_location_creates_link()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на Мира");
        var request = new CreateDepartmentRequest("Продажи", "prodazhi", null, [locationId]);

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/departments", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<Guid>>();
        Assert.NotNull(body);

        using var scope = _webFactory.Services.CreateScope();
        var linkExists = await DbContext(scope).DepartmentLocations
            .AnyAsync(dl => dl.DepartmentId == body.Data && dl.LocationId == locationId);

        Assert.True(linkExists);
    }

    [Fact]
    public async Task Create_child_department_builds_path_from_parent()
    {
        // Arrange
        var parentId = await CreateDepartmentAsync("Головной", "golovnoy");
        var request = new CreateDepartmentRequest("Продажи", "prodazhi", parentId, []);

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/departments", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<Guid>>();
        Assert.NotNull(body);

        using var scope = _webFactory.Services.CreateScope();
        var child = await DbContext(scope).Departments
            .FirstOrDefaultAsync(d => d.Id == body.Data);

        Assert.NotNull(child);
        Assert.Equal("golovnoy/prodazhi", child.Path.Value);
        Assert.Equal(parentId, child.ParentId);
    }

    [Fact]
    public async Task Create_department_with_empty_name_returns_400()
    {
        // Arrange
        var request = new CreateDepartmentRequest(string.Empty, "prodazhi", null, []);

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/departments", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "name.not.space");
    }

    [Fact]
    public async Task Create_department_with_cyrillic_slug_returns_400()
    {
        // Arrange — slug допускает только строчную латиницу, цифры и дефис
        var request = new CreateDepartmentRequest("Продажи", "продажи", null, []);

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/departments", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "slug.regex");
    }

    [Fact]
    public async Task Create_department_with_taken_slug_returns_409()
    {
        // Arrange
        await CreateDepartmentAsync("Продажи", "prodazhi");
        var duplicate = new CreateDepartmentRequest("Другое имя", "prodazhi", null, []);

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/departments", UriKind.Relative), duplicate);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "is.slug.taken");

        using var scope = _webFactory.Services.CreateScope();
        Assert.Equal(1, await DbContext(scope).Departments.CountAsync());
    }

    [Fact]
    public async Task Create_department_with_unknown_location_returns_404()
    {
        // Arrange
        var request = new CreateDepartmentRequest("Продажи", "prodazhi", null, [Guid.NewGuid()]);

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/departments", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "location.not.found");

        using var scope = _webFactory.Services.CreateScope();
        Assert.False(await DbContext(scope).Departments.AnyAsync());
    }

    // ---------- GET BY ID ----------

    [Fact]
    public async Task Get_department_by_id_returns_200_and_card()
    {
        // Arrange
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi");

        // Act
        var response = await _httpClient.GetAsync(
            new Uri($"/departments/{departmentId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<DepartmentResponse>>();
        Assert.NotNull(body);
        Assert.Equal(departmentId, body.Data.Id);
        Assert.Equal("Продажи", body.Data.Name);
        Assert.Equal("prodazhi", body.Data.Slug);
        Assert.Equal("prodazhi", body.Data.Path);
        Assert.Null(body.Data.ParentId);
    }

    [Fact]
    public async Task Get_department_by_unknown_id_returns_404()
    {
        // Act
        var response = await _httpClient.GetAsync(
            new Uri($"/departments/{Guid.NewGuid()}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "department.not.found");
    }

    // ---------- LIST ----------

    [Fact]
    public async Task Get_departments_returns_200_and_all_rows()
    {
        // Arrange
        await CreateDepartmentAsync("Альфа", "alfa");
        await CreateDepartmentAsync("Бета", "beta");

        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/departments", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<DataResponse<PagedResult<DepartmentListItemDto>>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Data.TotalCount);
        Assert.Contains(body.Data.Items, i => i.Slug == "alfa");
        Assert.Contains(body.Data.Items, i => i.Slug == "beta");
    }

    [Fact]
    public async Task Get_departments_second_page_is_not_empty()
    {
        // Arrange
        await CreateDepartmentAsync("Альфа", "alfa");
        await CreateDepartmentAsync("Бета", "beta");

        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/departments?page=2&pageSize=1&sortBy=NAME&sortDir=ASC", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<DataResponse<PagedResult<DepartmentListItemDto>>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Data.TotalCount);
        Assert.Equal(2, body.Data.Page);
        Assert.Equal("Бета", body.Data.Items.Single().Name);
    }

    [Fact]
    public async Task Get_departments_with_unknown_sort_by_returns_400()
    {
        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/departments?sortBy=bogus", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "sort.by.invalid");
    }

    [Fact]
    public async Task Get_departments_with_too_large_page_size_returns_400()
    {
        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/departments?pageSize=1000", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "page.size.invalid");
    }

    // ---------- UPDATE ----------

    [Fact]
    public async Task Update_department_changes_name()
    {
        // Arrange
        var departmentId = await CreateDepartmentAsync("Старое имя", "staroe");
        var request = new UpdateDepartmentRequest("Новое имя");

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/departments/{departmentId}", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        var saved = await DbContext(scope).Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        Assert.NotNull(saved);
        Assert.Equal("Новое имя", saved.Name.Value);
        Assert.Equal("staroe", saved.Slug.Value);
    }

    [Fact]
    public async Task Update_department_with_unknown_id_returns_404()
    {
        // Arrange
        var request = new UpdateDepartmentRequest("Новое имя");

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/departments/{Guid.NewGuid()}", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_department_with_empty_name_returns_400_and_keeps_old_value()
    {
        // Arrange
        var departmentId = await CreateDepartmentAsync("Старое имя", "staroe");
        var request = new UpdateDepartmentRequest(string.Empty);

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/departments/{departmentId}", UriKind.Relative), request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        var saved = await DbContext(scope).Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        Assert.NotNull(saved);
        Assert.Equal("Старое имя", saved.Name.Value);
    }

    // ---------- DELETE ----------

    [Fact]
    public async Task Delete_department_without_links_removes_row()
    {
        // Arrange
        var departmentId = await CreateDepartmentAsync("На снос", "na-snos");

        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/departments/{departmentId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        Assert.False(await DbContext(scope).Departments.AnyAsync(d => d.Id == departmentId));
    }

    [Fact]
    public async Task Delete_department_with_location_returns_409()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на Мира");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi", null, locationId);

        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/departments/{departmentId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "department.in.use");

        using var scope = _webFactory.Services.CreateScope();
        Assert.True(await DbContext(scope).Departments.AnyAsync(d => d.Id == departmentId));
        Assert.True(await DbContext(scope).DepartmentLocations.AnyAsync(dl => dl.DepartmentId == departmentId));
    }

    [Fact]
    public async Task Delete_department_with_children_returns_409()
    {
        // Arrange
        var parentId = await CreateDepartmentAsync("Головной", "golovnoy");
        await CreateDepartmentAsync("Продажи", "prodazhi", parentId);

        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/departments/{parentId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "department.in.use");

        using var scope = _webFactory.Services.CreateScope();
        Assert.Equal(2, await DbContext(scope).Departments.CountAsync());
    }

    [Fact]
    public async Task Delete_department_with_unknown_id_returns_404()
    {
        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/departments/{Guid.NewGuid()}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- СВЯЗИ С ЛОКАЦИЯМИ ----------

    [Fact]
    public async Task Add_location_to_department_creates_link()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на Мира");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi");

        // Act
        var response = await _httpClient.PostAsync(
            new Uri($"/departments/{departmentId}/locations/{locationId}", UriKind.Relative),
            content: null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        Assert.True(await DbContext(scope).DepartmentLocations
            .AnyAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId));
    }

    [Fact]
    public async Task Add_same_location_twice_returns_409()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на Мира");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi", null, locationId);

        // Act
        var response = await _httpClient.PostAsync(
            new Uri($"/departments/{departmentId}/locations/{locationId}", UriKind.Relative),
            content: null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "department.location.already_exists");

        using var scope = _webFactory.Services.CreateScope();
        Assert.Equal(1, await DbContext(scope).DepartmentLocations
            .CountAsync(dl => dl.DepartmentId == departmentId));
    }

    [Fact]
    public async Task Add_location_to_unknown_department_returns_404()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на Мира");

        // Act
        var response = await _httpClient.PostAsync(
            new Uri($"/departments/{Guid.NewGuid()}/locations/{locationId}", UriKind.Relative),
            content: null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "department.not.found");
    }

    [Fact]
    public async Task Remove_location_from_department_deletes_link()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на Мира");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi", null, locationId);

        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/departments/{departmentId}/locations/{locationId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        Assert.False(await DbContext(scope).DepartmentLocations
            .AnyAsync(dl => dl.DepartmentId == departmentId && dl.LocationId == locationId));
    }

    [Fact]
    public async Task Remove_unknown_link_returns_404()
    {
        // Arrange
        var locationId = await CreateLocationAsync("Офис на Мира");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi");

        // Act — связь между ними не создавалась
        var response = await _httpClient.DeleteAsync(
            new Uri($"/departments/{departmentId}/locations/{locationId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}