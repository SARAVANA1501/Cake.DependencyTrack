using Cake.Core;
using Cake.DependencyTrack.Models;
using Cake.DependencyTrack.Services;
using Moq;

namespace Cake.DependencyTrack.Tests;

public class DependencyTrackContextTests
{
    [Fact]
    public async Task TestBomUploadWhenProjectIsAlreadyCreated()
    {
        var mockDtrackClient = new Mock<IDependencyTrackClient>();
        mockDtrackClient.Setup(t => t.GetServerVersion())
            .ReturnsAsync(new AppVersion()
            {
                Application = "Dependency Track",
                Version = "4.8.2",
                Uuid = new Guid()
            });
        var mockCakeContext = new Mock<ICakeContext>();
        var context = new DependencyTrackContext(mockDtrackClient.Object, mockCakeContext.Object);
        var projectId = "test-id";
        var filePath = "TestData/test_bom.xml";
        string exactPath = Path.GetFullPath(filePath);
        string fileContent = File.ReadAllText(exactPath);
        var bomUploadSettings = new UploadBomSettings()
        {
            ProjectId = projectId,
            AbsoluteBomFilePath = exactPath
        };
        await context.UploadBom(bomUploadSettings);
        mockDtrackClient.Verify(t => t.UploadBomAsync(projectId, fileContent));
    }

    [Fact]
    public async Task TestFailBomUploadWhenServerIsNotAvailable()
    {
        var mockDtrackClient = new Mock<IDependencyTrackClient>();
        mockDtrackClient.Setup(t => t.GetServerVersion())
            .Throws(new HttpRequestException("Server Connection failed"));
        var mockCakeContext = new Mock<ICakeContext>();
        var context = new DependencyTrackContext(mockDtrackClient.Object, mockCakeContext.Object);
        var projectId = "test-id";
        var filePath = "TestData/test_bom.xml";
        string exactPath = Path.GetFullPath(filePath);
        var bomUploadSettings = new UploadBomSettings()
        {
            ProjectId = projectId,
            AbsoluteBomFilePath = exactPath
        };
        await Assert.ThrowsAsync<HttpRequestException>(async () => await context.UploadBom(bomUploadSettings));
    }

    [Fact]
    public async Task TestBomUploadWithAutoProjectCreation()
    {
        var mockDtrackClient = new Mock<IDependencyTrackClient>();
        mockDtrackClient.Setup(t => t.GetServerVersion())
            .ReturnsAsync(new AppVersion()
            {
                Application = "Dependency Track",
                Version = "4.8.2",
                Uuid = new Guid()
            });
        var mockCakeContext = new Mock<ICakeContext>();
        var context = new DependencyTrackContext(mockDtrackClient.Object, mockCakeContext.Object);
        var projectName = "test";
        var filePath = "TestData/test_bom.xml";
        string exactPath = Path.GetFullPath(filePath);
        string fileContent = File.ReadAllText(exactPath);
        var version = "CI";
        var bomUploadSettings = new UploadBomSettings()
        {
            ProjectName = projectName,
            Version = version,
            AutoCreate = true,
            AbsoluteBomFilePath = exactPath
        };
        await context.UploadBom(bomUploadSettings);
        mockDtrackClient.Verify(t => t.UploadBomAsync(projectName, version, true, fileContent));
    }
}