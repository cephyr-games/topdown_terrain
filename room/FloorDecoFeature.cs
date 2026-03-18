using Godot;
using TopdownTerrain.Graph;
using TopdownTerrain.Material;

namespace TopdownTerrain.Room.Feature;

[Tool]
[GlobalClass]
public partial class FloorDecoFeature : RoomFeature
{
    [Export]
    public FloorMaterial Material = null!;

    [Export]
    public IntRange Size = new IntRange(1, 8);

    [Export]
    public IntRange Count = new IntRange(3, 8);

    [Export(PropertyHint.Range, "0,1,")]
    public double Density = 1;

    public override Rect2I ApplyFeature(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    )
    {
        int remaining = Count.GetUniform(rng);
        while (remaining > 0)
        {
            remaining--;
            int x = Size.GetUniform(rng);
            int y = Size.GetUniform(rng);
            var tile = new Rect2I(
                room.Position.X + rng.RandiRange(0, room.Size.X - x),
                room.Position.Y + rng.RandiRange(0, room.Size.Y - y),
                x,
                y
            );
            terrain.BufferFloor(tile, Material, Density, rng);
        }
        return room;
    }
}
