namespace RescueTube.Core.Jobs;

public static class JobQueues
{
    public const string Critical = "priority_critical";
    public const string HighPriority = "priority_high";
    public const string Default = "default";
    public const string LowPriority = "priority_low";
    public const string LowerPriority = "priority_lower";

    public static string[] Queues = [Critical, HighPriority, Default, LowPriority, LowerPriority];
}