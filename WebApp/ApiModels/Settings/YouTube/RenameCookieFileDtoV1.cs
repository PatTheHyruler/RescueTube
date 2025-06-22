using System.ComponentModel.DataAnnotations;
using WebApp.Utils.Validation;

namespace WebApp.ApiModels.Settings.YouTube;

public record RenameCookieFileDtoV1
{
    public required string OldFileName { get; init; }

    [MaxLength(30), RegularExpression(ValidationUtils.LimitedFileNameRegex)]
    public required string NewFileName { get; init; }
}