using System.Threading.Tasks;
using Soenneker.Dtos.Results.Operation;

namespace Soenneker.Gen.Adapt.Tests.Types;

public interface IAdvancedValueProvider
{
    ValueTask<OperationResult<InlineValueResponse>> GetInline(string id);

    ValueTask<OperationResult<HelperValueResponse>> GetHelper(string id);

    ValueTask<PayloadEnvelope<PayloadValueResponse>> GetPayload(string id);
}
