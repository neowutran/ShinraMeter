using System;

namespace Tera.DamageMeter.Annotations
{
    /// <summary>
    ///     Indicates that the value of the marked element could never be <c>null</c>
    /// </summary>
    /// <example>
    ///     <code>
    /// [NotNull]
    /// public object Foo()
    /// {
    ///   return null; // Warning: Possible 'null' assignment
    /// } 
    /// </code>
    /// </example>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate |
        AttributeTargets.Field)]
    public sealed class NotNullAttribute : Attribute
    {
    }
}