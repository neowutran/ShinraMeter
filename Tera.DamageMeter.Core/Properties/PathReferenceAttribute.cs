using System;

namespace Tera.DamageMeter.Annotations
{
    /// <summary>
    ///     Indicates that a parameter is a path to a file or a folder within a web project.
    ///     Path can be relative or absolute, starting from web root (~).
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class PathReferenceAttribute : Attribute
    {
        public PathReferenceAttribute()
        {
        }

        [UsedImplicitly]
        public PathReferenceAttribute([PathReference] string basePath)
        {
            BasePath = basePath;
        }

        [UsedImplicitly]
        public string BasePath { get; private set; }
    }
}