namespace Public.DTO.v1;

/// <summary>
/// Generic error response.
/// Any errors intentionally returned by the API should follow this format.
/// </summary>
public class RestApiErrorResponse
{
    /// <summary>
    /// The type of the error.
    /// </summary>
    public EErrorType ErrorType { get; set; }
    /// <summary>
    /// An optional message describing the error.
    /// </summary>
    public string? Message { get; set; }
}