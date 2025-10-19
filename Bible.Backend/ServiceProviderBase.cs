using Bible.Backend.Abstractions;
using Bible.Backend.Services;
using Bible.Usx.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bible.Backend
{
    public abstract class ServiceProviderBase
    {
        protected static void RegisterIoC(IServiceCollection services)
        {
#if DEBUG
            services.AddLogging(o => o.AddDebug());
#endif
            // Services
            services.AddSingleton<IStorageService, DictionaryStorageService>();
            services.AddScoped<IBibleBookNavService, BibleBookNavService>();
            services.AddSingleton<UsxParserFactory>();
            services.AddSingleton<UsxToUsjConverter>();
            services.AddSingleton<XmlReaderDeserializer>();
            services.AddSingleton<UnihanService>();
            services.AddSingleton<UsjBookService>();

            _provider = services.BuildServiceProvider();
        }

        private static IServiceProvider? _provider;

        /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
        public static T? GetService<T>()
        {
            if (_provider == null)
                return default;
            return _provider.GetService<T>();
        }

        /// <inheritdoc cref="ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)(Type)"/>
        public static T GetRequiredService<T>() where T : notnull
        {
            if (_provider == null)
                throw new ArgumentNullException(nameof(_provider));
            return _provider.GetRequiredService<T>() ??
                throw new InvalidOperationException($"There is no service of type {typeof(T).FullName}");
        }
    }
}
