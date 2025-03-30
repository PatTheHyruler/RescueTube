using System.ComponentModel.DataAnnotations;

namespace WebApp.ApiModels;

public class LinkSubmissionRequestDtoV1
{
    [Required]
    public required string Url { get; set; }
}