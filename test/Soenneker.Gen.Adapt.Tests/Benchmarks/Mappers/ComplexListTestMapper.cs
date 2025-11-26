using Riok.Mapperly.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks.Mappers;

[Mapper]
public partial class ComplexListTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);

    public partial NestedDest MapToNestedDest(NestedSource source);

    public partial ComplexListDest MapToComplexListDest(ComplexListSource1 source);

    public partial ComplexListFacetDestComparison MapToComplexListFacetDestComparison(ComplexListSource2 source);
}