using Riok.Mapperly.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[Mapper]
public partial class LargeListTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
    public partial List<BasicDest> MapToBasicDestList(List<BasicSource> source);
}


