using BLL.DTO.Entities;
using Domain.Enums;

namespace BLL.Contracts;

public interface IPlatformSubmissionHandler
{
    public bool IsPlatformUrl(string url);
    public Task<LinkSubmissionResult> SubmitLink(string url, Guid submitterId, bool autoSubmit);
    public bool CanHandle(EPlatform platform, EEntityType entityType);
}