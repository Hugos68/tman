using System;
using Microsoft.Extensions.Logging;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class MapNameEdited(Func<string, string> edit) : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            map.Name = edit(map.Name);
            context.Logger.LogInformation("Renamed to {Name}", map.Name);
        }
    }
}
