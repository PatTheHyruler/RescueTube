namespace RescueTube.Core.Jobs;

public static class JobQueues
{
    // Unfortunately queue processing order is storage-specific
    // In the case of PostgreSQL, it seems to be alphabetical ascending order?
    public const string Critical = "a1000_priority_critical";
    public const string HighPriority = "a2000_priority_high";
    public const string Default = "default";
    public const string LowPriority = "z1000_priority_low";
    public const string LowerPriority = "z2000_priority_lower";

    public static string[] Queues = [Critical, HighPriority, Default, LowPriority, LowerPriority];
}