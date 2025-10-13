using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Collections;

public class IEnumerableCollectionsSource
{
    public IEnumerable<int> IntEnumerable { get; set; }
    public ICollection<string> StringCollection { get; set; }
    public IList<double> DoubleList { get; set; }
    public IReadOnlyList<int> ReadOnlyIntList { get; set; }
    public IReadOnlyCollection<string> ReadOnlyStringCollection { get; set; }
}

