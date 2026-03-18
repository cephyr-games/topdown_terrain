using System.Collections.Generic;
using Godot;
using TopdownTerrain.Material;

namespace TopdownTerrain;

[Tool]
[GlobalClass]
// Displays and manages terrain consisting of floor and wall materials.
public partial class TopdownTerrain : Node2D
{
    public static readonly Vector2I Size = new Vector2I(128, 128);

    [Export]
    public TileMapLayer WallTop = null!;

    [Export]
    public TileMapLayer WallOutline = null!;

    [Export]
    public TileMapLayer WallSide = null!;

    [Export]
    public TileMapLayer Floor = null!;

    private TerrainMap _map = new TerrainMap(TopdownTerrain.Size);
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private HashSet<WallMaterial> _wallMaterialSet = new HashSet<WallMaterial>();
    private HashSet<FloorMaterial> _floorMaterialSet = new HashSet<FloorMaterial>();

    public void SetSeed(ulong seed)
    {
        _rng.Seed = seed;
    }

    // Clear floor, wall and buffer.
    public void Clear()
    {
        _map = new TerrainMap(TopdownTerrain.Size);
        _wallMaterialSet.Clear();
        _floorMaterialSet.Clear();
        WallTop.Clear();
        WallOutline.Clear();
        WallSide.Clear();
        Floor.Clear();
    }

    // Write the current buffered state to the TileMapLayers.
    public void FlushBuffer()
    {
        var walls = new Dictionary<WallMaterial, List<Vector2I>>();
        var floors = new Dictionary<FloorMaterial, List<Vector2I>>();
        for (int x = 0; x < TopdownTerrain.Size.X; x++)
        {
            for (int y = 0; y < TopdownTerrain.Size.Y; y++)
            {
                var pos = new Vector2I(x, y);
                var f = _map.Floor(pos);
                if (f != null)
                {
                    if (!floors.ContainsKey(f))
                        floors[f] = new List<Vector2I>();
                    floors[f].Add(pos);
                }
                var w = _map.Wall(pos);
                if (w != null)
                {
                    if (!walls.ContainsKey(w))
                        walls[w] = new List<Vector2I>();
                    walls[w].Add(pos);
                }
            }
        }
        foreach (WallMaterial material in walls.Keys)
            PlaceWall(material, walls[material]);
        foreach (FloorMaterial material in floors.Keys)
            PlaceFloor(material, floors[material]);
    }

    // Buffer the wall at the given position.
    public void BufferWall(Vector2I position, WallMaterial? material)
    {
        _map.PlaceWall(position, material);
    }

    // Buffer the wall in the given rectangle.
    public void BufferWall(Rect2I positions, WallMaterial? material)
    {
        for (int x = 0; x < positions.Size.X; x++)
        {
            for (int y = 0; y < positions.Size.Y; y++)
            {
                BufferWall(
                    new Vector2I(positions.Position.X + x, positions.Position.Y + y),
                    material
                );
            }
        }
    }

    // Buffer the wall within the given rectangle, hollow with the given wall thickness.
    public void BufferWallHollow(Rect2I rect, int thickness, WallMaterial? material)
    {
        if (thickness == 0)
            return;
        for (int x = 0; x < rect.Size.X; x++)
        {
            for (int y = 0; y < thickness; y++)
            {
                BufferWall(new Vector2I(rect.Position.X + x, rect.Position.Y + y), material);
                BufferWall(
                    new Vector2I(
                        rect.Position.X + x,
                        rect.Position.Y + rect.Size.Y - thickness + y
                    ),
                    material
                );
            }
        }
        for (int y = 0; y < rect.Size.Y - 2 * thickness; y++)
        {
            for (int x = 0; x < thickness; x++)
            {
                BufferWall(
                    new Vector2I(rect.Position.X + x, rect.Position.Y + thickness + y),
                    material
                );
                BufferWall(
                    new Vector2I(
                        rect.Position.X + rect.Size.X - thickness + x,
                        rect.Position.Y + thickness + y
                    ),
                    material
                );
            }
        }
    }

    // Buffer the floor at the given position.
    public void BufferFloor(Vector2I position, FloorMaterial? material)
    {
        _map.PlaceFloor(position, material);
    }

    // Buffer the floor within the given rectangle.
    public void BufferFloor(Rect2I positions, FloorMaterial? material)
    {
        for (int x = 0; x < positions.Size.X; x++)
        {
            for (int y = 0; y < positions.Size.Y; y++)
            {
                BufferFloor(
                    new Vector2I(positions.Position.X + x, positions.Position.Y + y),
                    material
                );
            }
        }
    }

