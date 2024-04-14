using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NightOwlEnterprise.Api.Endpoints.Universities;

public static class UniversityEndpoints
{
    public static IEndpointConventionBuilder MapUniversity(this IEndpointRouteBuilder endpoints)
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

            universities.AddRange(await applicationDbContext.Universities.Where(x => x.Name.ToLower().Contains(name.ToLower()))
                .Select(x => new UniversityItem(x.Id, x.Name)).ToListAsync());
            
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
        
        routeGroup.MapGet("/{universityId}/departments", async Task<Results<Ok<List<DepartmentItem>>, ProblemHttpResult>>
            ([FromRoute]Guid universityId, [FromServices] IServiceProvider sp) =>
        {
            var departmentItems = new List<DepartmentItem>();
            
            var applicationDbContext = sp.GetRequiredService<ApplicationDbContext>();

            var university = await applicationDbContext.Universities.Where(x => x.Id == universityId)
                .FirstOrDefaultAsync();

            departmentItems.AddRange(
                university?.UniversityDepartments.Select(x => new DepartmentItem(x.DepartmentId, x.Department.Name)) ?? Array.Empty<DepartmentItem>());
            
            return TypedResults.Ok(departmentItems);
            
        }).ProducesProblem(StatusCodes.Status500InternalServerError);

        routeGroup.MapPost("/{universityId}/departments",
            async Task<Results<Ok<UniversityDepartmentItem>, ProblemHttpResult>>
            ([FromRoute] Guid universityId, [FromBody] string name, HttpContext context,
                [FromServices] IServiceProvider sp) =>
            {
                var applicationDbContext = sp.GetRequiredService<ApplicationDbContext>();

                var university = await applicationDbContext.Universities.Where(x => x.Id == universityId)
                    .FirstOrDefaultAsync();

                if (university is null)
                {
                    return TypedResults.Problem("Üniversite bulunamadı");
                }

                var nameExist = university.UniversityDepartments
                    .FirstOrDefault(x =>
                        string.Equals(x.Department.Name, name, StringComparison.CurrentCultureIgnoreCase));

                if (nameExist is not null)
                {
                    return TypedResults.Ok(new UniversityDepartmentItem(nameExist.University.Id,
                        nameExist.University.Name, nameExist.Department.Id, nameExist.Department.Name));
                }

                var departmentEntity = new Department()
                {
                    Name = name
                };

                await applicationDbContext.Departments.AddAsync(departmentEntity);

                var universityDepartmentEntity = new UniversityDepartment()
                {
                    UniversityId = universityId,
                    DepartmentId = departmentEntity.Id
                };

                await applicationDbContext.UniversityDepartments.AddAsync(universityDepartmentEntity);

                return TypedResults.Ok(new UniversityDepartmentItem(university.Id, university.Name, departmentEntity.Id,
                    departmentEntity.Name));

            }).ProducesProblem(StatusCodes.Status400BadRequest);

        return new IdentityEndpointsConventionBuilder(routeGroup).WithOpenApi().WithTags("Üniversite");
    }

    public record CreateDepartmentItem(Guid UniversityId, string Name);
    public record UniversityItem(Guid Id, string Name);
    public record DepartmentItem(Guid Id, string Name);
    public record UniversityDepartmentItem(Guid UniversityId, string UniversityName, Guid DepartmentId, string DepartmentName);
}
