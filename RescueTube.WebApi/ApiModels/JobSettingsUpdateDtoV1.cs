using FluentValidation;

namespace RescueTube.WebApi.ApiModels;

public record JobSettingsUpdateDtoV1
{
    public required string JobId { get; init; }

    public required bool IsEnabled { get; init; }
    public required string Cron { get; init; }

    public required DataFetchJobSettingsDtoV1? DataFetchJobSettings { get; init; }
}

public sealed class JobSettingsUpdateDtoV1Validator : AbstractValidator<JobSettingsUpdateDtoV1>
{
    public JobSettingsUpdateDtoV1Validator()
    {
        RuleFor(x => x.JobId).NotEmpty();
        RuleFor(x => x.Cron).NotEmpty();
        RuleFor(x => x.DataFetchJobSettings!).ChildRules(v =>
        {
            v.RuleFor(x => x.SuccessCutoffOffset).GreaterThan(TimeSpan.FromMinutes(5));
            v.RuleFor(x => x.FailureCutoffOffset).GreaterThan(TimeSpan.FromMinutes(5));
        }).When(x => x.DataFetchJobSettings is not null);
    }
}

public sealed class JobSettingsUpdateDtoV1CollectionValidator : AbstractValidator<ICollection<JobSettingsUpdateDtoV1>>
{
    public JobSettingsUpdateDtoV1CollectionValidator()
    {
        RuleFor(x => x).ForEach(v =>
        {
            v.SetValidator(new JobSettingsUpdateDtoV1Validator());
        });
    }
}