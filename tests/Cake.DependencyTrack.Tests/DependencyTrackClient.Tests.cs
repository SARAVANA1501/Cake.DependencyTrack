using System.Net;
using Moq;
using Moq.Protected;

namespace Cake.DependencyTrack.Tests;

public class DependencyTrackClientTests
{
    [Fact]
    public async Task TestUploadBomFile()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var token = "5075a155-8779-4074-b46a-2447bb81ca7e";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent($@"{{ ""token"": ""{token}"" }}"),
        };
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var baseUrl = "https://dependencytrack.org";
        var apikey = "test-key";
        var dependencyTrackClient = new DependencyTrackClient(new HttpClient(handlerMock.Object), baseUrl, apikey);

        string projectId = "test_proj";
        string bomFile = "test_bom";
        var taskId = await dependencyTrackClient.UploadBomAsync(projectId, bomFile);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Put
                && req.RequestUri.ToString() == "https://dependencytrack.org/api/v1/bom"
                && req.Headers.Contains("X-Api-Key") && req.Headers.GetValues("X-Api-Key").First() == "test-key"
                && req.Headers.Contains("accept")
                && req.Content.Headers.Contains("Content-Type")
                && req.Content.ReadAsStringAsync().Result.Contains("project") 
                && req.Content.ReadAsStringAsync().Result.Contains("bom")),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("5075a155-8779-4074-b46a-2447bb81ca7e", taskId);
    }
}