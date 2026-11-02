using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Tman.Core.Maps;

namespace Tman.Core.Alterations
{
    public sealed class AlterationPipeline(IServiceProvider services)
    {
        internal static readonly IReadOnlyList<Type> AlterationTypes =
            [.. typeof(Alteration).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Alteration)) && !type.IsAbstract)];

        public static IEnumerable<string> Names => AlterationTypes.Select(type => type.Name).Order(StringComparer.Ordinal);

        public void Apply(string alterationName, Map map)
        {
            var type = AlterationTypes.FirstOrDefault(candidate => candidate.Name.Equals(alterationName, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Unknown alteration: {alterationName}");
            var alteration = (Alteration)services.GetRequiredService(type);
            alteration.Apply(map, services.GetRequiredService<AlterationContext>());
        }
    }
}
