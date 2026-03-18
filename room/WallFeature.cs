using Godot;
using TopdownTerrain.Graph;
using TopdownTerrain.Material;

namespace TopdownTerrain.Room.Feature;

[Tool]
[GlobalClass]
// Creates a wall around the room, with cutouts for the doors defined in graph.
public partial class WallFeature : RoomFeature
{
    [Export]
    public IntRange Thickness = new IntRange(1, 1);

    [Export]
    public WallMaterial Material = null!;

    public override Rect2I ApplyFeature(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    )
    {
        int thickness = Thickness.GetUniform(rng);
        terrain.BufferWallHollow(room, thickness, Material);
        // remove walls where doors are supposed to be
        var doors = graph.GetDoors(room);
        foreach (var door in doors)
        {
            Rect2I tmp;
            if (door.Size.X > 1)
            {
                if (door.Position.Y == room.Position.Y)
                    tmp = door.GrowSide(Side.Bottom, thickness - 1);
                else
                    tmp = door.GrowSide(Side.Top, thickness - 1);
            }
            else
            {
                if (door.Position.X == room.Position.X)
                    tmp = door.GrowSide(Side.Right, thickness - 1);
                else
                    tmp = door.GrowSide(Side.Left, thickness - 1);
            }
            terrain.BufferWall(tmp, null);
        }
        return room.Grow(-thickness);
    }
}
