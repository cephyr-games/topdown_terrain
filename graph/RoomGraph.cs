using System.Collections.Generic;
using Godot;

namespace TopdownTerrain.Graph;

[Tool]
// A graph of all rooms and their connections.
public class RoomGraph
{
    // A List of all rooms.
    public List<Rect2I> Rooms { get; } = new List<Rect2I>();

    // Maps each room on a List of all connected rooms.
    public Dictionary<Rect2I, List<Rect2I>> RoomConnections { get; } =
        new Dictionary<Rect2I, List<Rect2I>>();

    // A List of all Doors.
    public List<Rect2I> Doors { get; } = new List<Rect2I>();

    // A List of all rooms that should be entirely filled with wall.
    public List<Rect2I> FilledRooms { get; } = new List<Rect2I>();

    // Minimum distance to the edge of each room to place a door.
    public int DoorPadding;

    // Create the graph from the leaf nodes of the BSP and adds doors with the given parameters.
    // Ensures that every room is reachable from every other room.
    public RoomGraph(
        BSPTree bsp,
        IntRange doorWidth,
        int doorChancePercent,
        int doorPadding,
        RandomNumberGenerator rng
    )
    {
        DoorPadding = doorPadding;
        List<BSPTree> open = new List<BSPTree>();
        open.Add(bsp);
        while (open.Count > 0)
        {
            BSPTree current = open[0];
            open.RemoveAt(0);
            if (current.Left != null && current.Right != null)
            {
                open.Add(current.Left);
                open.Add(current.Right);
            }
            else
            {
                AddRoom(current.Rect, doorWidth.From + 2 * DoorPadding);
            }
        }
        RemoveConnections(100 - doorChancePercent, rng);
        GenerateDoors(doorWidth, rng);
    }

    // Fint all doors that a given room is connected with.
    public List<Rect2I> GetDoors(Rect2I room)
    {
        var result = new List<Rect2I>();
        foreach (Rect2I door in Doors)
        {
            if (door.Intersects(room))
            {
                result.Add(door);
            }
        }
        return result;
    }

    // An inclusive path of rooms between from and to or null if no such path exists.
    public List<Rect2I>? FindPath(Rect2I from, Rect2I to)
    {
        List<Rect2I> result = new List<Rect2I>();
        // Done if already at goal.
        if (from == to)
        {
            result.Add(from);
            return result;
        }
        // Maps each room on the previous room.
        Dictionary<Rect2I, Rect2I?> parent = new Dictionary<Rect2I, Rect2I?>();
        parent[from] = null;
        // Rooms that will be checked next in.
        Queue<Rect2I> open = new Queue<Rect2I>();
        open.Enqueue(from);
        // Breadth First Search
        while (open.Count > 0)
        {
            Rect2I current = open.Dequeue();
            foreach (Rect2I neighbor in RoomConnections[current])
            {
                if (!parent.ContainsKey(neighbor))
                {
                    parent[neighbor] = current;
                    if (neighbor == to)
                        return BuildPath(to, parent);
                    else
                        open.Enqueue(neighbor);
                }
            }
        }
        // No path found.
        return null;
    }

    // Check if removing the given room can be done without splitting the graph.
    public bool CanRemoveRoom(Rect2I room)
    {
        var connections = new List<Rect2I>(RoomConnections[room]);
        // Room is leaf, trivially removable.
        if (connections.Count <= 1)
            return true;
        // Remove all connections.
        foreach (var other in connections)
            RemoveConnection(room, other);
        // Check if any connected room can no longer be reached.
        bool valid = true;
        foreach (var other in connections)
        {
            if (FindPath(connections[0], other) == null)
            {
                valid = false;
                break;
            }
        }
        // Restore all connections.
        foreach (var other in connections)
            AddConnection(room, other);
        // Give result.
        return valid;
    }

