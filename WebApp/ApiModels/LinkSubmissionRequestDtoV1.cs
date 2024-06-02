using System.ComponentModel.DataAnnotations;
using RescueTube.Domain.Enums;

namespace WebApp.ApiModels;

public class LinkSubmissionRequestDtoV1
{
    [Required]
    public required string Url { get; set; }

    public EPlatform? PlatformHint { get; set; }
    public EEntityType? TypeHint { get; set; }
}