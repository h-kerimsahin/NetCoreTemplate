using System.Reflection;

namespace NetCoreTemplate.Api.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var endpointTypes = assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpoint endpoint)
            {
                endpoint.Map(app);
            }
        }

        return app;
    }
}
