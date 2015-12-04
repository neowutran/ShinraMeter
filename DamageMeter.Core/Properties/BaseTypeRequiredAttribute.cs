using System;

namespace Tera.DamageMeter.Annotations
{
    /// <summary>
    ///     When applied to a target attribute, specifies a requirement for any type marked with
    ///     the target attribute to implement or inherit specific type or types.
    /// </summary>
    /// <example>
    ///     <code>
    /// [BaseTypeRequired(typeof(IComponent)] // Specify requirement
    /// public class ComponentAttribute : Attribute 
    /// {}
    /// 
    /// [Component] // ComponentAttribute requires implementing IComponent interface
    /// public class MyComponent : IComponent
    /// {}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [BaseTypeRequired(typeof (Attribute))]
    public sealed class BaseTypeRequiredAttribute : Attribute
    {
        /// <summary>
        ///     Initializes new instance of BaseTypeRequiredAttribute
        /// </summary>
        /// <param name="baseType">Specifies which types are required</param>
        public BaseTypeRequiredAttribute(Type baseType)
        {
            BaseTypes = new[] {baseType};
        }

        /// <summary>
        ///     Gets enumerations of specified base types
        /// </summary>
        public Type[] BaseTypes { get; private set; }
    }
}