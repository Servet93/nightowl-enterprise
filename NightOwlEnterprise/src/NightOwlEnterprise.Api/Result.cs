namespace NightOwlEnterprise.Api;

public class Result
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public ErrorDescriptor Error { get; }

    protected Result() => IsSuccess = true;

    protected Result(ErrorDescriptor error)
    {
        IsSuccess = false;
        Error = error;  
    } 

    public static Result Success() => new();
    
    public static Result Failure(ErrorDescriptor error) => new(error);
}

public class Result<TValue>
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;
    public ErrorDescriptor Error { get; }
    
    private readonly TValue? _value;

    public TValue? Value => _value;

    protected internal Result(ErrorDescriptor error)
    {
        Error = error;
        IsSuccess = false;
    }

    protected internal Result(TValue? value)
    {
        IsSuccess = true;
        _value = value;  
    } 
    
    public static Result<TValue> Success(TValue value) => new(value);
    
    public static Result<TValue> Failure(ErrorDescriptor error) => new(error);
}
