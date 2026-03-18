using System.Collections.Generic;
using Godot;
using TopdownTerrain.Graph;
using TopdownTerrain.Material;

namespace TopdownTerrain.Room.Feature;

[Tool]
[GlobalClass]
public partial class CorridorFeature : RoomFeature
{
    [Export]
    public WallMaterial Material = null!;

    public override Rect2I ApplyFeature(
        TopdownTerrain terrain,
        RoomGraph graph,
        Rect2I room,
        RandomNumberGenerator rng
    )
    {
        // fill the room with wall
        terrain.BufferWall(room, Material);
        var doors = graph.GetDoors(room);
        // if only one door is connected, carve it out and return
        if (doors.Count == 1)
        {
            terrain.BufferWall(doors[0], null);
            return room;
        }
        // Connect each pair of doors that is at a 90 degree angle.
        // Chosing those connections first yields better results visually.
        var remaining = new List<Rect2I>(doors);
        foreach (var door in doors)
        {
            var sideDoor = DetermineSide(door, room);
            foreach (var other in doors)
            {
                if (door == other)
                    continue;
                var sideOther = DetermineSide(other, room);
                if (sideDoor == sideOther || InvertSide(sideDoor) == sideOther)
                    continue;
                if (!remaining.Contains(other))
                    continue;
                remaining.Remove(door);
                ConnectDoors(terrain, room, door, other, rng);
            }
        }
        // Connect the doors that have not been connected yet.
        foreach (var door in remaining)
        {
            if (doors[0] == door)
                ConnectDoors(terrain, room, door, doors[1], rng);
            else
                ConnectDoors(terrain, room, door, doors[0], rng);
        }
        return room;
    }

    // Connect the two given doors in the given room with a corridor.
    private void ConnectDoors(
        TopdownTerrain terrain,
        Rect2I room,
        Rect2I doorA,
        Rect2I doorB,
        RandomNumberGenerator rng
    )
    {
        int width;
        if (rng.RandiRange(0, 1) == 1)
            width = int.Max(doorA.Size.X, doorA.Size.Y);
        else
            width = int.Max(doorB.Size.X, doorB.Size.Y);
        Side sideA = DetermineSide(doorA, room);
        Side sideB = DetermineSide(doorB, room);
        Rect2I space = doorA.Merge(doorB);
        // on the same side
        if (sideA == sideB)
        {
            terrain.BufferWall(doorA, null);
            terrain.BufferWall(doorB, null);
            switch (sideA)
            {
                case Side.Left:
                    space = new Rect2I(space.Position.X + 1, space.Position.Y, space.Size);
                    break;
                case Side.Right:
                    space = new Rect2I(space.Position.X - 1, space.Position.Y, space.Size);
                    break;
                case Side.Top:
                    space = new Rect2I(space.Position.X, space.Position.Y + 1, space.Size);
                    break;
                case Side.Bottom:
                    space = new Rect2I(space.Position.X, space.Position.Y - 1, space.Size);
                    break;
            }
            terrain.BufferWall(space.GrowSide(InvertSide(sideA), width - 1), null);
        }
        // on opposite sides
        else if (sideA == InvertSide(sideB))
        {
            Rect2I segmentA;
            Rect2I segmentB;
            Rect2I segmentCenter;
            if (sideA == Side.Left || sideA == Side.Right)
            {
                int center = room.Size.X - width - 2;
                center = center / 2;
                segmentCenter = new Rect2I(
                    room.Position.X + center + 1,
                    int.Min(doorA.Position.Y, doorB.Position.Y),
                    width,
                    space.Size.Y
                );
                segmentA = doorA.GrowSide(
                    InvertSide(sideA),
                    int.Abs(doorA.Position.X - segmentCenter.Position.X)
                );
                segmentB = doorB.GrowSide(
                    InvertSide(sideB),
                    int.Abs(doorB.Position.X - segmentCenter.Position.X)
                );
            }
            else
            {
                int center = room.Size.Y - width - 2;
                center = center / 2;
                segmentCenter = new Rect2I(
                    int.Min(doorA.Position.X, doorB.Position.X),
                    room.Position.Y + center + 1,
                    space.Size.X,
                    width
                );
                segmentA = doorA.GrowSide(
                    InvertSide(sideA),
                    int.Abs(doorA.Position.Y - segmentCenter.Position.Y)
                );
                segmentB = doorB.GrowSide(
                    InvertSide(sideB),
                    int.Abs(doorB.Position.Y - segmentCenter.Position.Y)
                );
            }
            terrain.BufferWall(segmentA, null);
            terrain.BufferWall(segmentB, null);
            terrain.BufferWall(segmentCenter, null);
        }
        // on adjacent sides
        else
        {
            int tmp = int.Max(room.Size.X, room.Size.Y);
            Rect2I corner = doorA
                .GrowSide(InvertSide(sideA), tmp)
                .Intersection(doorB.GrowSide(InvertSide(sideB), tmp));
            terrain.BufferWall(doorA.Merge(corner), null);
            terrain.BufferWall(doorB.Merge(corner), null);
        }
    }

    // Return the Side opposite to the given side.
    private Side InvertSide(Side side)
    {
        switch (side)
        {
            case Side.Left:
                return Side.Right;
            case Side.Right:
                return Side.Left;
            case Side.Bottom:
                return Side.Top;
            default:
                return Side.Bottom;
        }
    }

    // Find out on which side the door is in the room.
    private Side DetermineSide(Rect2I door, Rect2I room)
    {
        if (door.Position.X == room.Position.X)
            return Side.Left;
        else if (door.Position.Y == room.Position.Y)
            return Side.Top;
        else if (door.Position.X == room.Position.X + room.Size.X - 1)
            return Side.Right;
        else
            return Side.Bottom;
    }
}
