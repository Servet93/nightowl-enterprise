using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Endpoints.CommonDto;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class GetProfilePhoto
{
    public static void MapGetProfilePhoto(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/{coachId}/profile-photo", async Task<Results<EmptyHttpResult, ProblemHttpResult>>
            ([FromRoute] Guid coachId, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();

            var profilePhoto = dbContext.ProfilePhotos.FirstOrDefault(x => x.UserId == coachId);

            if (string.IsNullOrEmpty(profilePhoto?.Photo))
            {
                return TypedResults.Empty;
            }
            
            // Content-Type başlığını ayarlama
            var contentType = "image/*"; // ya da başka bir resim türü
            httpContext.Response.ContentType = contentType;

            var data = Convert.FromBase64String(profilePhoto.Photo);
            // Resmi yanıt olarak gönderme
            // await stream.CopyToAsync(httpContext.Response.Body);

            await httpContext.Response.Body.WriteAsync(data, 0, data.Length);

            return TypedResults.Empty;
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsCoachListAndReserve);
    }
}