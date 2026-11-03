using System.Collections.Generic;
using GBX.NET.Engines.Plug;

namespace Tman.Core.Surfaces.Building
{
    internal sealed class MergedMaterials
    {
        private const int UserInstanceVersion = 11;

        private const int UserInstanceTilingVersion = 5;

        private const float TextureSizeInMeters = 1;

        private const string TintConstant = "TargetColor";

        private const string RealConstant = "Real";

        private readonly List<CPlugSolid2Model.Material> materials = [];

        private readonly Dictionary<(string Link, CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay, Tint Tint), int> indices = [];

        public int IndexOf(string link, CPlugSurface.MaterialId physics, CPlugSurface.GameplayId gameplay) => IndexOf(link, physics, gameplay, Tint.None);

        public int IndexOf(string link, CPlugSurface.MaterialId physics, CPlugSurface.GameplayId gameplay, Tint tint)
        {
            var key = (link, physics, gameplay, tint);
            if (!indices.TryGetValue(key, out var index))
            {
                index = materials.Count;
                materials.Add(UserMaterial(link, physics, gameplay, tint));
                indices[key] = index;
            }
            return index;
        }

        public CPlugSolid2Model.Material[] ToArray() => [.. materials];

        private static CPlugSolid2Model.Material UserMaterial(string link, CPlugSurface.MaterialId physics, CPlugSurface.GameplayId gameplay, Tint tint)
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
            instance.Csts = tint == Tint.None ? [] : [new CPlugMaterialUserInst.Cst { U01 = TintConstant, U02 = RealConstant, U03 = tint.Constants.Length }];
            instance.Color = tint.Constants;
            instance.UvAnims = [];
            instance.UserTextures = [];
            instance.HidingGroup = "";
            return new CPlugSolid2Model.Material { MaterialName = "", MaterialUserInst = instance };
        }
    }
}
