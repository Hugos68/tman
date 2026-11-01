using System.Collections.Generic;
using Tman.Core.Alterations.Steps;

namespace Tman.Core.Alterations
{
    public abstract class StandardAlteration : Alteration
    {
        protected abstract string DisplayName { get; }

        protected virtual IEnumerable<Alteration> Steps => [];

        protected sealed override IEnumerable<Alteration> PreappliedAlterations =>
            [new ValidationRemoved(), new LightmapRemoved(), .. Steps, new MapNameEdited(name => $"{name} {DisplayName}")];
    }
}
