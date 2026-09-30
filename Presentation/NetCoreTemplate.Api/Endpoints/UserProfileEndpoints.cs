using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetCoreTemplate.Api.Idempotency;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.DTOs.UserProfile;
using NetCoreTemplate.Application.Features.UserProfile.Commands.Update;
using NetCoreTemplate.Application.Features.UserProfile.Queries.GetById;

namespace NetCoreTemplate.Api.Endpoints;

public class UserProfileEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/user-profiles").WithTags("UserProfiles").RequireAuthorization();

        group.MapGet("{id:guid}", async ([FromRoute] Guid id, [FromServices] ISender sender) =>
        {
            var response = await sender.Send(new GetUserProfileByIdQuery(id));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("GetUserProfileById");

        group.MapPut("mine", async ([FromBody] UpdateUserProfileRequestDto req, [FromServices] ISender sender, ClaimsPrincipal user, HttpContext ctx) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var response = await sender.Send(new UpdateUserProfileCommand(userId, req.FirstName, req.LastName, req.BirthDate, req.PhoneNumber, req.Address, req.City, req.Country, req.AvatarUrl, req.Bio, ip));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("UpdateMyProfile").AddEndpointFilter<IdempotencyEndpointFilter>();
    }
}
