using Godot;

namespace TopdownTerrain.Material;

[Tool]
[GlobalClass]
// References a specific Terrain of a TileSet.
public partial class TerrainTile : Resource
{
    [Export]
    public int TerrainSet;

    [Export]
    public int Terrain;
}
