using Godot;
using TopdownTerrain.Graph;

namespace TopdownTerrain.Room.Feature;

[Tool]
[GlobalClass]
// Stores a list of RoomFeatures and applies all of them in order.
public partial class RoomFeatureCollection : RoomFeature
{
    [Export]
    public Godot.Collections.Array<RoomFeature> Features = null!;

    public override Rect2I ApplyFeature(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    )
    {
        foreach (var feature in Features)
            room = feature.ApplyFeature(terrain, graph, room, rng);
        return room;
    }
}
