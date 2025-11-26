using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks.Mappers;

[Mapper]
public partial class NestedListTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
    public partial NestedDest MapToNestedDest(NestedSource source);
    public partial List<NestedDest> MapToNestedDestList(List<NestedSource> source);
}


