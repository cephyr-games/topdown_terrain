using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public partial class TreeMergeFeature : BSPTreeFeature
{
    [Export(PropertyHint.Range, "0,1,")]
    public double Chance = 1.0;

    [Export]
    public IntRange Amount = new IntRange(1, 1);

    public override void ApplyFeature(BSPTree tree, RandomNumberGenerator rng)
    {
        if (tree == null)
        {
            return;
        }
        if (Chance == 0 || rng.Randf() > Chance)
        {
            return;
        }
        int remaining = Amount.GetUniform(rng);
        var leafSet = new HashSet<BSPTree>(tree.Leaves());
        var targetSet = new HashSet<BSPTree>();
        foreach (var l in leafSet)
        {
            var parent = l.Parent;
            if (parent == null || parent.Left == null || parent.Right == null)
            {
                continue;
            }
            if (leafSet.Contains(parent.Left) && leafSet.Contains(parent.Right))
            {
                targetSet.Add(parent);
            }
        }

        var target = new List<BSPTree>(targetSet);
        target.Shuffle(rng);
        foreach (var t in target)
        {
            if (remaining <= 0)
            {
                return;
            }
            t.Unpartion();
            remaining -= 1;
        }
    }
}
