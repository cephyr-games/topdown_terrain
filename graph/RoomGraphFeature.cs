using Godot;

namespace TopdownTerrain.Graph;

[Tool]
[GlobalClass]
// Abstract base class for manipulation feature acting on the RoomGraph.
public abstract partial class RoomGraphFeature : Resource
{
    public abstract void ApplyFeature(RoomGraph graph, RandomNumberGenerator rng);
}
