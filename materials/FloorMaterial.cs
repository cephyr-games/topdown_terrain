using Godot;

namespace TopdownTerrain.Material;

[Tool]
[GlobalClass]
// Defines a material for the floor.
public partial class FloorMaterial : Resource
{
    [Export]
    public RandomTile Tile = null!;

    [Export]
    public RandomTile BrokenTile = null!;

    // Read and store the weights defined in the TileSet for materials that use those.
    public void ReadWeights(TileSet tileSet)
    {
        Tile.ReadWeights(tileSet);
    }
}
