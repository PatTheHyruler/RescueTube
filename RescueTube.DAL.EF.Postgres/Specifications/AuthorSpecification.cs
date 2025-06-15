using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data.Specifications;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Postgres.Specifications;

public class AuthorSpecification : IAuthorSpecification
{
    public Expression<Func<Author, bool>> HasName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return static a => true;
        }

        var nameQuery = '%' + Utils.EscapeWildcards(name) + '%';
        return a =>
            a.DisplayName != null && Microsoft.EntityFrameworkCore.EF.Functions.ILike(a.DisplayName, nameQuery, "\\") ||
            a.UserName != null && Microsoft.EntityFrameworkCore.EF.Functions.ILike(a.UserName, nameQuery, "\\");
    }
}