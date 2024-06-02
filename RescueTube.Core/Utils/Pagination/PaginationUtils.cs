namespace RescueTube.Core.Utils.Pagination;

public static class PaginationUtils
{
    public static int? ClampTotal(int? total)
    {
        return total == null ? null : Math.Max(total.Value, 0);
    }

    public static int ClampPage(int? total, int limit, int page)
    {
        return Math.Max(0, 
            Math.Min(GetLastPage(total, limit), page));
    }

    public static int ClampLimit(int limit, int maxLimit = 100) => Math.Max(Math.Min(limit, maxLimit), 1);

    public static int PageToSkipAmount(int limit, int page)
    {
        return page * limit;
    }

    public static bool IsOnlyPage(int? total, int limit)
    {
        return total <= limit;
    }

    public static bool IsLastPage(int? total, int limit, int page)
    {
        if (total == null) return false;
        return page == GetLastPage(total.Value, limit);
    }

    public static int GetLastPage(int? total, int limit)
    {
        if (total == null) return int.MaxValue;
        return Math.Max(total.Value - 1, 0) / limit;
    }

    public static IPaginationQuery ToClamped(this IPaginationQuery query)
    {
        return new PaginationQuery
        {
            Limit = ClampLimit(query.Limit),
            Page = Math.Max(query.Page, 0),
        };
    }

    public static PaginationResult ToPaginationResult(this IPaginationQuery query, int amountOnPage, int? totalResults = null)
    {
        return new PaginationResult
        {
            Limit = query.Limit,
            Page = query.Page,
            AmountOnPage = amountOnPage,
            TotalResults = totalResults,
        };
    }

    public static int GetSkipAmount(this IPaginationQuery query)
    {
        return PageToSkipAmount(query.Limit, query.Page);
    }
}