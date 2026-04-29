using System.Threading.Tasks;
using Soenneker.Dtos.Results.Operation;

namespace Soenneker.Gen.Adapt.Tests.Types;

public interface IValueProvider<T>
{
    ValueTask<OperationResult<T>> Get(string id);
}
