using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api.Endpoints.Universities;

public static class UniversityEndpoints
{
    public static IEndpointConventionBuilder MapUniversityApi(this IEndpointRouteBuilder endpoints)
    {
        var routeGroup = endpoints.MapGroup("universities");

        routeGroup.MapGet("/", async Task<Results<Ok<List<UniversityItem>>, ProblemHttpResult>>
            ([FromQuery] string? name, [FromServices] IServiceProvider sp) =>
        {
            var applicationDbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var universities = new List<UniversityItem>();
            
            if (string.IsNullOrEmpty(name))
            {
                universities.AddRange(await applicationDbContext.Universities.Select(x => new UniversityItem(x.Id, x.Name)).ToListAsync());
            }
            else
            {
                universities.AddRange(await applicationDbContext.Universities.Where(x => x.Name.ToLower().Contains(name.ToLower()))
                    .Select(x => new UniversityItem(x.Id, x.Name)).ToListAsync());    
            }
            
            return TypedResults.Ok(universities);
            
        }).ProducesProblem(StatusCodes.Status500InternalServerError);
        
        routeGroup.MapPost("/", async Task<Results<Ok<UniversityItem>, ProblemHttpResult>>
            ([FromBody] string name, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var applicationDbContext = sp.GetRequiredService<ApplicationDbContext>();

            var university = await applicationDbContext.Universities.Where(x => x.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();

            if (university is not null) return TypedResults.Ok(new UniversityItem(university.Id, university.Name));
            
            var entity = new University()
            {
                Name = name
            };
                
            var result = await applicationDbContext.Universities.AddAsync(entity);
                
            return TypedResults.Ok(new UniversityItem(entity.Id, entity.Name));

        }).ProducesProblem(StatusCodes.Status500InternalServerError);

        return new IdentityEndpointsConventionBuilder(routeGroup).WithOpenApi().WithTags("Üniversite");
    }
    
    public record UniversityItem(Guid Id, string Name);
}
