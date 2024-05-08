namespace WebApp.ApiModels;

public class ErrorResponseDto
{
    /// <summary>
    /// Custom error code identifying the type of error.
    /// </summary>
    public required EErrorType ErrorType { get; set; } = EErrorType.GenericError;
    /// <summary>
    /// Text describing the error.
    /// </summary>
    public string? Message { get; set; }
    /// <summary>
    /// Error details object.
    /// </summary>
    public object? Details { get; set; }

    public List<ErrorResponseDto> SubErrors { get; set; } = [];
}