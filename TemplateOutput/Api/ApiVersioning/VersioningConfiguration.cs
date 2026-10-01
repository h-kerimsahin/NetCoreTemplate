using Microsoft.AspNetCore.OData;

namespace $safeprojectname$.Api.ApiVersioning;

public static class VersioningConfiguration
{
    public static IServiceCollection AddApiVersioningAndOData(this IServiceCollection services)
    {
        services.AddControllers()
            .AddOData(opt =>
            {
                opt.AddRouteComponents("api/v1/odata", OData.ODataModelBuilder.GetEdmModel(1.0))
                    .Count()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Select()
                    .SetMaxTop(100);
                opt.AddRouteComponents("api/v2/odata", OData.ODataModelBuilder.GetEdmModel(2.0))
                    .Count()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Select()
                    .SetMaxTop(100);
            });

        return services;
    }
}
