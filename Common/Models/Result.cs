namespace ZeeAcom.Common.Models;
public class Result
{
    public bool Succeeded { get; set; }
    public List<string>? Errors { get; set; }

    public bool IsFailure => !Succeeded;
    public string FirstError => Errors?.FirstOrDefault() ?? string.Empty;

    public static Result Success() => new()
    {
        Succeeded = true
    };

    public static Result Failure(List<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors
    };

    public static Result Failure(string error) => new()
    {
        Succeeded = false,
        Errors = [error]
    };
}

public class Result<T>
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public int? Count { get; set; }
    public List<string>? Errors { get; set; }

    public bool IsFailure => !Succeeded;
    public string FirstError => Errors?.FirstOrDefault() ?? string.Empty;

    public static Result<T> Success(T data) => new()
    {
        Data = data,
        Succeeded = true
    };

    public static Result<T> Success(T data, int count) => new()
    {
        Data = data,
        Count = count,
        Succeeded = true
    };

    public static Result<T> Failure(string error) => new()
    {
        Succeeded = false,
        Errors = [error]
    };

    public static Result<T> Failure(List<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors
    };
}
