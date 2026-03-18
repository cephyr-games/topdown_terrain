using Godot;
using TopdownTerrain.Graph;
using TopdownTerrain.Material;

namespace TopdownTerrain.Room.Feature;

[Tool]
[GlobalClass]
// Adds a uniform floor to the room.
public partial class SimpleFloorFeature : RoomFeature
{
    [Export]
    public FloorMaterial Material = null!;

    public override Rect2I ApplyFeature(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    )
    {
        terrain.BufferFloor(room, Material);
        return room;
    }
}
