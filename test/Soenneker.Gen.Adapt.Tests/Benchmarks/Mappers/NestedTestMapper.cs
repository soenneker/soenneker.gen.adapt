using Riok.Mapperly.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks.Mappers;

[Mapper]
public partial class NestedTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);

    public partial NestedDest MapToNestedDest(NestedSource2 source);

    public partial NestedFacetDestComparison MapToNestedFacetDest(NestedSource1 source);
}