    // Buffer the floor within the given rectangle, each tile affected with the given Chance.
    public void BufferFloor(
        Rect2I positions,
        FloorMaterial? material,
        double Chance,
        RandomNumberGenerator rng
    )
    {
        if (Chance <= 0)
            return;
        if (Chance >= 1)
        {
            BufferFloor(positions, material);
            return;
        }
        for (int x = 0; x < positions.Size.X; x++)
        {
            for (int y = 0; y < positions.Size.Y; y++)
            {
                if (rng.Randf() > Chance)
                    continue;
                BufferFloor(
                    new Vector2I(positions.Position.X + x, positions.Position.Y + y),
                    material
                );
            }
        }
    }

    // Destroy the cell at the given positon, turning wall and floor into broken variants.
    public void BreakCell(Vector2I position) { }

    // Buffer the wall at the given positions to the given material.
    private void PlaceWall(WallMaterial material, List<Vector2I> positions)
    {
        if (!_wallMaterialSet.Contains(material))
        {
            _wallMaterialSet.Add(material);
            material.ReadWeights(WallSide.TileSet);
        }
        foreach (Vector2I at in positions)
        {
            WallSide.SetCell(at, material.Side.Source, material.Side.PickAtlasCoords(_rng));
        }
        var posArray = new Godot.Collections.Array<Vector2I>(positions);
        WallTop.SetCellsTerrainConnect(posArray, material.Top.TerrainSet, material.Top.Terrain);
        WallOutline.SetCellsTerrainConnect(
            posArray,
            material.Outline.TerrainSet,
            material.Outline.Terrain
        );
    }

    // Buffer the floor at the given positions to the given material.
    private void PlaceFloor(FloorMaterial material, List<Vector2I> positions)
    {
        if (!_floorMaterialSet.Contains(material))
        {
            _floorMaterialSet.Add(material);
            material.ReadWeights(Floor.TileSet);
        }
        foreach (Vector2I at in positions)
        {
            Floor.SetCell(at, material.Tile.Source, material.Tile.PickAtlasCoords(_rng));
        }
    }

    // Stores which Terrain is set at each grid cell.
    private class TerrainMap
    {
        // The size of the grid.
        public Vector2I Size;

        // Maps WallMaterials to an id to decrease memory cost.
        private Dictionary<WallMaterial, ushort> _wallMapping =
            new Dictionary<WallMaterial, ushort>();

        // Used WallMaterials in order of assigned ids.
        private List<WallMaterial> _wallMaterials = new List<WallMaterial>();

        // Maps FloorMaterials to an id to decrease memory cost.
        private Dictionary<FloorMaterial, ushort> _floorMapping =
            new Dictionary<FloorMaterial, ushort>();

        // Used FloorMaterials in order of assigned ids.
        private List<FloorMaterial> _floorMaterials = new List<FloorMaterial>();

        // The id that will be given to the next encountered WallMaterial.
        private ushort _wallMapNext = 1;

        // The id that will be given to the next encountered FloorMaterial.
        private ushort _floorMapNext = 1;

        // Grid of WallMaterial ids.
        private ushort[,] _wall;

        // Grid of FloorMaterial ids.
        private ushort[,] _floor;

        public TerrainMap(Vector2I size)
        {
            Size = size;
            _floor = new ushort[size.X, size.Y];
            _wall = new ushort[size.X, size.Y];
        }

        public void PlaceFloor(Vector2I position, FloorMaterial? material)
        {
            if (material is null)
            {
                _floor[position.X, position.Y] = 0;
                return;
            }
            if (!_floorMapping.ContainsKey(material))
            {
                _floorMapping[material] = _floorMapNext;
                _floorMaterials.Add(material);
                _floorMapNext += 1;
            }
            _floor[position.X, position.Y] = _floorMapping[material];
        }

        public void PlaceWall(Vector2I position, WallMaterial? material)
        {
            if (material is null)
            {
                _wall[position.X, position.Y] = 0;
                return;
            }
            if (!_wallMapping.ContainsKey(material))
            {
                _wallMapping[material] = _wallMapNext;
                _wallMaterials.Add(material);
                _wallMapNext += 1;
            }
            _wall[position.X, position.Y] = _wallMapping[material];
        }

        public FloorMaterial? Floor(Vector2I position)
        {
            ushort index = _floor[position.X, position.Y];
            if (index == 0)
                return null;
            return _floorMaterials[index - 1];
        }

        public WallMaterial? Wall(Vector2I position)
        {
            ushort index = _wall[position.X, position.Y];
            if (index == 0)
                return null;
            return _wallMaterials[index - 1];
        }
    }
}
