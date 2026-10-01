using System.Text.Json;
using System.Text.Json.Serialization;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Api.Endpoints;

public class V2TestEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v2/test")
            .WithTags("V2Test");

        group.MapGet("dummy", () =>
        {
            var data = new { Version = "v2.0", Test = true };
            var response = ApiResponse.Success(data, StatusCodes.Status200OK, "V2 Dummy endpoint çalışıyor.");
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("V2DummyTest").AllowAnonymous();
    }
}
