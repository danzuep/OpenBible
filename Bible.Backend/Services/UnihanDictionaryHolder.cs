using Bible.Usx.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Unihan.Models;
using Unihan.Services;

namespace Bible.Backend.Services
{
    public class InitializationHostedService : IHostedService
    {
        private readonly IServiceProvider _provider;
        private readonly UnihanLanguage _unihanLanguage;

        public InitializationHostedService(IServiceProvider provider, UnihanLanguage unihanLanguage)
        {
            _provider = provider;
            _unihanLanguage = unihanLanguage;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _provider.CreateScope();
            var unihanLanguage = scope.ServiceProvider.GetRequiredService<UnihanLanguage>();
            var unihanService = scope.ServiceProvider.GetRequiredService<UnihanService>();
            _unihanLanguage.Dictionary = await unihanService.GetUnihanDictionaryAsync(UnihanField.kCantonese) ?? new UnihanDictionary();
            var usxParserFactory = scope.ServiceProvider.GetRequiredService<UsxParserFactory>();
            usxParserFactory.SetTextParser(_unihanLanguage.Dictionary.GetValue);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
