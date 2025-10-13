using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Collections;

public class ListCollectionsDest
{
    public List<int> IntList { get; set; }
    public List<string> StringList { get; set; }
    public List<double> DoubleList { get; set; }
    public List<List<int>> NestedIntList { get; set; }
}

