using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class IdentityResultExtensions
{
    public static ValidationProblem CreateValidationProblem(this IdentityResult identityResult) =>
        CreateValidationProblemSpecial(identityResult);

    private static ValidationProblem CreateValidationProblemSpecial(IdentityResult result)
    {
        var dict = new Dictionary<string, string[]>();
        
        foreach (var error in result.Errors)
        {
            dict.Add(error.Code, new string[1] {error.Description});    
        }

        return TypedResults.ValidationProblem(dict);
        
        
        // return TypedResults.ValidationProblem(
        //     new Dictionary<string, string[]>(),
        //     extensions: new Dictionary<string, object?>()
        //     {
        //         { "errors",  errorDescriptions  }
        //     });
    }
}