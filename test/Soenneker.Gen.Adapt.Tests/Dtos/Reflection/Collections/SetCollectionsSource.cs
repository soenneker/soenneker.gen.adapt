using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Collections;

public class SetCollectionsSource
{
    public HashSet<int> IntHashSet { get; set; }
    public HashSet<string> StringHashSet { get; set; }
    public SortedSet<int> IntSortedSet { get; set; }
}

