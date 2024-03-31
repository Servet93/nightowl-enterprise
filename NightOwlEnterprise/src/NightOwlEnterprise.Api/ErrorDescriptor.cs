using Microsoft.AspNetCore.Http.HttpResults;

namespace NightOwlEnterprise.Api;

public class ErrorDescriptor
{
    public string Code { get; set; } = default!;

    public string Description { get; set; } = default!;

    public ErrorDescriptor()
    {
        
    }
    
    public ErrorDescriptor(string code, string description)
    {
        Code = code;
        Description = description;
    }
}

public static class ErrorDescriptorExtensions
{
    public static ValidationProblem CreateValidationProblem(this ErrorDescriptor errorDescriptor) =>
        CreateValidationProblemSpecial(errorDescriptor);

    private static ValidationProblem CreateValidationProblemSpecial(ErrorDescriptor errorDescriptor)
    {
        var dict = new Dictionary<string, string[]>()
        {
            { errorDescriptor.Code, new string[1] { errorDescriptor.Description } }
        };

        return TypedResults.ValidationProblem(dict);
    }
}