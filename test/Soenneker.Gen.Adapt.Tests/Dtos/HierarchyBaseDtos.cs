namespace Soenneker.Gen.Adapt.Tests.Dtos;

public abstract class SharedBaseSource
{
    public string Key { get; set; }
}

public sealed class AlphaSource : SharedBaseSource
{
    public int Count { get; set; }
}

public sealed class BetaSource : SharedBaseSource
{
    public string Description { get; set; }
}

public sealed class GammaSource : SharedBaseSource
{
    public decimal Amount { get; set; }
}

public sealed class HierarchySource
{
    public AlphaSource First { get; set; }
    public BetaSource Second { get; set; }
    public GammaSource Third { get; set; }
}

public abstract class SharedBaseDest
{
    public string Key { get; set; }
}

public sealed class AlphaDest : SharedBaseDest
{
    public int Count { get; set; }
}

public sealed class BetaDest : SharedBaseDest
{
    public string Description { get; set; }
}

public sealed class GammaDest : SharedBaseDest
{
    public decimal Amount { get; set; }
}

public sealed class HierarchyDest
{
    public AlphaDest First { get; set; }
    public BetaDest Second { get; set; }
    public GammaDest Third { get; set; }
}

