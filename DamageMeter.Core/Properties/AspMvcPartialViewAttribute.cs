using System;

namespace Tera.DamageMeter.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. If applied to a parameter, indicates that the parameter is an MVC partial view.
    ///     If applied to a method, the MVC partial view name is calculated implicitly from the context.
    ///     Use this attribute for custom wrappers similar to
    ///     <see cref="System.Web.Mvc.Html.RenderPartialExtensions.RenderPartial(HtmlHelper, string)" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public sealed class AspMvcPartialViewAttribute : PathReferenceAttribute
    {
    }
}