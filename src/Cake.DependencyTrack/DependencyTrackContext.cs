using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.DependencyTrack.Models;
using Cake.DependencyTrack.Services;

namespace Cake.DependencyTrack;

internal class DependencyTrackContext
{
    private readonly IDependencyTrackClient _dependencyTrackClient;
    private readonly ICakeContext _cakeContext;

    public DependencyTrackContext(IDependencyTrackClient dependencyTrackClient, ICakeContext cakeContext)
    {
        _dependencyTrackClient = dependencyTrackClient;
        _cakeContext = cakeContext;
    }

    public async Task UploadBom(UploadBomSettings bomUploadSettings)
    {
        await CheckServerAvailability();
        _cakeContext.Log.Information("Starting bom uploading");
        var fileContent = await File.ReadAllTextAsync(bomUploadSettings.AbsoluteBomFilePath);
        string taskId = "";
        Project project;
        if (!string.IsNullOrEmpty(bomUploadSettings.ProjectId))
        {
            taskId = await _dependencyTrackClient.UploadBomAsync(bomUploadSettings.ProjectId, fileContent);
            project = await _dependencyTrackClient.GetProjectDetails(bomUploadSettings.ProjectId);
        }
        else
        {
            taskId = await _dependencyTrackClient.UploadBomAsync(bomUploadSettings.ProjectName,
                bomUploadSettings.Version, bomUploadSettings.AutoCreate, fileContent);
            project = await _dependencyTrackClient.GetProjectDetails(bomUploadSettings.ProjectName,
                bomUploadSettings.Version);
        }

        _cakeContext.Log.Information("Completed bom uploading");
        _cakeContext.Log.Information($"Project Id : {project.Uuid}");
        _cakeContext.Log.Information($"Bom uploading Task Id : {taskId}");
        if (bomUploadSettings.ShouldValidateMetrics)
        {
            _cakeContext.Log.Information("Metrics validation started.");
            await EnsureProjectMetricsAreUnderThreshold(bomUploadSettings.MetricsThresholdSettings, project, taskId);
        }
    }

    internal async Task EnsureProjectMetricsAreUnderThreshold(MetricsThresholdSettings metricsThresholdSettings, Project project, string taskId)
    {
        throw new NotImplementedException();
    }

    private async Task CheckServerAvailability()
    {
        try
        {
            _cakeContext.Log.Information("Checking server information");
            var version = await _dependencyTrackClient.GetServerVersion();
            if (version == null)
            {
                throw new Exception("Unable to find server");
            }

            _cakeContext.Log.Information("Dependency server information");
            _cakeContext.Log.Information("-------------------------------------------");
            _cakeContext.Log.Information($@"Server Name : {version.Application}");
            _cakeContext.Log.Information($@"Server version : {version.Version}");
            _cakeContext.Log.Information($@"Server Id : {version.Uuid}");
            _cakeContext.Log.Information("-------------------------------------------");
            _cakeContext.Log.Information("Checking server information completed");
        }
        catch (Exception e)
        {
            _cakeContext.Log.Error("Error occured while checking the server Availability");
            _cakeContext.Log.Error($"Error: {e.ToString()}");
            throw;
        }
    }
}