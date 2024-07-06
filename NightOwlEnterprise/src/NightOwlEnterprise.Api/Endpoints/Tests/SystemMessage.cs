
using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.Common;

public static class SystemMessage
{
    public static void MapSystemMessage(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/system-message", async Task<Results<Ok, ProblemHttpResult>>
            ([FromQuery] Guid senderId, [FromQuery] Guid receiverId, [FromQuery] string message,[FromServices] IServiceProvider sp) =>
        {
            var chatClientService = sp.GetRequiredService<ChatClientService>();
            
            chatClientService.SendMessageFromSystem(senderId.ToString(), receiverId.ToString(), message);

            return TypedResults.Ok();
        }).ProducesValidationProblem(401).ProducesValidationProblem(403);
    }
}