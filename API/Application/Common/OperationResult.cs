namespace API.Application.Common;
public record OperationResult<T>(T? Value = default, IDictionary<string, string[]>? Errors = null,
    bool NotFound = false, string? Conflict = null);
