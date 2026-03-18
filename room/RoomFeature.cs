using Godot;
using TopdownTerrain.Graph;

namespace TopdownTerrain.Room.Feature;

[Tool]
[GlobalClass]
// Abstract base class for features manipulating room generation.
public abstract partial class RoomFeature : Resource
{
    // Apply this feature for the given room on the terrain.
    public abstract Rect2I ApplyFeature(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    );
}
