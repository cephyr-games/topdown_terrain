using Godot;

namespace TopdownTerrain.Material;

[Tool]
[GlobalClass]
// References a rectangle of Cells on a specific TileAtlas.
public partial class RandomTile : Resource
{
    [Export]
    public int Source;

    [Export]
    public Rect2I Area;

    private float[]? weights;

    public Vector2I PickAtlasCoords(RandomNumberGenerator rng)
    {
        if (weights == null)
        {
            GD.PrintErr("Missing random tile weights, call ReadWeights first");
            return Area.Position;
        }
        int index = (int)rng.RandWeighted(weights);
        Vector2I coords = new Vector2I(
            Area.Position.X + index % Area.Size.X,
            Area.Position.Y + index / Area.Size.X
        );
        return coords;
    }

    public void ReadWeights(TileSet tileSet)
    {
        weights = new float[Area.Size.X * Area.Size.Y];
        int i = 0;
        for (int y = 0; y < Area.Size.Y; y++)
        {
            for (int x = 0; x < Area.Size.X; x++)
            {
                Vector2I at = new Vector2I(x + Area.Position.X, y + Area.Position.Y);
                TileData data = ((TileSetAtlasSource)tileSet.GetSource(Source)).GetTileData(at, 0);
                weights[i] = data.Probability;
                i += 1;
            }
        }
    }
}
