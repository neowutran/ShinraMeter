/*
 * Copyright 2007-2012 JetBrains s.r.o.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Tera.DamageMeter.Annotations
{
    /// <summary>
    ///     Indicates that marked element should be localized or not.
    /// </summary>
    /// <example>
    ///     <code>
    /// [LocalizationRequiredAttribute(true)]
    /// public class Foo
    /// {
    ///   private string str = "my string"; // Warning: Localizable string
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class LocalizationRequiredAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationRequiredAttribute" /> class with
        ///     <see cref="Required" /> set to <see langword="true" />.
        /// </summary>
        public LocalizationRequiredAttribute() : this(true)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationRequiredAttribute" /> class.
        /// </summary>
        /// <param name="required"><c>true</c> if a element should be localized; otherwise, <c>false</c>.</param>
        public LocalizationRequiredAttribute(bool required)
        {
            Required = required;
        }

        /// <summary>
        ///     Gets a value indicating whether a element should be localized.
        ///     <value><c>true</c> if a element should be localized; otherwise, <c>false</c>.</value>
        /// </summary>
        [UsedImplicitly]
        public bool Required { get; private set; }

        /// <summary>
        ///     Returns whether the value of the given object is equal to the current <see cref="LocalizationRequiredAttribute" />.
        /// </summary>
        /// <param name="obj">The object to test the value equality of. </param>
        /// <returns>
        ///     <c>true</c> if the value of the given object is equal to that of the current; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var attribute = obj as LocalizationRequiredAttribute;
            return attribute != null && attribute.Required == Required;
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="LocalizationRequiredAttribute" />.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


    // ASP.NET MVC attributes

    // Razor attributes
}