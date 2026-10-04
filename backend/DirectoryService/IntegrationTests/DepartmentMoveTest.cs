using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.Departments;

namespace IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class DepartmentMoveTest : IAsyncLifetime
{
    private sealed record CreatedResponse(Guid Data);

    private sealed record ErrorItem(string Code, string Message, string Type);

    private sealed record ErrorResponse(ErrorItem[] Errors);

    private readonly HttpClient _httpClient;

    private readonly IntegrationTestWebFactory _webFactory;
    
    private sealed record DataResponse<T>(T Data);

    public DepartmentMoveTest(IntegrationTestWebFactory webFactory)
    {
        _httpClient = webFactory.HttpClient;
        _webFactory = webFactory;
    }

    public async Task InitializeAsync()
    {
        await _webFactory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
    
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

    [Fact]
    public async Task Move_with_children_returns_200_and_updates_subtree()
    {
        var hqId = await CreateDepartmentAsync("Головной офис", "hq");
        var itId = await CreateDepartmentAsync("ИТ", "it", hqId);
        var devId = await CreateDepartmentAsync("Разработка", "dev", itId);
        var salesId = await CreateDepartmentAsync("Продажи", "sales", hqId);

        var response = await _httpClient.PutAsJsonAsync(
            new Uri($"/departments/{itId}/parent", UriKind.Relative), new MoveDepartmentRequest(salesId));
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<MoveDepartmentResponse>>();

        Assert.NotNull(body);
        Assert.Equal("hq.sales.it", body.Data.Path);
        Assert.Equal(2, body.Data.Depth);

        var responseGetChildren = await _httpClient.GetAsync(new Uri($"departments/{itId}/children", UriKind.Relative));
        
        Assert.Equal(HttpStatusCode.OK, responseGetChildren.StatusCode);

        var bodyGetChildren =
            await responseGetChildren.Content.ReadFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>();

        Assert.NotNull(bodyGetChildren);

        var dev = Assert.Single(bodyGetChildren.Data);
        
        Assert.Equal(devId, dev.Id);
        
        Assert.Equal("hq.sales.it.dev", dev.Path);
        Assert.Equal(3, dev.Depth);
        
        Assert.Equal(salesId, body.Data.ParentId);

    }

    // ===================== TODO(разобрать): ниже тесты написаны ментором (DS-23) =====================

    private Task<HttpResponseMessage> MoveAsync(Guid departmentId, Guid? parentId) =>
        _httpClient.PutAsJsonAsync(
            new Uri($"/departments/{departmentId}/parent", UriKind.Relative),
            new MoveDepartmentRequest(parentId));

    private static async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, e => e.Code == code);
    }

    [Fact]
    public async Task Move_to_root_returns_200_and_depth_0_for_node_and_children_recalculated()
    {
        var hqId = await CreateDepartmentAsync("Головной офис", "hq");
        var itId = await CreateDepartmentAsync("ИТ", "it", hqId);
        var devId = await CreateDepartmentAsync("Разработка", "dev", itId);

        var response = await MoveAsync(itId, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DataResponse<MoveDepartmentResponse>>();
        Assert.NotNull(body);
        Assert.Null(body.Data.ParentId);
        Assert.Equal("it", body.Data.Path);
        Assert.Equal(0, body.Data.Depth);

        var children = await _httpClient.GetFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>(
            new Uri($"departments/{itId}/children", UriKind.Relative));
        Assert.NotNull(children);
        var dev = Assert.Single(children.Data);
        Assert.Equal(devId, dev.Id);
        Assert.Equal("it.dev", dev.Path);
        Assert.Equal(1, dev.Depth);
    }

    [Fact]
    public async Task Move_under_own_descendant_returns_409_cycle()
    {
        var hqId = await CreateDepartmentAsync("Головной офис", "hq");
        var itId = await CreateDepartmentAsync("ИТ", "it", hqId);
        var devId = await CreateDepartmentAsync("Разработка", "dev", itId);

        var response = await MoveAsync(itId, devId);

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "department.move.cycle");
    }

    [Fact]
    public async Task Move_under_itself_returns_400_parent_is_self()
    {
        var hqId = await CreateDepartmentAsync("Головной офис", "hq");

        var response = await MoveAsync(hqId, hqId);

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "department.move.parent_is_self");
    }

    [Fact]
    public async Task Move_under_soft_deleted_parent_returns_409_parent_deleted()
    {
        var hqId = await CreateDepartmentAsync("Головной офис", "hq");
        var itId = await CreateDepartmentAsync("ИТ", "it", hqId);
        var archiveId = await CreateDepartmentAsync("Архив", "archive", hqId);

        var deleteResponse = await _httpClient.DeleteAsync(new Uri($"/departments/{archiveId}", UriKind.Relative));
        Assert.True(deleteResponse.IsSuccessStatusCode, $"soft delete failed: {deleteResponse.StatusCode}");

        var response = await MoveAsync(itId, archiveId);

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "department.move.parent_deleted");
    }

    [Fact]
    public async Task Move_under_unknown_parent_returns_404_parent_not_found()
    {
        var hqId = await CreateDepartmentAsync("Головной офис", "hq");

        var response = await MoveAsync(hqId, Guid.NewGuid());

        await AssertErrorAsync(response, HttpStatusCode.NotFound, "department.parent.not_found");
    }

    [Fact]
    public async Task Move_unknown_department_returns_404_not_found()
    {
        var hqId = await CreateDepartmentAsync("Головной офис", "hq");

        var response = await MoveAsync(Guid.NewGuid(), hqId);

        await AssertErrorAsync(response, HttpStatusCode.NotFound, "department.not_found");
    }

    [Fact]
    public async Task Move_to_same_parent_is_noop_and_does_not_change_updated_at()
    {
        var hqId = await CreateDepartmentAsync("Головной офис", "hq");
        var itId = await CreateDepartmentAsync("ИТ", "it", hqId);
        var salesId = await CreateDepartmentAsync("Продажи", "sales", hqId);

        var first = await MoveAsync(itId, salesId);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstBody = await first.Content.ReadFromJsonAsync<DataResponse<MoveDepartmentResponse>>();
        Assert.NotNull(firstBody);

        _webFactory.UpdateCounter.Reset();

        var second = await MoveAsync(itId, salesId);

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondBody = await second.Content.ReadFromJsonAsync<DataResponse<MoveDepartmentResponse>>();
        Assert.NotNull(secondBody);
        Assert.Equal(firstBody.Data.UpdatedAt, secondBody.Data.UpdatedAt);
        Assert.Equal("hq.sales.it", secondBody.Data.Path);
        Assert.Equal(0, _webFactory.UpdateCounter.Count);
    }

    [Fact]
    public async Task Move_big_subtree_uses_constant_number_of_update_commands()
    {
        const int childrenCount = 50;

        var hqId = await CreateDepartmentAsync("Головной офис", "hq");
        var itId = await CreateDepartmentAsync("ИТ", "it", hqId);
        for (var i = 0; i < childrenCount; i++)
            await CreateDepartmentAsync($"Команда {i}", $"team-{i}", itId);
        var salesId = await CreateDepartmentAsync("Продажи", "sales", hqId);

        _webFactory.UpdateCounter.Reset();

        var response = await MoveAsync(itId, salesId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // 1 UPDATE от SaveChanges (parent_id + updated_at у самого узла)
        // + 1 bulk UPDATE поддерева. Не зависит от childrenCount.
        Assert.Equal(2, _webFactory.UpdateCounter.Count);

        var children = await _httpClient.GetFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>(
            new Uri($"departments/{itId}/children?pageSize=100", UriKind.Relative));
        Assert.NotNull(children);
        Assert.All(children.Data, c =>
        {
            Assert.StartsWith("hq.sales.it.", c.Path, StringComparison.Ordinal);
            Assert.Equal(3, c.Depth);
        });
    }
}