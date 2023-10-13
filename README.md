# Cake.DependencyTrack

![workflow](https://github.com/SARAVANA1501/Cake.DependencyTrack/actions/workflows/build.yml/badge.svg)
![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/SARAVANA1501/cb35602c6a5e75e8a32ca62e0d79631b/raw/cakedependancytrackcoverage.json&logo=csharp)


DependencyTrack (https://dependencytrack.org/) is an open source software composition analysis platform that allows organizations to identify and reduce the risk of using third-party components in their applications. 

DependencyTrack integrates with multiple sources of vulnerability intelligence, such as the National Vulnerability Database (NVD), and provides a comprehensive view of the security posture of the software supply chain. DependencyTrack supports multiple formats for importing and exporting data, such as Software Bill of Materials (SBOM), CycloneDX, SPDX, and OWASP Dependency-Check. DependencyTrack also offers a rich API and a web-based dashboard for managing and visualizing the analysis results.

Cake.DependencyTrack add-in helps to integrate the dependency track platform to your CI/CD pipelines and provides functionalities like Build time bom upload, fail the pipeline when thresholds are not satisfied. 