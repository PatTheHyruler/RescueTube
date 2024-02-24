namespace WebApp.ViewModels;

public record PaginationButtonPartialViewModel(int Page, int Limit, string Text, bool Disabled = false);