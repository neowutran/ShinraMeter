using System;

namespace Tera.DamageMeter.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. If applied to a parameter, indicates that the parameter is an MVC controller.
    ///     If applied to a method, the MVC controller name is calculated implicitly from the context.
    ///     Use this attribute for custom wrappers similar to
    ///     <see cref="System.Web.Mvc.Html.ChildActionExtensions.RenderAction(HtmlHelper, string, string)" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public sealed class AspMvcControllerAttribute : Attribute
    {
        public AspMvcControllerAttribute()
        {
        }

        public AspMvcControllerAttribute(string anonymousProperty)
        {
            AnonymousProperty = anonymousProperty;
        }

        [UsedImplicitly]
        public string AnonymousProperty { get; private set; }
    }
}