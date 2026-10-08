using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.Departments;

namespace IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class DepartmentMoveConcurrencyTests : IAsyncLifetime
{
    private sealed record CreatedResponse(Guid Data);

     private sealed record ErrorItem(string Code, string Message, string Type);

     private sealed record ErrorResponse(ErrorItem[] Errors);

     private readonly HttpClient _httpClient;

     private readonly IntegrationTestWebFactory _webFactory;
    
     private sealed record DataResponse<T>(T Data);

     public DepartmentMoveConcurrencyTests(IntegrationTestWebFactory webFactory)
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
     public async Task Concurrent_cross_move_one_succeeds_other_returns_409_cycle()
     {
         //Arrange 
         var hqId = await CreateDepartmentAsync("HQ", "hq");
         var aId = await CreateDepartmentAsync("HQ.A", "a", hqId);
         var bId = await CreateDepartmentAsync("HQ.B", "b", hqId);
         
         //Act
         var responseA = MoveAsync(aId, bId);
         var responseB = MoveAsync(bId, aId);

         var results = await Task.WhenAll(responseA, responseB);
         
         //Assert's
         
         var okResponse = Assert.Single(results, r => r.StatusCode == HttpStatusCode.OK);
         var conflictResponse = Assert.Single(results, r => r.StatusCode == HttpStatusCode.Conflict);

         await AssertErrorAsync(conflictResponse, HttpStatusCode.Conflict, "department.move.cycle");

         var children = await _httpClient.GetFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>(
             new Uri($"/departments/{hqId}/children", UriKind.Relative));
         
         var body = await okResponse.Content.ReadFromJsonAsync<DataResponse<MoveDepartmentResponse>>();
         
         Assert.NotNull(body);
         Assert.NotNull(children);
         var remaining = Assert.Single(children.Data);
         Assert.Equal(1, remaining.Depth);
         Assert.NotEqual(body.Data.Id, remaining.Id);
     }

     [Fact]
     public async Task Concurrent_move_same_node_to_different_parents_keeps_tree_consistent()
     {
         //Arrange
         var hqId = await CreateDepartmentAsync("HQ", "hq");
         var aId = await CreateDepartmentAsync("HQ.A", "a", hqId);
         var teamId = await CreateDepartmentAsync("HQ.A.TEAM", "team", aId);
         var xId = await CreateDepartmentAsync("HQ.X", "x", hqId);
         var yId = await CreateDepartmentAsync("HQ.Y", "y", hqId);
         
         //Act

         var responseA = MoveAsync(aId, xId);
         var responseB = MoveAsync(aId, yId);
         
         var results = await Task.WhenAll(responseA, responseB);
         
         //Assert's
         
         Assert.Contains(results, r =>  r.StatusCode is HttpStatusCode.OK);
         Assert.All(results, r => Assert.True(r.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.OK));
         
         var childrenX = await _httpClient.GetFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>(
             new Uri($"/departments/{xId}/children", UriKind.Relative));
         
         var childrenY = await _httpClient.GetFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>(
             new Uri($"/departments/{yId}/children", UriKind.Relative));

         Assert.NotNull(childrenX);
         Assert.NotNull(childrenY);
         
         var all = childrenX.Data.Concat(childrenY.Data).ToList();
         
         var result = Assert.Single(all, a => a.Id == aId);

         var isUnderX = childrenX.Data.Any(a => a.Id == aId);
         
         var expectedPath = isUnderX ? "hq.x.a" : "hq.y.a";
         
         Assert.Equal(expectedPath, result.Path);
         Assert.Equal(2, result.Depth);
        
         var childrenA = await _httpClient.GetFromJsonAsync<DataResponse<List<DepartmentTreeNodeDto>>>(
             new Uri($"/departments/{aId}/children", UriKind.Relative));
         
         Assert.NotNull(childrenA);
         var resultChildrenA = Assert.Single(childrenA.Data, a => a.Id == teamId);
         
         Assert.Equal(expectedPath + ".team", resultChildrenA.Path);
         Assert.Equal(3, resultChildrenA.Depth);
     }
     
}