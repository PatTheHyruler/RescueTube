using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Localization;

namespace WebApp.Utils;

public static class DateTimeOffsetHelpers
{
    private static IHtmlContent SpanFor(DateTimeOffset? value, string culture = "en-US")
    {
        return new HtmlString($"<span class='date-time-local' culture='{culture}'>{value.ToString()} UTC</span>");
    }

    public static IHtmlContent ToSpan(this DateTimeOffset value, HttpContext context) =>
        SpanFor(value, context.CurrentCultureName());

    public static CultureInfo CurrentUiCulture(this HttpContext context) =>
        context.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture ??
        new CultureInfo("en-US").UseConstantDateTimeOffset();

    public static string CurrentUiCultureName(this HttpContext context) =>
        context.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name ?? "en-US";

    public static string CurrentCultureName(this HttpContext context) =>
        context.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture.Name ?? "en-US";

    public static IHtmlContent SpanFor(this HttpContext context, DateTimeOffset? value) =>
        SpanFor(value, context.CurrentCultureName());

    public static CultureInfo UseConstantDateTimeOffset(this CultureInfo cultureInfo)
    {
        try
        {
            cultureInfo.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
        }
        catch (InvalidOperationException)
        {
            cultureInfo = new CultureInfo(cultureInfo.Name)
            {
                DateTimeFormat =
                {
                    ShortDatePattern = "yyyy-MM-dd"
                }
            };
        }

        return cultureInfo;
    }

    public static string GetActiveClass(this CultureInfo cultureInfo, HttpContext context) =>
        cultureInfo.Name == context.CurrentUiCulture().Name ? "active" : "";
}