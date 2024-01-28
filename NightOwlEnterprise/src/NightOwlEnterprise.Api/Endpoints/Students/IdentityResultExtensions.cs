using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class IdentityResultExtensions
{
    public static ValidationProblem CreateValidationProblem(this IdentityResult identityResult) =>
        CreateValidationProblemSpecial(identityResult);

    private static ValidationProblem CreateValidationProblemSpecial(IdentityResult result)
    {
        var errorDescriptions = result.Errors.Select(x => x.Description);

        return TypedResults.ValidationProblem(
            new Dictionary<string, string[]>(),
            extensions: new Dictionary<string, object?>()
            {
                { "errors",  errorDescriptions  }
            });
    }
}