using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Collections;

public class DictionaryCollectionsDest
{
    public Dictionary<string, int> StringIntDict { get; set; }
    public Dictionary<int, string> IntStringDict { get; set; }
    public Dictionary<string, List<int>> StringListDict { get; set; }
    public Dictionary<string, Dictionary<string, int>> NestedDict { get; set; }
}

