using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace NightOwlEnterprise.Api.Endpoints;

public static class IdentityResultExtensions
{
    public static ProblemHttpResult CreateProblem(this IdentityError identityError, string? detail = null)
    {
        var dictExtensions = new Dictionary<string, object?>()
        {
            { "errors" , new List<ErrorDescriptor>() { new ErrorDescriptor(identityError.Code, identityError.Description) } }
        };

        detail = string.IsNullOrEmpty(detail) ? identityError.Description : null;
        
        return TypedResults.Problem(detail, statusCode: StatusCodes.Status400BadRequest, extensions:dictExtensions);
    }
    
    public static ProblemHttpResult CreateProblem(this List<IdentityError> identityErrors, string? detail = null)
    {
        var errorDescriptors = identityErrors.Select(x => new ErrorDescriptor(x.Code, x.Description));
        
        var dictExtensions = new Dictionary<string, object?>()
        {
            { "errors" , errorDescriptors }
        };
        
        return TypedResults.Problem(detail, statusCode: StatusCodes.Status400BadRequest, extensions:dictExtensions);
    }
    
    public static ProblemHttpResult CreateProblem(this IEnumerable<IdentityError> identityErrors, string? detail = null)
    {
        var errorDescriptors = identityErrors.Select(x => new ErrorDescriptor(x.Code, x.Description));
        
        var dictExtensions = new Dictionary<string, object?>()
        {
            { "errors" , errorDescriptors }
        };
        
        return TypedResults.Problem(detail, statusCode: StatusCodes.Status400BadRequest, extensions:dictExtensions);
    }
    
    public static ProblemHttpResult CreateProblem(this ErrorDescriptor errorDescriptor, string? detail = null)
    {
        var dictExtensions = new Dictionary<string, object?>()
        {
            { "errors" , new List<ErrorDescriptor>() { errorDescriptor } }
        };
        
        detail = string.IsNullOrEmpty(detail) ? errorDescriptor.Description : null;

        return TypedResults.Problem(detail, statusCode: StatusCodes.Status400BadRequest, extensions:dictExtensions);
    }
    
    public static ProblemHttpResult CreateProblem(this List<ErrorDescriptor> errorDescriptors, string? detail = null)
    {
        var dictExtensions = new Dictionary<string, object?>()
        {
            { "errors" , errorDescriptors }
        };

        return TypedResults.Problem(detail, statusCode: StatusCodes.Status400BadRequest, extensions:dictExtensions);
    }
}