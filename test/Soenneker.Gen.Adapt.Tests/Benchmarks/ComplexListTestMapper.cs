using Riok.Mapperly.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[Mapper]
public partial class ComplexListTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
    public partial NestedDest MapToNestedDest(NestedSource source);
    public partial ComplexListDest MapToComplexListDest(ComplexListSource source);
}


