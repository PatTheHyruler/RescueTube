using FluentValidation;
using RescueTube.WebApi.Utils.Validation;

namespace RescueTube.WebApi.ApiModels.Settings.YouTube;

public sealed record CreateCookieFileDtoV1
{
    public required string Content { get; init; }
    public string? FileName { get; init; }
}

// ReSharper disable once UnusedType.Global
public sealed class CreateCookieFileDtoV1Validator : AbstractValidator<CreateCookieFileDtoV1>
{
    public CreateCookieFileDtoV1Validator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(8 * 1000);
        RuleFor(x => x.FileName)
            .MaximumLength(30)
            .Matches(ValidationUtils.LimitedFileNameRegex);
    }
}