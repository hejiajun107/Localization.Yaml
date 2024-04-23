using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Localization.Yaml
{
    internal class YamlStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _resourcesCache = new ConcurrentDictionary<string, Dictionary<string, string>>();
        private readonly string _resourcesPath;
        private readonly string _resourceName;
        private readonly ResourcesPathType _resourcesPathType;
        private readonly ILogger _logger;
        YamlLocalizationOptions _options;

        private string _searchedLocation;

        public YamlStringLocalizer(
            YamlLocalizationOptions localizationOptions,
            string resourceName,
            ILogger logger)
        {
            _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
            _logger = logger ?? NullLogger.Instance;
            _resourcesPath = Path.Combine(AppContext.BaseDirectory, localizationOptions.ResourcesPath);
            _resourcesPathType = localizationOptions.ResourcesPathType;
            _options = localizationOptions;

        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetStringSafely(name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var format = GetStringSafely(name);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null, searchedLocation: _searchedLocation);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

        public IStringLocalizer WithCulture(CultureInfo culture) => this;

        private IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var resourceNames = includeParentCultures
                ? GetAllStringsFromCultureHierarchy(culture)
                : GetAllResourceStrings(culture);

            foreach (var name in resourceNames)
            {
                var value = GetStringSafely(name);
                yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
            }
        }

        private string GetStringSafely(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            string value = null;

            var resources = GetResources(CultureInfo.CurrentUICulture.Name);
            if (resources?.ContainsKey(name) == true)
            {
                value = resources[name];
            }

            return value;
        }

        private IEnumerable<string> GetAllStringsFromCultureHierarchy(CultureInfo startingCulture)
        {
            var currentCulture = startingCulture;
            var resourceNames = new HashSet<string>();

            while (currentCulture.Equals(currentCulture.Parent) == false)
            {
                var cultureResourceNames = GetAllResourceStrings(currentCulture);

                if (cultureResourceNames != null)
                {
                    foreach (var resourceName in cultureResourceNames)
                    {
                        resourceNames.Add(resourceName);
                    }
                }

                currentCulture = currentCulture.Parent;
            }

            return resourceNames;
        }

        private IEnumerable<string> GetAllResourceStrings(CultureInfo culture)
        {
            var resources = GetResources(culture.Name);
            return resources?.Select(r => r.Key);
        }

        private Dictionary<string, string> GetResources(string culture)
        {
            return _resourcesCache.GetOrAdd(culture, _ =>
            {
                var resourceFile = "yaml";

                if(_options.BuildType == ResourceBuildType.FileSystem)
                {
                    if (_resourcesPathType == ResourcesPathType.TypeBased)
                    {
                        resourceFile = $"{culture}.yaml";
                        if (_resourceName != null)
                        {
                            resourceFile = string.Join(".", _resourceName.Replace('.', Path.DirectorySeparatorChar), resourceFile);
                        }
                    }
                    else
                    {
                        resourceFile = string.Join(".",
                            Path.Combine(culture, _resourceName.Replace('.', Path.DirectorySeparatorChar)), resourceFile);
                    }

                    _searchedLocation = Path.Combine(_resourcesPath, resourceFile);
                    Dictionary<string, string> value = null;

                    if (File.Exists(_searchedLocation))
                    {
                        var content = File.ReadAllText(_searchedLocation, System.Text.Encoding.UTF8);
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            try
                            {
                                var deserializer = new DeserializerBuilder()
                                .Build();

                                value = deserializer.Deserialize<Dictionary<string, string>>(content.Trim());
                            }
                            catch (Exception e)
                            {
                                _logger.LogWarning(e, $"invalid yaml content, path: {_searchedLocation}, content: {content}");
                            }
                        }
                    }
                    return value;
                }
                else if(_options.BuildType == ResourceBuildType.Embeded)
                {
                    resourceFile = $"{culture}.yaml";
                  

                    Dictionary<string, string> value = null;

                    if(_options.ResourceAssemblyType is null)
                    {
                        throw new Exception("The ResourceAssemblyType was not defined.");
                    }

                    Assembly assembly = Assembly.GetAssembly(_options.ResourceAssemblyType);
                    var resourses =  assembly.GetManifestResourceNames();

                    if (_resourceName != null)
                    {
                        resourceFile = string.Join(".", _options.ResourceAssemblyType.Namespace, _options.ResourcesPath, $"{_resourceName}_{resourceFile}");
                    }

                    // 获取嵌入的资源流
                    using (Stream stream = assembly.GetManifestResourceStream(resourceFile))
                    {
                        // 确保资源存在
                        if (stream == null)
                        {
                            throw new Exception("The embedded resource was not found.");
                        }

                        // 读取流内容
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string content = reader.ReadToEnd();
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                try
                                {
                                    var deserializer = new DeserializerBuilder()
                                    .Build();

                                    value = deserializer.Deserialize<Dictionary<string, string>>(content.Trim());
                                }
                                catch (Exception e)
                                {
                                    _logger.LogWarning(e, $"invalid yaml content, path: {_searchedLocation}, content: {content}");
                                }
                            }
                        }
                    }
            
                    return value;
                }
                else
                {
                    throw new NotSupportedException("Unexpected ResourceBuildType");
                }
             
            });
        }
    }
}
