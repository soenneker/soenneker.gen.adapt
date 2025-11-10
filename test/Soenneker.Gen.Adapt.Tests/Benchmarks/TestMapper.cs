using Riok.Mapperly.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[Mapper]
public partial class TestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
}

