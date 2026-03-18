using System.Collections.Generic;
using Godot;
using TopdownTerrain.Graph;
using TopdownTerrain.Material;
using TopdownTerrain.Room;

namespace TopdownTerrain;

[Tool]
[GlobalClass]
public partial class GenerationPipeline : Resource
{
    [Export]
    public Vector2I Size = new Vector2I(32, 32);

    [Export]
    public Vector2I PartitionSize = new Vector2I(8, 8);

    [Export]
    public int DoorPadding = 2;

    [Export]
    public IntRange DoorWidth = new IntRange(2, 3);

    [Export]
    public int DoorChance = 100;

    [Export]
    public int Padding = 8;

    [Export]
    public Godot.Collections.Array<BSPTreeFeature>? TreeFeatures;

    [Export]
    public Godot.Collections.Array<RoomGraphFeature>? GraphFeatues;

    [Export]
    public Godot.Collections.Array<RoomGenerator>? Generators;

    [Export]
    public WallMaterial? PaddingWall;
    public RoomGraph? Graph
    {
        get { return _graph; }
    }

    private RoomGraph? _graph;

    public void Clear()
    {
        _graph = null;
    }

    public void Generate(TopdownTerrain terrain, RandomNumberGenerator rng)
    {
        if (Generators == null)
        {
            GD.PrintErr("Pipeline is missing Generators");
            return;
        }
        terrain.BufferWallHollow(
            new Rect2I(0, 0, Size.X + 2 * Padding, Size.Y + 2 * Padding),
            Padding,
            PaddingWall
        );

        BSPTree bsp = new BSPTree(
            new Rect2I(Padding, Padding, Size.X, Size.Y),
            PartitionSize,
            rng,
            1
        );
        if (TreeFeatures != null)
        {
            foreach (var feature in TreeFeatures)
            {
                feature.ApplyFeature(bsp, rng);
            }
        }

        _graph = new RoomGraph(bsp, DoorWidth, DoorChance, DoorPadding, rng);
        if (GraphFeatues != null)
        {
            foreach (var feature in GraphFeatues)
            {
                feature.ApplyFeature(_graph, rng);
            }
        }

        foreach (var filled in _graph.FilledRooms)
        {
            terrain.BufferWall(filled, PaddingWall);
        }

        List<Rect2I> remaining = new List<Rect2I>(_graph.Rooms);
        remaining.Shuffle(rng);
        foreach (var generator in Generators)
        {
            generator.Generate(terrain, _graph, remaining, rng);
        }
    }
}
