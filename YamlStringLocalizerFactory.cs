using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Localization.Yaml
{
    internal class YamlStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ConcurrentDictionary<string, YamlStringLocalizer> _localizerCache =
            new ConcurrentDictionary<string, YamlStringLocalizer>();

        private readonly ILoggerFactory _loggerFactory;
        private readonly YamlLocalizationOptions _localizationOptions;

        public YamlStringLocalizerFactory(IOptions<YamlLocalizationOptions> localizationOptions, ILoggerFactory loggerFactory)
        {
            _localizationOptions = localizationOptions.Value;
            _loggerFactory = loggerFactory;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
            {
                throw new ArgumentNullException(nameof(resourceSource));
            }
            var resourceName = TrimPrefix(resourceSource.FullName, (_localizationOptions.RootNamespace ?? resourceSource.Namespace) + ".");
            return CreateYamlStringLocalizer(resourceName);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            var resourceName = TrimPrefix(baseName, location + ".");
            return CreateYamlStringLocalizer(resourceName);
        }

        private YamlStringLocalizer CreateYamlStringLocalizer(string resourceName)
        {
            var logger = _loggerFactory.CreateLogger<YamlStringLocalizer>();
            return _localizerCache.GetOrAdd(resourceName, resName => new YamlStringLocalizer(
                _localizationOptions,
                resName,
                logger));
        }

        private static string TrimPrefix(string name, string prefix)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return name.Substring(prefix.Length);
            }

            return name;
        }
    }
}
