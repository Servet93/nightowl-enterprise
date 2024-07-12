using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;

namespace NightOwlEnterprise.Api.Endpoints.Students.Me;

public static class UploadProfilePhoto
{
    public static void MapUploadProfilePhoto(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/me/profile-photo", async Task<Results<Ok, ProblemHttpResult>>
                ([FromForm]IFormFile file, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                if (file == null || file.Length == 0)
                {
                    var errorDescriptor = new ErrorDescriptor("NoFile", "No file uploaded");
                    return errorDescriptor.CreateProblem();
                    // return Results.BadRequest(new { Message = "No file uploaded" });
                }

                // Dosya uzantısını kontrol edin (güvenlik için)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    var errorDescriptor = new ErrorDescriptor("InvalidFileType", "Invalid file type");
                    return errorDescriptor.CreateProblem();
                    
                    // return Results.BadRequest(new { Message = "Invalid file type" });
                }

                // Dosya boyutunu kontrol edin (örneğin 5 MB maksimum)
                var maxFileSize = 5 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    var errorDescriptor = new ErrorDescriptor("FileSizeExceeds", "File size exceeds the limit of 5 MB");
                    return errorDescriptor.CreateProblem();
                    
                    // return Results.BadRequest(new { Message = "File size exceeds the limit of 5 MB" });
                }
                
                var coachId = claimsPrincipal.GetId();

                var fileAsBase64 = string.Empty;
                
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

                    // Bellek akışını sıfırla
                    stream.Position = 0;
                    
                    var t = ImageProcessor.ResizeImage(stream, 500, 500);

                    fileAsBase64 = Convert.ToBase64String(t);
                }

                // var fileAsBase64 = file.ToBase64String();
                
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var profilePhoto = dbContext.ProfilePhotos.FirstOrDefault(x => x.UserId == coachId);

                if (profilePhoto is null)
                {
                    dbContext.ProfilePhotos.Add(new ProfilePhoto()
                    {
                        UserId = coachId,
                        Photo = fileAsBase64
                    });
                }
                else
                {
                    profilePhoto.Photo = fileAsBase64;
                }

                await dbContext.SaveChangesAsync();
                
                // Benzersiz dosya adı oluşturun
                // var uniqueFileName = $"{coachId}{extension}";
                //
                // // Yükleme için gerekli dizini oluşturun
                // var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                // if (!Directory.Exists(uploadDirectory))
                // {
                //     Directory.CreateDirectory(uploadDirectory);
                // }
                //
                // // Dosyayı yükleme dizinine kaydedin
                // var filePath = Path.Combine(uploadDirectory, uniqueFileName);
                //
                // using (var stream = new FileStream(filePath, FileMode.Create))
                // {
                //     await file.CopyToAsync(stream);
                // }

                return TypedResults.Ok();
                
            }).RequireAuthorization("Student").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery()
            .WithTags(TagConstants.StudentsMeInfo);
        
        endpoints.MapGet("/me/profile-photo", async Task<Results<EmptyHttpResult, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
            {
                var coachId = claimsPrincipal.GetId();
                 
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
                
            }).RequireAuthorization("Student").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.StudentsMeInfo);
        
         endpoints.MapGet("/me/profile-photo/as-base64", async Task<Results<Ok<StudentProfilePhotoInfo>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
             {
                 var coachId = claimsPrincipal.GetId();
                 
                 var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                 var profilePhoto = await dbContext.ProfilePhotos.FirstOrDefaultAsync(x => x.UserId == coachId);
                 
                return TypedResults.Ok(new StudentProfilePhotoInfo()
                {
                    ProfilePhoto = profilePhoto?.Photo
                });
                
            }).RequireAuthorization("Student").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
             .WithTags(TagConstants.StudentsMeInfo);
    }

    public class StudentProfilePhotoInfo
    {
        public string? ProfilePhoto { get; set; }
    }
}