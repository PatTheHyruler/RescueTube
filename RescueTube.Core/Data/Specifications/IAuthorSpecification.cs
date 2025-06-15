using System.Linq.Expressions;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IAuthorSpecification
{
    Expression<Func<Author, bool>> HasName(string? name);
}