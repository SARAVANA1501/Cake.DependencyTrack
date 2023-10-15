using System.Net.Http;
using System.Threading.Tasks;
using Cake.DependencyTrack.Models;

namespace Cake.DependencyTrack.Services;

internal interface IDependencyTrackClient
{
    Task<string> UploadBomAsync(string projectId, string bomFile);
    HttpRequestMessage HttpRequestMessage(HttpMethod method, object body, string path);
    Task<Project> GetProjectDetails(string projectName, string version);
    Task<AppVersion> GetServerVersion();
}