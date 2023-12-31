#addin nuget:?package=Cake.Sonar&version=1.1.32
#tool dotnet:?package=dotnet-sonarscanner&version=5.14.0

var target = Argument("target", "SonarEnd");
var isCI = Argument("mode", "local")=="CI";
var configuration = Argument("configuration", "Release");
var token = Argument("token", "");
var solution = "Cake.DependencyTrack.sln";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("SonarBegin")
.WithCriteria(isCI)
.Does(() => {
 SonarBegin(new SonarBeginSettings{
    Key = "saravana1501_cake-dependencytrack",
    Name="Cake.DependencyTrack",
    Organization="saravana1501",
    Url = "https://sonarcloud.io",
    Token=token,
    OpenCoverReportsPath="./tests/Cake.DependencyTrack.Tests/TestResults/coverage.net6.0.opencover.xml",
    UseCoreClr=true,
    Exclusions="**/tests/**"
 });
});

Task("Build")
.IsDependentOn("SonarBegin")
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

Task("Publish")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetPublish("./src/Cake.DependencyTrack/Cake.DependencyTrack.csproj", new DotNetPublishSettings {
        Configuration = configuration,
        OutputDirectory = "./artifact/lib",
        Framework = "net6.0"
    });
});


  
Task("SonarEnd")
.WithCriteria(isCI)
.IsDependentOn("Publish")
.Does(() => {
  SonarEnd(new SonarEndSettings{
     Token=token,
     UseCoreClr=true
  });
}); 

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);