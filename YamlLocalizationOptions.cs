using System;

namespace Localization.Yaml
{
    public class YamlLocalizationOptions
    {
        /// <summary>
        /// The relative path under application root where resource files are located.
        /// </summary>
        public string ResourcesPath { get; set; } = "Resources";

        /// <summary>
        /// ResourcesPathType
        /// </summary>
        public ResourcesPathType ResourcesPathType { get; set; }

        /// <summary>
        /// RootNamespace
        /// use entry assembly name by default
        /// </summary>
        public string RootNamespace { get; set; }

        public ResourceBuildType BuildType { get; set; } = ResourceBuildType.FileSystem;

        public Type ResourceAssemblyType { get; set; } = null;


    }

    public enum ResourceBuildType
    {
        FileSystem,
        Embeded
    }

    public enum ResourcesPathType
    {
        TypeBased = 0,
        CultureBased = 1,
    }
}
