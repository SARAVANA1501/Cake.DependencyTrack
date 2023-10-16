using System;
using System.IO;
using System.Threading;
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
        string taskId;
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

    internal async Task EnsureProjectMetricsAreUnderThreshold(MetricsThresholdSettings metricsThresholdSettings,
        Project project, string taskId)
    {
        await WaitForTheBomProcessing(taskId);
        await WaitForTheMetricsUpdate(project);
        await ValidMetricsThresholds(metricsThresholdSettings, project);
    }

    internal async Task ValidMetricsThresholds(MetricsThresholdSettings metricsThresholdSettings, Project project)
    {
        _cakeContext.Log.Information("Starting Metrics validation");
        var metrics = await _dependencyTrackClient.GetMetrics(project.Uuid.ToString());
        var metricsAreValid = true;
        if (metrics.Critical >= metricsThresholdSettings.CriticalCount)
        {
            _cakeContext.Log.Error(
                $"Critical vulnerabilities are greater then or equal to threshold value. Threshold :{metricsThresholdSettings.CriticalCount}, Metrics : {metrics.Critical}");
            metricsAreValid = false;
        }
        if (metrics.High >= metricsThresholdSettings.HighCount)
        {
            _cakeContext.Log.Error(
                $"High vulnerabilities are greater then or equal to threshold value. Threshold :{metricsThresholdSettings.HighCount}, Metrics : {metrics.High}");
            metricsAreValid = false;
        }
        if (metrics.Medium >= metricsThresholdSettings.MediumCount)
        {
            _cakeContext.Log.Error(
                $"Medium vulnerabilities are greater then or equal to threshold value. Threshold :{metricsThresholdSettings.MediumCount}, Metrics : {metrics.Medium}");
            metricsAreValid = false;
        }
        if (metrics.Low >= metricsThresholdSettings.LowCount)
        {
            _cakeContext.Log.Error(
                $"Low vulnerabilities are greater then or equal to threshold value. Threshold :{metricsThresholdSettings.LowCount}, Metrics : {metrics.Low}");
            metricsAreValid = false;
        }

        if (!metricsAreValid) throw new CakeException("Threshold validation failure");
        _cakeContext.Log.Information("Metrics validation completed. All the vulnerability metrics are under threshold values");
    }

    internal async Task WaitForTheMetricsUpdate(Project project)
    {
        _cakeContext.Log.Information("Waiting for the Metrics updates to complete.");
        var latestProject = await _dependencyTrackClient.GetProjectDetails(project.Uuid.ToString());
        var metrics = await _dependencyTrackClient.GetMetrics(project.Uuid.ToString());
        while (metrics.LastOccurrence < latestProject.LastBomImport)
        {
            Thread.Sleep(10000);
            _cakeContext.Log.Information("Pooling server to check the status.");
            metrics = await _dependencyTrackClient.GetMetrics(project.Uuid.ToString());
        }

        _cakeContext.Log.Information("Project metrics updates completed");
    }

    internal async Task WaitForTheBomProcessing(string taskId)
    {
        _cakeContext.Log.Information("Waiting for the BOM processing to complete.");
        while (true)
        {
            Thread.Sleep(10000);
            _cakeContext.Log.Information("Pooling server to check the status.");
            var result = await _dependencyTrackClient.GetBomProcessingStatus(taskId);
            if (result.Processing)
            {
                _cakeContext.Log.Information("BOM analysis is not completed. waiting...");
            }
            else
            {
                _cakeContext.Log.Information("BOM analysis is completed.");
                break;
            }
        }
    }

    private async Task CheckServerAvailability()
    {
        try
        {
            _cakeContext.Log.Information("Checking server information");
            var version = await _dependencyTrackClient.GetServerVersion();
            if (version == null)
            {
                throw new CakeException("Unable to find server");
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
            _cakeContext.Log.Error($"Error: {e}");
            throw;
        }
    }
}