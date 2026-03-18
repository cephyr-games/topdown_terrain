using System.Collections.Generic;
using Godot;

namespace TopdownTerrain.Graph;

[Tool]
[GlobalClass]
// Fills a random amount of rooms, erasing them from generation.
public partial class FilledRoomFeature : RoomGraphFeature
{
    [Export(PropertyHint.Range, "0,1,")]
    public double Chance = 1.0;

    [Export]
    public IntRange Amount = new IntRange(1, 1);

    public override void ApplyFeature(RoomGraph graph, RandomNumberGenerator rng)
    {
        if (Chance == 0 || rng.Randf() > Chance)
            return;
        int count = Amount.GetUniform(rng);
        for (int i = 0; i < count; i++)
        {
            var successful = FillRoom(graph, rng);
            if (!successful)
                return;
        }
    }

    private bool FillRoom(RoomGraph graph, RandomNumberGenerator rng)
    {
        var rooms = new List<Rect2I>(graph.Rooms);
        rooms.Shuffle(rng);
        foreach (var room in graph.Rooms)
        {
            if (graph.CanRemoveRoom(room))
            {
                graph.RemoveRoom(room);
                return true;
            }
        }
        return false;
    }
}
