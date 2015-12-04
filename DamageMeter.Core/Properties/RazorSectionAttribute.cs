using System;

namespace Tera.DamageMeter.Annotations
{
    /// <summary>
    ///     Razor attribute. Indicates that a parameter or a method is a Razor section.
    ///     Use this attribute for custom wrappers similar to
    ///     <see cref="System.Web.WebPages.WebPageBase.RenderSection(string)" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public sealed class RazorSectionAttribute : Attribute
    {
    }
}