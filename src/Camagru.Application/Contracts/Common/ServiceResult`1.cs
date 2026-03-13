namespace Camagru.Application.Contracts.Common;

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    private ServiceResult(bool success, T? data = default, string? error = null)
        : base(success, error)
    {
        Data = data;
    }

    public static ServiceResult<T> Ok(T data) => new(true, data);

    public new static ServiceResult<T> Fail(string error) => new(false, default, error);
}
