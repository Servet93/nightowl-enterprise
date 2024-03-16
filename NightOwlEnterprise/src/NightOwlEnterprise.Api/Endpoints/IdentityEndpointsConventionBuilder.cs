namespace NightOwlEnterprise.Api.Endpoints;

// Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
public sealed class IdentityEndpointsConventionBuilder : IEndpointConventionBuilder
{
    private readonly IEndpointConventionBuilder _innerAsConventionBuilder;

    public IdentityEndpointsConventionBuilder(IEndpointConventionBuilder inner)
    {
        _innerAsConventionBuilder = inner ?? throw new ArgumentNullException(nameof(inner));
    }
        
    public void Add(Action<EndpointBuilder> convention) => _innerAsConventionBuilder.Add(convention);
    public void Finally(Action<EndpointBuilder> finallyConvention) => _innerAsConventionBuilder.Finally(finallyConvention);
}