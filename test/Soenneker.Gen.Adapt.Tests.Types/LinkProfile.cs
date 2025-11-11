using System;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Types;

public class LinkProfile
{
    public string Name { get; set; } = string.Empty;
    public Uri Primary { get; set; } = new("https://example.org");
    public Uri? Secondary { get; set; }
    public List<Uri> Mirrors { get; set; } = [];
}


