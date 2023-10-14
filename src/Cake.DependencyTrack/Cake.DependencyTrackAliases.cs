using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cake.DependencyTrack
{
    [CakeAliasCategory("Cake.DependencyTrack")]
    public static class AddinAliases
    {
        [CakeMethodAlias]
        [CakeAliasCategory("Dependency Track file upload")]
        public static void UploadBOMFile(this ICakeContext ctx, string name)
        {
            
            ctx.Log.Information("Hello " + name);
        }
    }
}