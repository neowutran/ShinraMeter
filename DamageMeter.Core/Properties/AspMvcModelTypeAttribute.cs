using System;

namespace Tera.DamageMeter.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. Indicates that a parameter is an MVC model type.
    ///     Use this attribute for custom wrappers similar to
    ///     <see cref="System.Web.Mvc.Controller.View(string, object)" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMvcModelTypeAttribute : Attribute
    {
    }
}