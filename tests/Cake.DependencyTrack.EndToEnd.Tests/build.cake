#r "Cake.DependencyTrack.dll"

using Cake.DependencyTrack;
using Cake.DependencyTrack.Models;
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
    var settings = new UploadBomSettings{
       ProjectName="test",
       Version="CI",
       AutoCreate=true,
       AbsoluteBomFilePath="/Users/saravanakumar/Downloads/bom.xml",
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
