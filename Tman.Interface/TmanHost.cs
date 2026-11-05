using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tman.Core;
using Tman.Core.Alterations;
using Tman.Core.Game;
using Tman.Core.Surfaces.Building;
using Tman.Paks;

namespace Tman.Interface
{
    public sealed class TmanHost : IDisposable
    {
        private const string ApplicationFolder = "tman";

        private const string CacheFolder = "cache";

        private readonly ServiceProvider services;

        private TmanHost(ServiceProvider services)
        {
            this.services = services;
        }

        public static TmanHost Create(Action<ILoggingBuilder> logging)
        {
            var services = new ServiceCollection()
                .AddLogging(logging)
                .AddSingleton<IPakStore>(_ => new PakStore(PakDirectory.Find()))
                .AddSingleton<IAssets>(new DirectoryAssets(AppContext.BaseDirectory))
                .AddSingleton<IBlockCache>(provider => new FileBlockCache(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationFolder, CacheFolder), provider.GetRequiredService<IPakStore>()))
                .AddTman()
                .BuildServiceProvider();
            return new TmanHost(services);
        }

        public AlterationPipeline Pipeline => services.GetRequiredService<AlterationPipeline>();

        public ILogger Logger(string name) => services.GetRequiredService<ILoggerFactory>().CreateLogger(name);

        public void Dispose() => services.Dispose();
    }
}
