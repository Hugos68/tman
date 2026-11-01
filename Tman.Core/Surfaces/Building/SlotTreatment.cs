using GBX.NET.Engines.Plug;

namespace Tman.Core.Surfaces.Building
{
    internal readonly record struct SlotTreatment(bool Deck, CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId? Gameplay);
}