    // Delete the given room.
    public void RemoveRoom(Rect2I room)
    {
        // Delete all connections with this room.
        var connections = new List<Rect2I>(RoomConnections[room]);
        foreach (var other in connections)
            RemoveConnection(room, other);
        // Delete all doors connected to this room.
        foreach (var door in GetDoors(room))
            Doors.Remove(door);
        // Remove the room.
        Rooms.Remove(room);
        // Store that this area needs to be filled when generating.
        FilledRooms.Add(room);
    }

    // Generate doors at every connections.
    private void GenerateDoors(IntRange doorWidth, RandomNumberGenerator rng)
    {
        HashSet<(Rect2I, Rect2I)> closed = new HashSet<(Rect2I, Rect2I)>();
        foreach (Rect2I room in Rooms)
        {
            foreach (Rect2I other in RoomConnections[room])
            {
                if (closed.Contains((other, room)))
                    continue;
                closed.Add((room, other));
                Rect2I overlap = room.Intersection(other);
                // horizontal door
                if (overlap.Size.X > 1)
                {
                    int totalSize = overlap.Size.X - 2 * DoorPadding;
                    int doorSize = int.Min(totalSize, doorWidth.GetUniform(rng));
                    int doorOffset = DoorPadding + rng.RandiRange(0, totalSize - doorSize);
                    Rect2I door = new Rect2I(
                        overlap.Position.X + doorOffset,
                        overlap.Position.Y,
                        doorSize,
                        1
                    );
                    Doors.Add(door);
                }
                // vertical door
                else
                {
                    int totalSize = overlap.Size.Y - 2 * DoorPadding;
                    int doorSize = int.Min(totalSize, doorWidth.GetUniform(rng));
                    int doorOffset = DoorPadding + rng.RandiRange(0, totalSize - doorSize);
                    Rect2I door = new Rect2I(
                        overlap.Position.X,
                        overlap.Position.Y + doorOffset,
                        1,
                        doorSize
                    );
                    Doors.Add(door);
                }
            }
        }
    }

    // Delete connections with the given chance, if they do not isolate rooms.
    private void RemoveConnections(int chance, RandomNumberGenerator rng)
    {
        List<Rect2I> open = new List<Rect2I>(Rooms);
        HashSet<Rect2I> closed = new HashSet<Rect2I>();
        open.Shuffle(rng);
        foreach (var room in open)
        {
            if (closed.Contains(room))
                continue;
            var con = new List<Rect2I>(RoomConnections[room]);
            con.Shuffle(rng);
            foreach (var other in con)
            {
                if (closed.Contains(other) || rng.RandiRange(0, 99) >= chance)
                    continue;
                RemoveConnection(room, other);
                if (FindPath(room, other) == null)
                    AddConnection(room, other);
            }
        }
    }

    // Given the mapping to the previous node, build a path from mapping root to given goal.
    private List<Rect2I> BuildPath(Rect2I goal, Dictionary<Rect2I, Rect2I?> parent)
    {
        var path = new List<Rect2I>();
        Rect2I? node = goal;
        while (node is Rect2I n)
        {
            path.Add(n);
            node = parent[n];
        }
        path.Reverse();
        return path;
    }

    // Create a new room and declare it overlapping with any other room sharing at least the given amount of tiles.
    private void AddRoom(Rect2I room, int overlapMin)
    {
        List<Rect2I> connections = new List<Rect2I>();
        foreach (Rect2I other in Rooms)
        {
            if (other.Intersection(room).Area >= overlapMin)
            {
                connections.Add(other);
                RoomConnections[other].Add(room);
            }
        }
        Rooms.Add(room);
        RoomConnections[room] = connections;
    }

    // Cache that the rooms are connected in both directions.
    private void AddConnection(Rect2I a, Rect2I b)
    {
        RoomConnections[a].Add(b);
        RoomConnections[b].Add(a);
    }

    // Cache that the rooms are no longer connected.
    private void RemoveConnection(Rect2I a, Rect2I b)
    {
        RoomConnections[a].Remove(b);
        RoomConnections[b].Remove(a);
    }
}
