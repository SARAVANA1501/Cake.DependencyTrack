#addin nuget:?package=Cake.Sonar&version=1.1.32
#tool dotnet:?package=dotnet-sonarscanner&version=5.14.0

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");
var token = Argument("token", "");
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
    Key = "saravana1501_cake-dependencytrack",
    Organization="saravana1501",
    Url = "https://sonarcloud.io",
    Token=token
 });
});
  
Task("SonarEnd")
.IsDependentOn("SonarBegin")
.Does(() => {
  SonarEnd(new SonarEndSettings{
     Token=token
  });
}); 

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);