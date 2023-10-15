using System;
using System.Collections.Generic;

namespace Cake.DependencyTrack.Models;

internal class AppVersion
{
    public Guid Uuid { get; set; }
    public string Application { get; set; }
    public string Version { get; set; }
}