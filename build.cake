#addin nuget:?package=Cake.Sonar&version=1.1.32
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.8.0

var target = Argument("target", "SonarEnd");
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

Task("SonarBegin")
.IsDependentOn("Build")
.Does(() => {
 SonarBegin(new SonarBeginSettings{
    Key = "saravana1501_cake-dependencytrack",
    Organization="saravana1501",
    Url = "https://sonarcloud.io",
    Token=token,
    OpenCoverReportsPath="./tests/Cake.DependencyTrack.Tests/TestResults/coverage.net6.0.opencover.xml"
 });
});

Task("Test")
    .IsDependentOn("SonarBegin")
    .Does(() =>
{
    DotNetTest(solution, new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
        ArgumentCustomization = args => args.Append(" -p:CollectCoverage=true -p:CoverletOutput=TestResults/ -p:CoverletOutputFormat=opencover")
    });
});


  
Task("SonarEnd")
.IsDependentOn("Test")
.Does(() => {
  SonarEnd(new SonarEndSettings{
     Token=token
  });
}); 

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);