#r "lib/Cake.DependencyTrack.dll"

using Cake.DependencyTrack;
using Cake.DependencyTrack.Models;
using System.IO;
///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
.Does(async (context) => {
    FileInfo f = new FileInfo("TestData/bom.xml");
    var settings = new UploadBomSettings{
       ProjectName="test",
       Version="CI",
       AutoCreate=true,
       AbsoluteBomFilePath=f.FullName,
       ServerSettings=new ServerSettings{
            BaseServerUrl="http://localhost:8081",
            ApiKey="wiB8mCEPaDACQFhuNtEQhvNvFHRAITxI"
       },
       ShouldValidateMetrics=true,
       MetricsThresholdSettings=new MetricsThresholdSettings{
            CriticalCount=2,
            HighCount=2,
            MediumCount=2,
            LowCount=2
       }
    };
    await context.UploadBomFile(settings);
});

RunTarget(target);
