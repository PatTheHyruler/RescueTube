namespace WebApp.ViewModels;

public class PaginationUiPartialViewModel
{
    public string ControllerName { get; set; } = default!;
    public string ActionName { get; set; } = default!;
    public object? RouteValues { get; set; }
    public string LimitParamName { get; set; } = default!;
    public string PageParamName { get; set; } = default!;
    public int Limit { get; set; }
    public int Page { get; set; }
    public int? Total { get; set; }
    public int AmountOnPage { get; set; }
}