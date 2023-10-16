using Cake.Core;
using Cake.Core.Annotations;
using System.Net.Http;
using System.Threading.Tasks;
using Cake.DependencyTrack.Models;
using Cake.DependencyTrack.Services;

namespace Cake.DependencyTrack
{
    [CakeAliasCategory("Cake.DependencyTrack")]
    public static class CakeDependencyTrackAliases
    {
        [CakeMethodAlias]
        [CakeAliasCategory("Dependency Track file upload")]
        public static async Task UploadBomFile(this ICakeContext ctx, UploadBomSettings uploadBomSettings)
        {
            var context = new DependencyTrackContext(new DependencyTrackClient(new HttpClient(),
                uploadBomSettings.ServerSettings.BaseServerUrl,
                uploadBomSettings.ServerSettings.ApiKey), ctx);
            await context.UploadBom(uploadBomSettings);
        }
    }
}