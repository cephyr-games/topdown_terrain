using System.Collections.Generic;
using Godot;

[Tool]
public class BSPTree
{
    public Rect2I Rect { get; }
    public Vector2I PartitionSize { get; }
    public BSPTree? Parent { get; }
    public BSPTree? Left { get; private set; }
    public BSPTree? Right { get; private set; }

    public BSPTree(
        Rect2I rect,
        Vector2I partitionSize,
        RandomNumberGenerator rng,
        int overlap,
        BSPTree parent
    )
        : this(rect, partitionSize, rng, overlap)
    {
        Parent = parent;
    }

    public BSPTree(Rect2I rect, Vector2I partitionSize, RandomNumberGenerator rng, int overlap)
    {
        Rect = rect;
        if (partitionSize.X <= 0 || partitionSize.Y <= 0)
        {
            GD.PrintErr("Partition size cannot be <= 0");
            return;
        }
        PartitionSize = partitionSize;
        int partX = rect.Size.X - partitionSize.X * 2;
        int partY = rect.Size.Y - partitionSize.Y * 2;
        if (partX < 0 && partY < 0)
        {
            return;
        }

        bool directionX;
        if (partX >= 0 && partY >= 0)
        {
            directionX = rng.RandiRange(0, 1) == 1;
        }
        else
        {
            directionX = partX >= 0;
        }

        int partRange;
        if (directionX)
        {
            partRange = partX;
        }
        else
        {
            partRange = partY;
        }
        int partOffset = rng.RandiRange(0, partRange);

        if (directionX)
        {
            Left = new BSPTree(
                new Rect2I(
                    rect.Position.X,
                    rect.Position.Y,
                    partitionSize.X + partOffset + overlap,
                    rect.Size.Y
                ),
                partitionSize,
                rng,
                overlap,
                this
            );
            Right = new BSPTree(
                new Rect2I(
                    rect.Position.X + partitionSize.X + partOffset,
                    rect.Position.Y,
                    rect.Size.X - partitionSize.X - partOffset,
                    rect.Size.Y
                ),
                partitionSize,
                rng,
                overlap,
                this
            );
        }
        else
        {
            Left = new BSPTree(
                new Rect2I(
                    rect.Position.X,
                    rect.Position.Y,
                    rect.Size.X,
                    partitionSize.Y + partOffset + overlap
                ),
                partitionSize,
                rng,
                overlap,
                this
            );
            Right = new BSPTree(
                new Rect2I(
                    rect.Position.X,
                    rect.Position.Y + partitionSize.Y + partOffset,
                    rect.Size.X,
                    rect.Size.Y - partitionSize.Y - partOffset
                ),
                partitionSize,
                rng,
                overlap,
                this
            );
        }
    }

    public List<BSPTree> Leaves()
    {
        var result = new List<BSPTree>();
        var open = new Queue<BSPTree>();
        open.Enqueue(this);
        while (open.Count > 0)
        {
            var current = open.Dequeue();
            if (current.Left != null && current.Right != null)
            {
                open.Enqueue(current.Left);
                open.Enqueue(current.Right);
            }
            else
            {
                result.Add(current);
            }
        }
        return result;
    }

    public void Unpartion()
    {
        Left = null;
        Right = null;
    }

    public override string ToString()
    {
        if (Left != null && Right != null)
        {
            return "[" + Left + ", " + Right + "]";
        }
        else
        {
            return Rect.ToString();
        }
    }
}
