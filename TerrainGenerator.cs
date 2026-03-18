using Godot;

namespace TopdownTerrain;

[Tool]
[GlobalClass]
// Generator to apply a GenerationPipeline to a TopDownTerrain.
public partial class TerrainGenerator : Node2D
{
    [Export]
    public TopdownTerrain Terrain = null!;

    [Export]
    public bool RandomSeed = false;

    [Export]
    public ulong Seed;

    [Export]
    public GenerationPipeline Pipeline = null!;

    [Export]
    public bool DrawDebug = false;

    [Export]
    public Color DebugColor = new Color(0, 0, 1, 0.5f);

    [ExportToolButton("Generate")]
    public Callable GenerateButton => Callable.From(Generate);

    [ExportToolButton("Clear")]
    public Callable ClearButton => Callable.From(Clear);

    public void Clear()
    {
        Terrain.Clear();
        Pipeline?.Clear();
        QueueRedraw();
    }

    public void Generate()
    {
        Clear();
        RandomNumberGenerator rng = new RandomNumberGenerator();
        if (RandomSeed)
            Seed = GD.Randi();
        rng.Seed = Seed;
        Terrain.SetSeed(Seed);
        Pipeline.Generate(Terrain, rng);
        Terrain.FlushBuffer();
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!DrawDebug)
            return;
        var rooms = Pipeline?.Graph?.Rooms;
        if (rooms != null)
        {
            foreach (var room in rooms)
                DrawRect(new Rect2(room.Position * 16, room.Size * 16), DebugColor);
        }
    }
}
