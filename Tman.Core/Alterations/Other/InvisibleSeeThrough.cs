using System.Collections.Generic;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Other
{
    internal sealed class InvisibleSeeThrough : StandardAlteration
    {
        protected override string DisplayName => "Invisible See-Through";

        protected override IEnumerable<Alteration> Steps => Invisible.For(Surface.SeeThrough);
    }
}
