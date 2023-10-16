# Cake.DependencyTrack

![workflow](https://github.com/SARAVANA1501/Cake.DependencyTrack/actions/workflows/build.yml/badge.svg)
![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/SARAVANA1501/cb35602c6a5e75e8a32ca62e0d79631b/raw/cakedependancytrackcoverage.json&logo=csharp)

DependencyTrack (https://dependencytrack.org/) is an open source software composition analysis platform that allows
organizations to identify and reduce the risk of using third-party components in their applications.

DependencyTrack integrates with multiple sources of vulnerability intelligence, such as the National Vulnerability
Database (NVD), and provides a comprehensive view of the security posture of the software supply chain. DependencyTrack
supports multiple formats for importing and exporting data, such as Software Bill of Materials (SBOM), CycloneDX, SPDX,
and OWASP Dependency-Check. DependencyTrack also offers a rich API and a web-based dashboard for managing and
visualizing the analysis results.

Cake.DependencyTrack add-in helps to integrate the dependency track platform to your CI/CD pipelines and provides
functionalities like Build time bom upload, fail the pipeline when thresholds are not satisfied.

### How To Use?

Sample code:
```
//Installing package
#addin nuget:?package=Cake.DependencyTrack&version=x.x.x&prerelease&loaddependencies=true

using Cake.DependencyTrack.Models;
using Cake.DependencyTrack;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Default")
.Does(async (context) => {
    var settings = new UploadBomSettings{
       ProjectId="99f8c557-5896-4adf-903e-966d7b47e86f",
       AbsoluteBomFilePath="<bom file location>",
       ServerSettings=new ServerSettings{
            BaseServerUrl="<dependency track server base url>",
            ApiKey="<api key>"
       }
    };
    await context.UploadBomFile(settings);
});

RunTarget(target);
```

Sample code with Auto create option:
```
Task("Default")
.Does(async (context) => {
    var settings = new UploadBomSettings{
       ProjectName="test",
       Version="CI",
       AutoCreate=true,
       AbsoluteBomFilePath="<bom file location>",
       ServerSettings=new ServerSettings{
            BaseServerUrl="<dependency track server base url>",
            ApiKey="<api key>"
       }
    };
    await context.UploadBomFile(settings);
});
```
Sample code with threshold validation, if the project metrics are greater then are equal to given threshold then the pipeline will fail
```
Task("Default")
.Does(async (context) => {
    var settings = new UploadBomSettings{
       ProjectName="test",
       Version="CI",
       AutoCreate=true,
       AbsoluteBomFilePath="<bom file location>",
       ServerSettings=new ServerSettings{
            BaseServerUrl="<dependency track server base url>",
            ApiKey="<api key>"
       }
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
```

### Parameters

| Parameter                       | Description                                                                                                                                 |
|---------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------|
| ProjectId                       | Unique identifier for the project in Dependency track system. If projectId is not specified then projectName and version must be specified. |
| AbsoluteBomFilePath             | Absolute bom file path (example: C:\Users\user\Downloads\bom.xml). <br/>Acceptable format CycloneDX format.                                 |   
| ProjectName                     | Project name. If the project name and version combination not found then upload will fail. Unless Auto creation enabled.                    |
| Version                         | Project version. If the project name and version combination not found then upload will fail. Unless Auto creation enabled.                 |
| AutoCreate                      | If autoCreate is specified 'true' and the project does not exist, the project will be created.                                              |
| ServerSettings.BaseServerUrl(*) | Server base url.                                                                                                                            |
| ServerSettings.ApiKey(*)        | Api Key. See document to generate Api Key: https://docs.dependencytrack.org/integrations/rest-api/                                          |
