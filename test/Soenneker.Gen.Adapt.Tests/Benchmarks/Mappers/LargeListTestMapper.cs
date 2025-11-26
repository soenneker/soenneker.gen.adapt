using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks.Mappers;

[Mapper]
public partial class LargeListTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);

    public partial List<BasicDest> MapToBasicDestList(List<BasicSource> source);
}