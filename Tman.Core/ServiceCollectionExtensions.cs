using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tman.Core.Alterations;
using Tman.Core.Game;
using Tman.Core.Surfaces.Building;

namespace Tman.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTman(this IServiceCollection services)
        {
            services.AddSingleton<GameMaterials>();
            services.AddSingleton<PartGeometry>();
            services.AddSingleton<SurfaceBlockMesh>();
            services.AddSingleton<SurfaceBlockBuilder>();
            services.AddSingleton<ISurfaceBlocks, CachedSurfaceBlocks>();
            services.AddSingleton(provider => new AlterationPipeline(provider, provider.GetRequiredService<IAssets>(), provider.GetRequiredService<ISurfaceBlocks>(), provider.GetRequiredService<ILoggerFactory>()));
            foreach (var alteration in AlterationPipeline.AlterationTypes)
            {
                services.AddTransient(alteration);
            }
            return services;
        }
    }
}
