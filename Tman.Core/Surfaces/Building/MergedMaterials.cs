using System.Collections.Generic;
using GBX.NET.Engines.Plug;

namespace Tman.Core.Surfaces.Building
{
    internal sealed class MergedMaterials
    {
        private const int UserInstanceVersion = 11;

        private const int UserInstanceTilingVersion = 5;

        private const float TextureSizeInMeters = 1;

        private readonly List<CPlugSolid2Model.Material> materials = [];

        private readonly Dictionary<(string Link, CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay), int> indices = [];

        public int IndexOf(string link, CPlugSurface.MaterialId physics, CPlugSurface.GameplayId gameplay)
        {
            var key = (link, physics, gameplay);
            if (!indices.TryGetValue(key, out var index))
            {
                index = materials.Count;
                materials.Add(UserMaterial(link, physics, gameplay));
                indices[key] = index;
            }
            return index;
        }

        public CPlugSolid2Model.Material[] ToArray() => [.. materials];

        private static CPlugSolid2Model.Material UserMaterial(string link, CPlugSurface.MaterialId physics, CPlugSurface.GameplayId gameplay)
        {
            var instance = new CPlugMaterialUserInst();
            instance.CreateChunk<CPlugMaterialUserInst.Chunk090FD000>().Version = UserInstanceVersion;
            instance.CreateChunk<CPlugMaterialUserInst.Chunk090FD001>().Version = UserInstanceTilingVersion;
            instance.CreateChunk<CPlugMaterialUserInst.Chunk090FD002>();
            instance.IsUsingGameMaterial = true;
            instance.MaterialName = "";
            instance.Model = "";
            instance.BaseTexture = "";
            instance.Link = link;
            instance.SurfacePhysicId = physics;
            instance.SurfaceGameplayId = gameplay;
            instance.TextureSizeInMeters = TextureSizeInMeters;
            instance.Csts = [];
            instance.Color = [];
            instance.UvAnims = [];
            instance.UserTextures = [];
            instance.HidingGroup = "";
            return new CPlugSolid2Model.Material { MaterialName = "", MaterialUserInst = instance };
        }
    }
}
