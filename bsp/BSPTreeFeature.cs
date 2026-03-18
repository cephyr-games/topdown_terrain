using Godot;

[Tool]
[GlobalClass]
public abstract partial class BSPTreeFeature : Resource
{
    public abstract void ApplyFeature(BSPTree tree, RandomNumberGenerator rng);
}
