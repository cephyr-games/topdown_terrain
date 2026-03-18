using Godot;

namespace TopdownTerrain.Material;

[Tool]
[GlobalClass]
// Defines a material for a wall.
public partial class WallMaterial : Resource
{
    [Export]
    public TerrainTile Top = null!;

    [Export]
    public TerrainTile Outline = null!;

    [Export]
    public RandomTile Side = null!;

    [Export]
    public RandomTile BrokenSide = null!;

    [Export]
    public RandomTile BrokenFloor = null!;

    // Read and store the weights defined in the TileSet for materials that use those.
    public void ReadWeights(TileSet tileSet)
    {
        Side.ReadWeights(tileSet);
    }
}
