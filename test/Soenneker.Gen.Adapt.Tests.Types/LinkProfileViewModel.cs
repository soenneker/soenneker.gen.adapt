namespace Soenneker.Gen.Adapt.Tests.Types;

public class LinkProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public Uri Primary { get; set; } = new("https://example.org");
    public Uri? Secondary { get; set; }
    public IReadOnlyList<Uri> Mirrors { get; set; } = Array.Empty<Uri>();
}


