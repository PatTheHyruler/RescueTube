using System.Linq.Expressions;
using RescueTube.Core.Data.Specifications;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Specifications;

public class ImageSpecification : IImageSpecification
{
    public Expression<Func<Image, bool>> ShouldAttemptResolutionUpdate => image =>
        image.LocalFilePath != null
        && (image.Width == null || image.Height == null)
        && (image.ResolutionParseAttemptedAt == null ||
            image.ResolutionParseAttemptedAt < DateTimeOffset.UtcNow.AddDays(-30));
}