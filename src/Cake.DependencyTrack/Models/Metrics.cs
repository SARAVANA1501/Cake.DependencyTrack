namespace Cake.DependencyTrack.Models;

internal class Metrics
{
    public int Critical { get; set; }
    public int High { get; set; }
    public int Medium { get; set; }
    public int Low { get; set; }
    public long LastOccurrence { get; set; }
}