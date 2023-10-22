#addin "nuget:?package=Cake.Sonar"
#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool"

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");
var configuration = Argument("token", "");
var solution = "Cake.DependencyTrack.sln";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .Does(() =>
{
    DotNetBuild(solution, new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest(solution, new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
        ArgumentCustomization = args => args.Append(" -p:CollectCoverage=true -p:CoverletOutput=TestResults/ -p:CoverletOutputFormat=opencover")
    });
});

Task("SonarBegin")
.IsDependentOn("Test")
.Does(() => {
 SonarBegin(new SonarBeginSettings{
    # Supported parameters
    Key = "saravana1501_cake-dependencytrack",
    Url = "https://sonarcloud.io",
    Token=""
 });
});
  
Task("SonarEnd")
.IsDependentOn("SonarBegin")
.Does(() => {
  SonarEnd(new SonarEndSettings{
     Token=""
  });
}); 

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);