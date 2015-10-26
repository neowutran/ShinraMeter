using System;

namespace Tera.DamageMeter.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. Indicates that a parameter is an MVC araa.
    ///     Use this attribute for custom wrappers similar to
    ///     <see cref="System.Web.Mvc.Html.ChildActionExtensions.RenderAction(HtmlHelper, string)" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMvcAreaAttribute : PathReferenceAttribute
    {
        [UsedImplicitly]
        public AspMvcAreaAttribute()
        {
        }

        public AspMvcAreaAttribute(string anonymousProperty)
        {
            AnonymousProperty = anonymousProperty;
        }

        [UsedImplicitly]
        public string AnonymousProperty { get; private set; }
    }
}