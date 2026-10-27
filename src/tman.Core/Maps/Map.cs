using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;
using tman.Core;
using tman.Positioning;

namespace tman.Maps
{
    public sealed class Map
    {
        private static readonly Dictionary<Car, string> CarModels = new()
        {
            [Car.Desert] = "CarDesert",
            [Car.Rally] = "CarRally",
            [Car.Snow] = "CarSnow",
            [Car.Sport] = "CarSport",
        };

        static Map()
        {
            GBX.NET.Gbx.LZO = new Lzo();
        }

        internal CGameCtnChallenge Gbx { get; }

        internal Map(CGameCtnChallenge gbx)
        {
            Gbx = gbx;
        }

        public static Map Load(string path) => new(GBX.NET.Gbx.ParseNode<CGameCtnChallenge>(path, new() { SafeSkippableChunks = true }));

        public void Save(string path) => Gbx.Save(path);

        public string Name
        {
            get => Gbx.MapName;
            set => Gbx.MapName = value;
        }

        public string Uid => Gbx.MapUid;

        public Int3 Size => Gbx.Size;

        public string Decoration => Gbx.Decoration.Id;

        public Collections Collection => Gbx.GetEnvironment() is "Stadium2020" ? Collections.Stadium : Enum.Parse<Collections>(Gbx.GetEnvironment());

        public int GroundRow => Collection switch
        {
            Collections.RedIsland or Collections.WhiteShore => 15,
            Collections.BlueBay or Collections.GreenCoast => 5,
            _ => Gbx.DecoBaseHeightOffset
        };

        public Car Car
        {
            set => Gbx.PlayerModel = new Ident(CarModels[value], 10003, "");
        }

        public bool Supports(Block block) => block.Collections.HasFlag(Collection);

        public bool Supports(Item item) => item.Collections.HasFlag(Collection);

        public IEnumerable<Placement> Placements =>
            (Gbx.Blocks ?? []).Select(block => new Placement(block))
                .Concat((Gbx.AnchoredObjects ?? []).Select(item => new Placement(item)));

        public IEnumerable<Placement> Terrain =>
            (Gbx.Blocks ?? []).Concat(Gbx.BakedBlocks ?? []).Where(block => !block.IsFree).Select(block => new Placement(block));

        public List<Placement> Of(Selector selector) => Placements.Where(selector.Matches).ToList();

        public List<Placement> Waypoints(Waypoint waypoint) =>
            Placements.Where(placement => placement.IsWaypoint && placement.Waypoint == waypoint).ToList();

        public void PlaceRelative(Placement source, Item model, Vec3 offset = default)
        {
            var position = source.Pose(this);
            position.Move(offset);
            Gbx.PlaceAnchoredObject(model.Ident(this), position.Coords, position.YawPitchRoll, Vec3.Zero);
        }

        public void PlaceRelative(Placement source, Block model, Vec3 offset = default)
        {
            var position = source.Pose(this);
            position.Move(offset);
            Gbx.PlaceBlock(new CGameCtnBlock
            {
                Name = model.Id,
                Coord = source.Cell,
                Direction = Direction.North,
                IsFree = true,
                AbsolutePositionInMap = position.Coords,
                YawPitchRoll = position.YawPitchRoll
            });
        }

        public void Place(Item model, Vec3 position, Vec3 yawPitchRoll = default) =>
            Gbx.PlaceAnchoredObject(model.Ident(this), position, yawPitchRoll, Vec3.Zero);

        public void Replace(Placement source, Block model, DirectionOffset rotation)
        {
            var block = source.Block!;
            Gbx.RemoveBlock(block);
            Gbx.PlaceBlock(new CGameCtnBlock
            {
                Name = model.Id,
                Coord = block.Coord,
                Direction = block.Direction.Turn(rotation),
                IsGround = block.IsGround,
                Variant = block.Variant,
                SubVariant = block.SubVariant,
                YawPitchRoll = block.YawPitchRoll,
                Color = block.Color,
                IsClip = block.IsClip,
                IsFree = block.IsFree,
                IsPillar = block.IsPillar,
                IsGhost = block.IsGhost,
                IsReplacement = block.IsReplacement
            });
        }

        public void Remove(Placement placement)
        {
            if (placement.Block is not null)
            {
                Gbx.RemoveBlock(placement.Block);
            }
            else
            {
                Gbx.AnchoredObjects?.Remove(placement.Item!);
            }
        }

        public void Embed(Item custom) => Embed(custom.FullPath());

        internal void Embed(string path)
        {
            Gbx.UpdateEmbeddedZipData(zip =>
            {
                var entry = zip.CreateEntry(Path.GetFileName(path));
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(path);
                fileStream.CopyTo(entryStream);
            });
        }

        public void RemoveValidation()
        {
            Gbx.AuthorTime = new(-1);
            Gbx.GoldTime = new(-1);
            Gbx.SilverTime = new(-1);
            Gbx.BronzeTime = new(-1);
            Gbx.ChallengeParameters?.RaceValidateGhost = null;
            if (Gbx.Xml is not null)
            {
                var header = XElement.Parse(Gbx.Xml);
                header.Element("desc")?.SetAttributeValue("validated", "0");
                foreach (var time in new[] { "bronze", "silver", "gold", "authortime" })
                {
                    header.Element("times")?.SetAttributeValue(time, "-1");
                }
                Gbx.Xml = header.ToString(SaveOptions.DisableFormatting);
            }
        }

        public void RemoveLightmap()
        {
            Gbx.HasLightmaps = false;
            Gbx.LightmapCache = null;
            Gbx.LightmapFrames = null;
            Gbx.LightmapCacheSmall = null;
            Gbx.LightmapCacheData = null;
        }

        public void GenerateUid() => Gbx.GenerateMapUid();
    }
}
