using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApp.Utils;

public static class InputHelpers
{
    private static ModelExpressionProvider? GetExpressionProvider(this IHtmlHelper htmlHelper)
    {
        return htmlHelper.ViewContext.HttpContext.RequestServices.GetService<ModelExpressionProvider>();
    }
    
    private static string? GetExpressionText<TModel, TProperty>(this IHtmlHelper htmlHelper,
        Expression<Func<TModel, TProperty>> expression)
    {
        return GetExpressionProvider(htmlHelper)?.GetExpressionText(expression);
    }
    
    private static string? GetPropertyName<TModel, TProperty>(this IHtmlHelper htmlHelper,
        Expression<Func<TModel, TProperty>> expression)
    {
        return htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(GetExpressionText(htmlHelper,
            expression));
    }

    public static IHtmlContent HiddenValueFor<TModel, TProperty>(
        this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, TProperty value)
    {
        var propertyName = htmlHelper.GetPropertyName(expression);

        return htmlHelper.Hidden(propertyName, value);
    }
    
    public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException($"Expression {expression} doesn't refer to a property");
        }

        if (memberExpression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException($"Expression {expression} doesn't refer to a property");
        }

        var type = typeof(TSource);
        if (propertyInfo.ReflectedType != null && type != propertyInfo.ReflectedType &&
            !type.IsSubclassOf(propertyInfo.ReflectedType))
        {
            throw new ArgumentException($"Expression {expression} refers to a property that is not from type {type}");
        }

        return propertyInfo;
    }

    public static IHtmlContent CheckBoxFor<TModel>(
        this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, bool value,
        string @class = "")
    {
        var propertyName = GetPropertyName(htmlHelper, expression);

        @class = SingleSpaces(@class);
        var htmlAttributes = new { @class };

        return htmlHelper.CheckBox(propertyName, value, htmlAttributes);
    }

    private static string SingleSpaces(string s)
    {
        return Regex.Replace(s, @" +", " ");
    }

    public static string GetFullPath(this HttpRequest request)
    {
        return request.Path + request.QueryString;
    }

    public static string GetFullPath(this HttpContext context)
    {
        return context.Request.GetFullPath();
    }

    public static object GetRouteValues(this HttpRequest request)
    {
        var queryValues = QueryHelpers.ParseQuery(request.QueryString.ToString());

        dynamic customObject = new ExpandoObject();
        
        foreach (var query in queryValues)
        {
            ((IDictionary<string, object>)customObject)[query.Key] = query.Value;
        }

        return customObject;
    }

    public static object GetRouteValues(this HttpContext context) => context.Request.GetRouteValues();

    public static IEnumerable<SelectListItem> GetEnumSelectList<TEnum>(this IHtmlHelper html, TEnum value) where TEnum : struct
    {
        var selectList = html.GetEnumSelectList<TEnum>();
        var selectListItems = selectList.ToList();
        var selected = selectListItems.FirstOrDefault(e => e.Value == Convert.ToInt32(value).ToString());
        if (selected != null) selected.Selected = true;
        return selectListItems;
    }
}