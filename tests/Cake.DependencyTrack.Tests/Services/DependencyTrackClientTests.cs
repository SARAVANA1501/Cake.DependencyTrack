using System.Net;
using Cake.DependencyTrack.Models;
using Cake.DependencyTrack.Services;
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

    [Fact]
    public async Task TestGetProjectDetails()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var projectId = "5075a155-8779-4074-b46a-2447bb81ca7e";
        string projectName = "test";
        string version = "CI";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                $@"{{ ""uuid"": ""{projectId}"",""name"": ""{projectName}"",""version"": ""{version}"" }}"),
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

        Project projectDetails = await dependencyTrackClient.GetProjectDetails(projectName, version);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get
                && req.RequestUri.ToString() == "https://dependencytrack.org/api/v1/project/lookup?name=test&version=CI"
                && req.Headers.Contains("X-Api-Key") && req.Headers.GetValues("X-Api-Key").First() == "test-key"
                && req.Headers.Contains("accept")),
            ItExpr.IsAny<CancellationToken>());

        Assert.Equal(projectId, projectDetails.Uuid.ToString());
        Assert.Equal(projectName, projectDetails.Name);
        Assert.Equal(version, projectDetails.Version);
    }

    [Fact]
    public async Task TestGetServerVersion()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var appId = "5075a155-8779-4074-b46a-2447bb81ca7e";
        string name = "Dependency-Track";
        string expectedVersion = "4.8.2";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                $@"{{ ""uuid"": ""{appId}"",""application"": ""{name}"",""version"": ""{expectedVersion}"" }}"),
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

        AppVersion appVersion = await dependencyTrackClient.GetServerVersion();

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get
                && req.RequestUri.ToString() == "https://dependencytrack.org/api/version"
                && req.Headers.Contains("accept")),
            ItExpr.IsAny<CancellationToken>());

        Assert.Equal(appId, appVersion.Uuid.ToString());
        Assert.Equal(name, appVersion.Application);
        Assert.Equal(expectedVersion, appVersion.Version);
    }

    [Fact]
    public async Task TestUploadBomFileWhenAutoCreationEnabled()
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

        string projectName = "test_proj";
        string projectVersion = "test_ver";
        string bomFile = "test_bom";
        var taskId = await dependencyTrackClient.UploadBomAsync(projectName, projectVersion, true, bomFile);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Put
                && req.RequestUri.ToString() == "https://dependencytrack.org/api/v1/bom"
                && req.Headers.Contains("X-Api-Key") && req.Headers.GetValues("X-Api-Key").First() == "test-key"
                && req.Headers.Contains("accept")
                && req.Content.Headers.Contains("Content-Type")
                && req.Content.ReadAsStringAsync().Result.Contains("projectName")
                && req.Content.ReadAsStringAsync().Result.Contains("projectVersion")
                && req.Content.ReadAsStringAsync().Result.Contains("bom")),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("5075a155-8779-4074-b46a-2447bb81ca7e", taskId);
    }
}