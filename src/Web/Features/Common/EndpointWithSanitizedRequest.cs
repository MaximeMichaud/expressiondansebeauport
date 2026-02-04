using FastEndpoints;

namespace Web.Features.Common;

public class EndpointWithSanitizedRequest<TRequest, TResponse> : Endpoint<TRequest, TResponse> where TRequest : ISanitizable, new()
{
    public override void OnBeforeValidate(TRequest req)
    {
        req.Sanitize();
    }
}