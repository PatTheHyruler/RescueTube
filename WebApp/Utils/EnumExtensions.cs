using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace WebApp.Utils;

public static class EnumExtensions
{
    public static string Translate<TEnum>(this IHtmlHelper htmlHelper, TEnum @enum) where TEnum : struct, Enum
    {
        var attribute = typeof(TEnum).GetMember(@enum.ToString()).FirstOrDefault()
            ?.GetCustomAttribute<DisplayAttribute>();
        if (attribute?.ResourceType == null || attribute.Name == null) return @enum.ToString();

        var localizer = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService(typeof(IStringLocalizer<>)
                    .MakeGenericType(attribute.ResourceType))
            as IStringLocalizer;
        return localizer?[attribute.Name] ?? attribute.Name;
    }

    public static IEnumerable<SelectListItem> GetLocalizedEnumSelectList<TEnum>(this IHtmlHelper htmlHelper,
        TEnum? selected = null) where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>()
            .Select(value => new SelectListItem(
                htmlHelper.Translate(value),
                value.ToString(),
                selected != null && selected.Value.Equals(value)))
            .ToList();
    }
}