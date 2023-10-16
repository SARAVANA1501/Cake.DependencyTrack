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
        mockDtrackClient.Setup(t => t.GetProjectDetails(It.IsAny<string>()))
            .ReturnsAsync(new Project()
            {
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
        mockDtrackClient.Setup(t => t.GetProjectDetails(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Project()
            {
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

    [Fact]
    public async Task TestWaitForTheBomProcessing()
    {
        var mockDtrackClient = new Mock<IDependencyTrackClient>();
        mockDtrackClient.SetupSequence(t => t.GetBomProcessingStatus(It.IsAny<string>()))
            .ReturnsAsync(new BomStatus()
            {
                Processing = true
            })
            .ReturnsAsync(new BomStatus()
            {
                Processing = false
            });
        var mockCakeContext = new Mock<ICakeContext>();
        var context = new DependencyTrackContext(mockDtrackClient.Object, mockCakeContext.Object);

        await context.WaitForTheBomProcessing("");

        mockDtrackClient.Verify(t => t.GetBomProcessingStatus(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task TestWaitForTheMetricsUpdate()
    {
        var mockDtrackClient = new Mock<IDependencyTrackClient>();
        mockDtrackClient.SetupSequence(t => t.GetMetrics(It.IsAny<string>()))
            .ReturnsAsync(new Metrics()
            {
                LastOccurrence = 10
            })
            .ReturnsAsync(new Metrics()
            {
                LastOccurrence = 50
            });
        mockDtrackClient.Setup(t => t.GetProjectDetails(It.IsAny<string>()))
            .ReturnsAsync(new Project() { LastBomImport = 15 });
        var mockCakeContext = new Mock<ICakeContext>();
        var context = new DependencyTrackContext(mockDtrackClient.Object, mockCakeContext.Object);

        await context.WaitForTheMetricsUpdate(new Project() { LastBomImport = 15 });

        mockDtrackClient.Verify(t => t.GetMetrics(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task TestEnsureProjectMetricsAreUnderGivenThresholdValues()
    {
        var mockDtrackClient = new Mock<IDependencyTrackClient>();
        mockDtrackClient.Setup(t => t.GetMetrics(It.IsAny<string>()))
            .ReturnsAsync(new Metrics()
            {
                LastOccurrence = 1,
                Critical = 1,
                High = 1,
                Medium = 1,
                Low = 2
            });

        var mockCakeContext = new Mock<ICakeContext>();
        var context = new DependencyTrackContext(mockDtrackClient.Object, mockCakeContext.Object);

        await context.ValidMetricsThresholds(
            new MetricsThresholdSettings() { CriticalCount = 5, HighCount = 2, MediumCount = 3, LowCount = 5 },
            new Project() { Uuid = new Guid() });

        mockDtrackClient.Verify(t => t.GetMetrics(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task TestFailWhenProjectMetricsAreAboveThresholdValues()
    {
        var mockDtrackClient = new Mock<IDependencyTrackClient>();
        mockDtrackClient.Setup(t => t.GetMetrics(It.IsAny<string>()))
            .ReturnsAsync(new Metrics()
            {
                LastOccurrence = 10,
                Critical = 10,
                High = 20,
                Medium = 10,
                Low = 5
            });

        var mockCakeContext = new Mock<ICakeContext>();
        var context = new DependencyTrackContext(mockDtrackClient.Object, mockCakeContext.Object);

        await Assert.ThrowsAsync<CakeException>(async () =>
            await context.ValidMetricsThresholds(
                new MetricsThresholdSettings() { CriticalCount = 5, HighCount = 10, MediumCount = 2, LowCount = 1 },
                new Project() { Uuid = new Guid() })
        );

        mockDtrackClient.Verify(t => t.GetMetrics(It.IsAny<string>()), Times.Once);
    }
}