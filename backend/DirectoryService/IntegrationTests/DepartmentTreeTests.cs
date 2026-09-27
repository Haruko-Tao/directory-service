

using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.Departments;

namespace IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class DepartmentTreeTests : IAsyncLifetime
{

    private sealed record CreatedResponse(Guid Data);

    private sealed record ErrorItem(string Code, string Message, string Type);

    private sealed record ErrorResponse(ErrorItem[] Errors);

    private readonly HttpClient _httpClient;

    private readonly IntegrationTestWebFactory _webFactory;
    
    private sealed record DataResponse<T>(T Data);

    public DepartmentTreeTests(IntegrationTestWebFactory webFactory)
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
    public async Task Get_tree_when_db_is_empty_returns_200_and_empty_list()
    {
        var response = await _httpClient.GetAsync(new Uri("departments/tree", UriKind.Relative));
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>();
        
        Assert.NotNull(body);
        Assert.Empty(body.Data);
    }

    [Fact]
    public async Task Search_tree_with_too_short_q_returns_400()
    {
        var response = await _httpClient.GetAsync(new Uri("departments/tree/search?q=a", UriKind.Relative));
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
    }

    [Fact]
    public async Task Get_children_of_node_without_children_returns_200_and_empty_list()
    {
        var rootId = await CreateDepartmentAsync("Головной", "golovnoy");

        var response = await _httpClient.GetAsync(new Uri($"departments/{rootId}/children", UriKind.Relative));
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>();
        
        Assert.NotNull(body);
        Assert.Empty(body.Data);
        
    }

    [Fact]
    public async Task Get_children_of_unknown_id_returns_404()
    {
        var response = await _httpClient.GetAsync(new Uri($"departments/{Guid.NewGuid()}/children", UriKind.Relative));
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ancestors_of_unknown_id_returns_404()
    {
        var response = await _httpClient.GetAsync(new Uri($"departments/{Guid.NewGuid()}/ancestors", UriKind.Relative));
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ancestors_of_root_returns_200_and_empty_list()
    {
        var rootId = await CreateDepartmentAsync("Головной", "golovnoy");

        await CreateDepartmentAsync("Продажи", "prodazhi", rootId);

        var response = await _httpClient.GetAsync(new Uri($"departments/{rootId}/ancestors", UriKind.Relative));

        var body = await response.Content.ReadFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>();

        Assert.NotNull(body);
        Assert.Empty(body.Data);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Search_tree_without_matches_returns_200_and_empty_list()
    {
        await CreateDepartmentAsync("Головной", "golovnoy");

        var response = await _httpClient.GetAsync(new Uri("departments/tree/search?q=цех", UriKind.Relative));

        var body = await response.Content.ReadFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.NotNull(body);
        
        Assert.Empty(body.Data);
        
    }
}