using Base.Domain;
using Domain.Enums;

namespace Domain.Entities;

public class StatusChangeEvent : AbstractIdDatabaseEntity
{
    public bool? PreviousAvailability { get; set; }
    public bool? NewAvailability { get; set; }
    public EPrivacyStatus? PreviousPrivacyStatus { get; set; }
    public EPrivacyStatus? NewPrivacyStatus { get; set; }
    public DateTime OccurredAt { get; set; }

    public Guid? AuthorId { get; set; }
    public Author? Author { get; set; }
    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }
}