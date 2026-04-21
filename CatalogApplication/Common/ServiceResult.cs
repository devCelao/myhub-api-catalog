namespace CatalogApplication.Common;

/// <summary>
/// Padrão de resposta para operações de serviço com tipo genérico
/// </summary>
/// <typeparam name="T">Tipo de dado retornado</typeparam>
public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public List<string> Errors { get; private set; }
    public bool IsNotFound { get; private set; }

    private ServiceResult(bool isSuccess, T? data, string? message, List<string>? errors, bool isNotFound)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        Errors = errors ?? [];
        IsNotFound = isNotFound;
    }

    public static ServiceResult<T> Success(T data, string? message = null)
        => new(true, data, message, null, false);

    public static ServiceResult<T> Failure(string error)
        => new(false, default, error, [error], false);

    public static ServiceResult<T> Failure(List<string> errors)
        => new(false, default, errors.FirstOrDefault(), errors, false);

    public static ServiceResult<T> NotFound(string message = "Recurso não encontrado.")
        => new(false, default, message, [message], true);

    public static ServiceResult<T> ValidationError(List<string> validationErrors)
        => new(false, default, "Erro de validacao.", validationErrors, false);
}

/// <summary>
/// ServiceResult sem tipo genérico (para operações que não retornam dados)
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }
    public List<string> Errors { get; private set; }
    public bool IsNotFound { get; private set; }

    private ServiceResult(bool isSuccess, string? message, List<string>? errors, bool isNotFound)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? [];
        IsNotFound = isNotFound;
    }

    public static ServiceResult Success(string? message = null)
        => new(true, message, null, false);

    public static ServiceResult Failure(string error)
        => new(false, error, [error], false);

    public static ServiceResult Failure(List<string> errors)
        => new(false, errors.FirstOrDefault(), errors, false);

    public static ServiceResult NotFound(string message = "Recurso não encontrado.")
        => new(false, message, [message], true);

    public static ServiceResult ValidationError(List<string> validationErrors)
        => new(false, "Erro de validacao.", validationErrors, false);
}
