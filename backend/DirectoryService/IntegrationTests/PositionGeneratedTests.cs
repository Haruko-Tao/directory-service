using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Positions;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

/// <summary>
/// СГЕНЕРИРОВАНО AI по образцу ручных тестов из LocationTests.
/// Подлежит ревью (пункт 7 задания). Удаляется одним файлом.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public class PositionsGeneratedTests : IAsyncLifetime
{
    private sealed record DataResponse<T>(T Data);

    private sealed record ErrorItem(string Code, string Message, string Type);

    private sealed record ErrorResponse(ErrorItem[] Errors);

    private readonly HttpClient _httpClient;
    private readonly IntegrationTestWebFactory _webFactory;

    public PositionsGeneratedTests(IntegrationTestWebFactory webFactory)
    {
        _httpClient = webFactory.HttpClient;
        _webFactory = webFactory;
    }

    public async Task InitializeAsync() => await _webFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------- подготовка данных ----------

    private async Task<Guid> CreatePositionAsync(string name)
    {
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/positions", UriKind.Relative), new CreatePositionRequest(name));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<Guid>>();
        Assert.NotNull(body);

        return body.Data;
    }

    private async Task<Guid> CreateDepartmentAsync(string name, string slug)
    {
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/departments", UriKind.Relative),
            new CreateDepartmentRequest(name, slug, null, []));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<Guid>>();
        Assert.NotNull(body);

        return body.Data;
    }

    private async Task LinkPositionAsync(Guid departmentId, Guid positionId)
    {
        var response = await _httpClient.PostAsync(
            new Uri($"/departments/{departmentId}/positions/{positionId}", UriKind.Relative),
            content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static AppDbContext DbContext(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // ---------- CREATE ----------

    [Fact]
    public async Task Create_position_returns_201_and_saves_row()
    {
        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/positions", UriKind.Relative), new CreatePositionRequest("Менеджер"));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<Guid>>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Data);

        using var scope = _webFactory.Services.CreateScope();
        var saved = await DbContext(scope).Positions.FirstOrDefaultAsync(p => p.Id == body.Data);

        Assert.NotNull(saved);
        Assert.Equal("Менеджер", saved.Name.Value);
    }

    [Fact]
    public async Task Create_position_with_empty_name_returns_400()
    {
        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/positions", UriKind.Relative), new CreatePositionRequest(string.Empty));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "name.not.space");

        using var scope = _webFactory.Services.CreateScope();
        Assert.False(await DbContext(scope).Positions.AnyAsync());
    }

    [Fact]
    public async Task Create_position_with_taken_name_returns_409()
    {
        // Arrange
        await CreatePositionAsync("Менеджер");

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/positions", UriKind.Relative), new CreatePositionRequest("Менеджер"));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "name.position.taken");

        using var scope = _webFactory.Services.CreateScope();
        Assert.Equal(1, await DbContext(scope).Positions.CountAsync());
    }

    // ---------- UPDATE ----------

    [Fact]
    public async Task Update_position_changes_name()
    {
        // Arrange
        var positionId = await CreatePositionAsync("Старое имя");

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/positions/{positionId}", UriKind.Relative),
            new UpdatePositionRequest("Новое имя"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        var saved = await DbContext(scope).Positions.FirstOrDefaultAsync(p => p.Id == positionId);

        Assert.NotNull(saved);
        Assert.Equal("Новое имя", saved.Name.Value);
    }

    [Fact]
    public async Task Update_position_to_its_own_name_returns_200()
    {
        // Arrange — переименование в то же самое имя не должно считаться конфликтом
        var positionId = await CreatePositionAsync("Менеджер");

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/positions/{positionId}", UriKind.Relative),
            new UpdatePositionRequest("Менеджер"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_position_to_taken_name_returns_409()
    {
        // Arrange
        await CreatePositionAsync("Менеджер");
        var secondId = await CreatePositionAsync("Кладовщик");

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/positions/{secondId}", UriKind.Relative),
            new UpdatePositionRequest("Менеджер"));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "name.taken");

        using var scope = _webFactory.Services.CreateScope();
        var saved = await DbContext(scope).Positions.FirstOrDefaultAsync(p => p.Id == secondId);

        Assert.NotNull(saved);
        Assert.Equal("Кладовщик", saved.Name.Value);
    }

    [Fact]
    public async Task Update_position_with_unknown_id_returns_404()
    {
        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/positions/{Guid.NewGuid()}", UriKind.Relative),
            new UpdatePositionRequest("Новое имя"));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_position_with_empty_name_returns_400_and_keeps_old_value()
    {
        // Arrange
        var positionId = await CreatePositionAsync("Старое имя");

        // Act
        var response = await _httpClient.PatchAsJsonAsync(
            new Uri($"/positions/{positionId}", UriKind.Relative),
            new UpdatePositionRequest(string.Empty));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        var saved = await DbContext(scope).Positions.FirstOrDefaultAsync(p => p.Id == positionId);

        Assert.NotNull(saved);
        Assert.Equal("Старое имя", saved.Name.Value);
    }

    // ---------- DELETE ----------

    [Fact]
    public async Task Delete_position_without_links_removes_row()
    {
        // Arrange
        var positionId = await CreatePositionAsync("На снос");

        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/positions/{positionId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        Assert.False(await DbContext(scope).Positions.AnyAsync(p => p.Id == positionId));
    }

    [Fact]
    public async Task Delete_position_with_department_returns_409()
    {
        // Arrange
        var positionId = await CreatePositionAsync("Менеджер");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi");
        await LinkPositionAsync(departmentId, positionId);

        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/positions/{positionId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "position.in.use");

        using var scope = _webFactory.Services.CreateScope();
        Assert.True(await DbContext(scope).Positions.AnyAsync(p => p.Id == positionId));
        Assert.True(await DbContext(scope).DepartmentPositions
            .AnyAsync(dp => dp.PositionId == positionId));
    }

    [Fact]
    public async Task Delete_position_with_unknown_id_returns_404()
    {
        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/positions/{Guid.NewGuid()}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- СВЯЗИ С ПОДРАЗДЕЛЕНИЯМИ ----------

    [Fact]
    public async Task Add_position_to_department_creates_link()
    {
        // Arrange
        var positionId = await CreatePositionAsync("Менеджер");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi");

        // Act
        var response = await _httpClient.PostAsync(
            new Uri($"/departments/{departmentId}/positions/{positionId}", UriKind.Relative),
            content: null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        Assert.True(await DbContext(scope).DepartmentPositions
            .AnyAsync(dp => dp.DepartmentId == departmentId && dp.PositionId == positionId));
    }

    [Fact]
    public async Task Add_same_position_twice_returns_409()
    {
        // Arrange
        var positionId = await CreatePositionAsync("Менеджер");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi");
        await LinkPositionAsync(departmentId, positionId);

        // Act
        var response = await _httpClient.PostAsync(
            new Uri($"/departments/{departmentId}/positions/{positionId}", UriKind.Relative),
            content: null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "department.position.already_exists");

        using var scope = _webFactory.Services.CreateScope();
        Assert.Equal(1, await DbContext(scope).DepartmentPositions
            .CountAsync(dp => dp.DepartmentId == departmentId));
    }

    [Fact]
    public async Task Add_unknown_position_to_department_returns_404()
    {
        // Arrange
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi");

        // Act
        var response = await _httpClient.PostAsync(
            new Uri($"/departments/{departmentId}/positions/{Guid.NewGuid()}", UriKind.Relative),
            content: null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == "position.not.found");
    }

    [Fact]
    public async Task Remove_position_from_department_deletes_link()
    {
        // Arrange
        var positionId = await CreatePositionAsync("Менеджер");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi");
        await LinkPositionAsync(departmentId, positionId);

        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/departments/{departmentId}/positions/{positionId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _webFactory.Services.CreateScope();
        Assert.False(await DbContext(scope).DepartmentPositions
            .AnyAsync(dp => dp.DepartmentId == departmentId && dp.PositionId == positionId));
    }

    [Fact]
    public async Task Remove_unknown_position_link_returns_404()
    {
        // Arrange — связь не создавалась
        var positionId = await CreatePositionAsync("Менеджер");
        var departmentId = await CreateDepartmentAsync("Продажи", "prodazhi");

        // Act
        var response = await _httpClient.DeleteAsync(
            new Uri($"/departments/{departmentId}/positions/{positionId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- НЕ РЕАЛИЗОВАННЫЕ ENDPOINT-Ы ----------
    // Написаны от ТРЕБОВАНИЯ, а не от текущего кода.
    // GetById сейчас всегда возвращает 404, GetAll — всегда пустой массив.
    // Снять Skip, когда endpoint-ы реализуют.

    [Fact(Skip = "GET /positions/{id} — заглушка, всегда возвращает NotFound")]
    public async Task Get_position_by_id_returns_200_and_card()
    {
        // Arrange
        var positionId = await CreatePositionAsync("Менеджер");

        // Act
        var response = await _httpClient.GetAsync(
            new Uri($"/positions/{positionId}", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<PositionResponse>>();
        Assert.NotNull(body);
        Assert.Equal(positionId, body.Data.Id);
        Assert.Equal("Менеджер", body.Data.Name);
    }

    [Fact(Skip = "GET /positions — заглушка, всегда возвращает пустой массив")]
    public async Task Get_positions_returns_created_positions()
    {
        // Arrange
        await CreatePositionAsync("Менеджер");
        await CreatePositionAsync("Кладовщик");

        // Act
        var response = await _httpClient.GetAsync(
            new Uri("/positions", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<PositionResponse[]>>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Data.Length);
        Assert.Contains(body.Data, p => p.Name == "Менеджер");
        Assert.Contains(body.Data, p => p.Name == "Кладовщик");
    }
}