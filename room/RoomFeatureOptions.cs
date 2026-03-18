using System.Collections.Generic;
using Godot;
using TopdownTerrain.Graph;

namespace TopdownTerrain.Room.Feature;

[Tool]
[GlobalClass]
// Applies one of RoomFeature from the list of features and associated weights.
public partial class RoomFeatureOptions : RoomFeature
{
    [Export]
    public Godot.Collections.Array<WeightedRoomFeature> Features = null!;

    public override Rect2I ApplyFeature(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    )
    {
        var weights = new List<float>();
        foreach (var feature in Features)
            weights.Add(feature.Weight);
        int index = (int)rng.RandWeighted(weights.ToArray());
        return Features[index].ApplyFeature(terrain, graph, room, rng);
    }
}
