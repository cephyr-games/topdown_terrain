using Godot;
using TopdownTerrain.Graph;

namespace TopdownTerrain.Room.Feature;

[Tool]
[GlobalClass]
// Associates a weight with a RoomFeature.
public partial class WeightedRoomFeature : RoomFeature
{
    [Export]
    public float Weight = 1f;

    [Export]
    public RoomFeature Feature = null!;

    public override Rect2I ApplyFeature(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    )
    {
        return Feature.ApplyFeature(terrain, graph, room, rng);
    }
}
