using Godot;
using TopdownTerrain.Graph;
using TopdownTerrain.Material;

namespace TopdownTerrain.Room.Feature;

[Tool]
[GlobalClass]
// Add some colums made of the wall material to the room.
public partial class ColumnFeature : RoomFeature
{
    [Export]
    public WallMaterial Material = null!;

    [Export]
    public IntRange Size = new IntRange(1, 2);

    [Export]
    public int OuterPadding = 1;

    [Export]
    public int InnerPadding = 1;

    [Export]
    public Vector2I PartitionSize = new Vector2I(3, 3);

    [Export(PropertyHint.Range, "0,1,")]
    public double Density = 1;

    public override Rect2I ApplyFeature(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    )
    {
        var bsp = new BSPTree(
            room.Grow(-OuterPadding).GrowIndividual(0, 0, InnerPadding, InnerPadding),
            PartitionSize,
            rng,
            0
        );
        var leaves = bsp.Leaves();
        foreach (var area in leaves)
        {
            if (Density == 0 || rng.Randf() > Density)
            {
                continue;
            }
            int size = int.Min(
                Size.GetUniform(rng),
                int.Min(area.Rect.Size.X, area.Rect.Size.Y) - InnerPadding
            );
            int offX = rng.RandiRange(0, area.Rect.Size.X - InnerPadding - size);
            int offY = rng.RandiRange(0, area.Rect.Size.Y - InnerPadding - size);
            var column = new Rect2I(area.Rect.Position.X, area.Rect.Position.Y, size, size);
            terrain.BufferWall(column, Material);
        }
        return room;
    }
}
