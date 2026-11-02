using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tman.Core;
using Tman.Core.Game;
using Tman.Core.Maps;
using Tman.Core.Surfaces.Building;
using Tman.Paks;

namespace Tman.Tests
{
    internal static class TestServices
    {
        public static readonly string TestMap = Path.Combine(AppContext.BaseDirectory, "Maps", "Test.Map.Gbx");

        public static ServiceProvider Build(Action<IServiceCollection>? configure = null)
        {
            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IPakStore, Fakes.NoPaks>()
                .AddSingleton<IAssets>(new DirectoryAssets(AppContext.BaseDirectory))
                .AddSingleton<IBlockCache, MemoryBlockCache>()
                .AddTman();
            configure?.Invoke(services);
            return services.BuildServiceProvider();
        }

        public static Map LoadTestMap() => Map.Load(TestMap);
    }
}
