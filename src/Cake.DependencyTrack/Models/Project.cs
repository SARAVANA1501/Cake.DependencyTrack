using System;

namespace Cake.DependencyTrack.Models;

internal class Project
{
    public string Name { get; set; }
    public string Version { get; set; }
    public Guid Uuid { get; set; }
    public bool Active { get; set; }
}