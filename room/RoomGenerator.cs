using System.Collections.Generic;
using Godot;
using TopdownTerrain.Graph;
using TopdownTerrain.Room.Feature;

namespace TopdownTerrain.Room;

[Tool]
[GlobalClass]
public partial class RoomGenerator : Resource
{
    [Export(PropertyHint.Range, "0,1,")]
    // Chance to apply this generator per room.
    public double Chance = 1.0;

    [Export]
    // Only apply to rooms that have an amount of connections in this range.
    public IntRange DoorAmount = new IntRange(0, 255);

    [Export]
    // Limit how often this generator will be applied, < 0 is unlimited.
    public int Limit = -1;

    [Export]
    // List of RoomFeatures to apply to the rooms.
    public Godot.Collections.Array<RoomFeature> Features =
        new Godot.Collections.Array<RoomFeature>();

    // Apply this generator on any amount of the given rooms.
    // Parameter rooms will store the ungenerated rooms post method call.
    public void Generate(
        TopdownTerrain terrain,
        RoomGraph graph,
        List<Rect2I> rooms,
        RandomNumberGenerator rng
    )
    {
        var count = 0;
        var pickedRooms = PickRooms(graph, rooms, rng);
        foreach (var room in pickedRooms)
        {
            if (Limit >= 0 && count >= Limit)
                return;
            rooms.Remove(room);
            GenerateRoom(terrain, graph, room, rng);
            count++;
        }
    }

    // From a list of ungenerated rooms, pick the ones to apply this generator to.
    // Default imlementation picks each room randomly controlled by the Chance property,
    // if they pass the DoorAmount guard.
    protected virtual List<Rect2I> PickRooms(
        RoomGraph graph,
        List<Rect2I> rooms,
        RandomNumberGenerator rng
    )
    {
        var result = new List<Rect2I>();
        if (Chance <= 0)
            return result;
        foreach (var room in rooms)
        {
            var doors = graph.GetDoors(room);
            if (DoorAmount.InRange(doors.Count) && rng.Randf() <= Chance)
                result.Add(room);
        }
        return result;
    }

    // Generate the given room.
    // Default implementation applies all RoomFeatures in order.
    protected virtual void GenerateRoom(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    )
    {
        foreach (var feature in Features)
            room = feature.ApplyFeature(terrain, graph, room, rng);
    }
}
