using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.LZO;
using Tman.Inventory;

namespace Tman.Maps
{
    public sealed class Map
    {
        private const int AdditionalVariantMask = 0x7F << 21;

        private static readonly Dictionary<Waypoint, string> WaypointTags = new()
        {
            [Waypoint.Start] = "Spawn",
            [Waypoint.Checkpoint] = "Checkpoint",
            [Waypoint.Finish] = "Goal",
            [Waypoint.StartFinish] = "StartFinish",
        };

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

        public int Laps
        {
            get => Gbx.NbLaps;
            set
            {
                Gbx.IsLapRace = true;
                Gbx.NbLaps = value;
            }
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

        public Placement PlaceRelative(Placement source, Item model, Vec3 offset = default)
        {
            var position = source.Pose(this);
            position.Move(offset);
            return Place(model, position.Coords, position.YawPitchRoll);
        }

        public Placement PlaceRelative(Placement source, Block model, Vec3 offset = default)
        {
            var position = source.Pose(this);
            position.Move(offset);
            var block = new CGameCtnBlock
            {
                Name = model.Id,
                Coord = source.Cell,
                Direction = Direction.North,
                IsFree = true,
                AbsolutePositionInMap = position.Coords,
                YawPitchRoll = position.YawPitchRoll,
                WaypointSpecialProperty = WaypointProperty(null, model.Waypoint)
            };
            Gbx.PlaceBlock(block);
            return new Placement(block);
        }

        public Placement Place(Item model, Vec3 position, Vec3 yawPitchRoll = default)
        {
            var item = Gbx.PlaceAnchoredObject(model.Ident(this), position, yawPitchRoll, Vec3.Zero);
            item.WaypointSpecialProperty = WaypointProperty(null, model.Waypoint);
            return new Placement(item);
        }

        public Placement PlaceLike(Placement source, Waypoint kind) => source.IsBlock
            ? PlaceBlock(Copy(source.Block!, Twin(source.BlockModel, kind, source.Id), Turn(source.Waypoint, kind), null, true))
            : PlaceLike(source, Twin(source.ItemModel, kind, source.Id), HalfTurn(source, kind));

        public Placement PlaceLike(Placement source, Block model) => PlaceBlock(Copy(source.Block!, model, DirectionOffset.None, null, false));

        public Placement PlaceLike(Placement source, Item model) => PlaceLike(source, model, 0);

        public Placement Replace(Placement source, Waypoint kind)
        {
            if (source.IsBlock)
            {
                return Replace(source, Twin(source.BlockModel, kind, source.Id), Turn(source.Waypoint, kind), true);
            }
            var item = source.Item!;
            item.YawPitchRoll += new Vec3(HalfTurn(source, kind), 0, 0);
            return Replace(source, Twin(source.ItemModel, kind, source.Id));
        }

        public Placement Replace(Placement source, Block model, DirectionOffset rotation) => Replace(source, model, rotation, false);

        private Placement Replace(Placement source, Block model, DirectionOffset rotation, bool sameFamily)
        {
            var block = source.Block!;
            Gbx.RemoveBlock(block);
            return PlaceBlock(Copy(block, model, rotation, block.WaypointSpecialProperty, sameFamily));
        }

        public Placement Replace(Placement source, Item model)
        {
            var item = source.Item!;
            item.ItemModel = model.Ident(this);
            item.WaypointSpecialProperty = WaypointProperty(item.WaypointSpecialProperty, model.Waypoint);
            return source;
        }

        public void Link(IReadOnlyCollection<Placement> checkpoints)
        {
            if (checkpoints.Count < 2)
            {
                return;
            }
            var order = Placements.Where(placement => placement.IsWaypoint && placement.Property!.Tag == "LinkedCheckpoint").Select(placement => placement.Property!.Order).DefaultIfEmpty(0).Max() + 1;
            foreach (var checkpoint in checkpoints)
            {
                var property = checkpoint.Property ?? throw new InvalidOperationException($"{checkpoint.Id} is not a waypoint");
                property.Tag = "LinkedCheckpoint";
                property.Order = order;
            }
        }

        private Placement PlaceBlock(CGameCtnBlock block)
        {
            Gbx.PlaceBlock(block);
            return new Placement(block);
        }

        private Placement PlaceLike(Placement source, Item model, float yaw)
        {
            var item = source.Item!;
            var copy = Gbx.PlaceAnchoredObject(model.Ident(this), item.AbsolutePositionInMap, item.YawPitchRoll + new Vec3(yaw, 0, 0), item.PivotPosition);
            copy.BlockUnitCoord = item.BlockUnitCoord;
            copy.AnchorTreeId = item.AnchorTreeId;
            copy.Flags = item.Flags;
            copy.Scale = item.Scale;
            copy.PackDesc = item.PackDesc;
            copy.Color = item.Color;
            copy.WaypointSpecialProperty = WaypointProperty(null, model.Waypoint);
            return new Placement(copy);
        }

        private static CGameCtnBlock Copy(CGameCtnBlock block, Block model, DirectionOffset rotation, CGameWaypointSpecialProperty? property, bool sameFamily)
        {
            var copy = new CGameCtnBlock
            {
                Name = model.Id,
                Coord = block.Coord,
                Direction = block.Direction.Turn(rotation),
                Flags = sameFamily ? block.Flags : block.Flags & ~AdditionalVariantMask,
                AbsolutePositionInMap = block.AbsolutePositionInMap,
                YawPitchRoll = block.YawPitchRoll,
                Color = block.Color,
                LightmapQuality = block.LightmapQuality
            };
            if (block.Skin is not null)
            {
                copy.Skin = block.Skin;
            }
            copy.WaypointSpecialProperty = WaypointProperty(property, model.Waypoint);
            return copy;
        }

        private static Block Twin(Block? model, Waypoint kind, string id) =>
            model?.As(kind) ?? throw new InvalidOperationException($"{id} has no {kind} twin");

        private static Item Twin(Item? model, Waypoint kind, string id) =>
            model?.As(kind) ?? throw new InvalidOperationException($"{id} has no {kind} twin");

        private static float HalfTurn(Placement source, Waypoint kind) => Items.HalfTurned.Contains((source.Id, kind)) ? MathF.PI : 0;

        private static DirectionOffset Turn(Waypoint from, Waypoint to) =>
            (from, to) is (Waypoint.Start, Waypoint.Finish) or (Waypoint.Finish, Waypoint.Start) ? DirectionOffset.Half : DirectionOffset.None;

        private static CGameWaypointSpecialProperty? WaypointProperty(CGameWaypointSpecialProperty? current, Waypoint kind)
        {
            if (kind == Waypoint.None)
            {
                return null;
            }
            var property = current;
            if (property is null)
            {
                property = new CGameWaypointSpecialProperty();
                property.CreateChunk<CGameWaypointSpecialProperty.Chunk2E009000>().Version = 2;
                property.CreateChunk<CGameWaypointSpecialProperty.Chunk2E009001>();
            }
            property.Tag = WaypointTags[kind];
            return property;
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
                var name = Path.GetFileName(path);
                if (zip.GetEntry(name) is not null)
                {
                    return;
                }
                var entry = zip.CreateEntry(name);
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
