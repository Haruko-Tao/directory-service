namespace DirectoryService.Shared;

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public static Error NotFound(string? code, string message) => 
        new(code ?? "not.found", message, ErrorType.NOTFOUND);

    public static Error Validation(string? code, string message) => 
        new(code ?? "validation", message, ErrorType.VALIDATION);

    public static Error Conflict(string? code, string message) =>
        new(code ?? "conflict", message, ErrorType.CONFLICT);

    public static Error Internal(string? code, string message) =>
       new(code ?? "internal", message, ErrorType.INTERNAL);
}

public enum ErrorType
{
    /// <summary>
    /// Ошибка с валидацией.
    /// </summary>
    VALIDATION,
    
    /// <summary>
    /// Ошибка ничего не найдено.
    /// </summary>
    NOTFOUND,
    
    /// <summary>
    /// Ошибка кофликт.
    /// </summary>
    CONFLICT,
    
    /// <summary>
    /// Ошибка на стороне сервера.
    /// </summary>
    INTERNAL
}