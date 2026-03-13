namespace Camagru.Application.Contracts.Common;

public class ServiceResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    protected ServiceResult(bool success, string? error = null)
    {
        Success = success;
        Error = error;
    }

    public static ServiceResult Ok() => new(true);

    public static ServiceResult Fail(string error) => new(false, error);
}
