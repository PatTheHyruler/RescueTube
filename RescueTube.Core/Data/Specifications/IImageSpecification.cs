using System.Linq.Expressions;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IImageSpecification
{
    Expression<Func<Image, bool>> ShouldAttemptResolutionUpdate { get; }
}