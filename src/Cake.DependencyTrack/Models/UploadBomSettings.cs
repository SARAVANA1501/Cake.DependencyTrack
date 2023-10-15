namespace Cake.DependencyTrack.Models;

public class UploadBomSettings
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Version { get; set; }
    public bool AutoCreate { get; set; }
    public string AbsoluteBomFilePath { get; set; }
    public ServerSettings ServerSettings { get; set; }
}