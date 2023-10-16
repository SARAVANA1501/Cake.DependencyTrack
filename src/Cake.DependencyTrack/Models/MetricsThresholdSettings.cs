namespace Cake.DependencyTrack.Models;

public class MetricsThresholdSettings
{
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int LowCount { get; set; }
}