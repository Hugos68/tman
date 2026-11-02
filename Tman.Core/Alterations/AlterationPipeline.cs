using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tman.Core.Game;
using Tman.Core.Maps;
using Tman.Core.Surfaces.Building;

namespace Tman.Core.Alterations
{
    public sealed class AlterationPipeline
    {
        internal static readonly IReadOnlyList<Type> AlterationTypes =
            [.. typeof(StandardAlteration).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(StandardAlteration)) && !type.IsAbstract)];

        private readonly IServiceProvider services;

        private readonly IAssets assets;

        private readonly ISurfaceBlocks surfaceBlocks;

        private readonly ILoggerFactory loggers;

        internal AlterationPipeline(IServiceProvider services, IAssets assets, ISurfaceBlocks surfaceBlocks, ILoggerFactory loggers)
        {
            this.services = services;
            this.assets = assets;
            this.surfaceBlocks = surfaceBlocks;
            this.loggers = loggers;
        }

        public static IEnumerable<string> Names => AlterationTypes.Select(type => type.Name).Order(StringComparer.Ordinal);

        public void Apply(string alterationName, Map map)
        {
            var type = AlterationTypes.FirstOrDefault(candidate => candidate.Name.Equals(alterationName, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Unknown alteration: {alterationName}");
            var alteration = (Alteration)services.GetRequiredService(type);
            var logger = loggers.CreateLogger(type);
            using var scope = logger.BeginScope("{Map}", map.Name);
            var stopwatch = Stopwatch.StartNew();
            alteration.Apply(map, new AlterationContext(assets, surfaceBlocks, logger));
            logger.LogInformation("Applied in {Elapsed} ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
