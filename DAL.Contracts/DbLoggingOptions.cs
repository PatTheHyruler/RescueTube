namespace DAL.Contracts;

public class DbLoggingOptions
{
    public const string Section = "Logging:DB";

    public bool SensitiveDataLogging { get; set; } = false;
}