using System.Linq.Expressions;
using Hangfire;

namespace RescueTube.Core.Jobs.Registration;

public class HangfireRecurringJobRegistry
{
    private readonly List<HangfireJobRegistration> _registeredJobs = [];
    public IReadOnlyCollection<HangfireJobRegistration> RegisteredJobs => _registeredJobs;

    public void RegisterJob<T>(
        string recurringJobId,
        Expression<Func<T, Task>> methodCall,
        string cronExpression,
        bool isArchivalJob = true)
    {
        _registeredJobs.Add(new HangfireJobRegistration(
            RecurringJobId: recurringJobId,
            RegistrationAction: x => x.AddOrUpdate(
                recurringJobId: recurringJobId,
                methodCall: methodCall,
                cronExpression: cronExpression),
            IsArchivalJob: isArchivalJob));
    }

    public void RegisterJob<T>(
        string recurringJobId,
        Expression<Action<T>> methodCall,
        string cronExpression,
        bool isArchivalJob = true)
    {
        _registeredJobs.Add(new HangfireJobRegistration(
            RecurringJobId: recurringJobId,
            RegistrationAction: x => x.AddOrUpdate(
                recurringJobId: recurringJobId,
                methodCall: methodCall,
                cronExpression: cronExpression),
            IsArchivalJob: isArchivalJob));
    }

    public readonly record struct HangfireJobRegistration(
        string RecurringJobId,
        Action<IRecurringJobManagerV2> RegistrationAction,
        bool IsArchivalJob);
}