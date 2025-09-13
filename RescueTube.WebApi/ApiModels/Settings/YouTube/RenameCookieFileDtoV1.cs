using FluentValidation;
using RescueTube.WebApi.Utils.Validation;

namespace RescueTube.WebApi.ApiModels.Settings.YouTube;

public record RenameCookieFileDtoV1
{
    public required string OldFileName { get; init; }
    public required string NewFileName { get; init; }
}

// ReSharper disable once UnusedType.Global
public class RenameCookieFileDtoV1Validator : AbstractValidator<RenameCookieFileDtoV1>
{
    public RenameCookieFileDtoV1Validator()
    {
        RuleFor(x => x.OldFileName).NotEmpty();
        RuleFor(x => x.NewFileName)
            .NotEmpty()
            .MaximumLength(30)
            .Matches(ValidationUtils.LimitedFileNameRegex);
    }
}