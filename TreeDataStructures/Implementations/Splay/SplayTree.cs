using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    where TKey : IComparable<TKey>
{
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (parent != null)
        {
            Splay(parent);
        }
    }

    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        BstNode<TKey, TValue>? current = Root;
        BstNode<TKey, TValue>? lastAccessed = null;

        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);

            if (cmp == 0)
            {
                Splay(current);
                value = current.Value;
                return true;
            }

            lastAccessed = current;
            current = cmp < 0 ? current.Left : current.Right;
        }

        if (lastAccessed != null)
        {
            Splay(lastAccessed);
        }

        value = default;
        return false;
    }

    public override bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    protected void Splay(BstNode<TKey, TValue> x)
    {
        while (x.Parent != null)
        {
            BstNode<TKey, TValue> p = x.Parent;
            BstNode<TKey, TValue>? g = p.Parent;

            if (g == null)
            {
                // Zig:У узла только родитель корень
                if (x.IsLeftChild) RotateRight(p);
                else RotateLeft(p);
            }
            else
            {
                if (x.IsLeftChild == p.IsLeftChild)
                {
                    // Zig-Zig:x и p оба левые или правые дети
                    if (x.IsLeftChild)
                    {
                        RotateRight(g);
                        RotateRight(p);
                    }
                    else
                    {
                        RotateLeft(g);
                        RotateLeft(p);
                    }
                }
                else
                {
                    // Zig-Zag:x левый ребенок, p правый (или наоборот)
                    if (x.IsLeftChild)
                    {
                        RotateRight(p);
                        RotateLeft(g);
                    }
                    else
                    {
                        RotateLeft(p);
                        RotateRight(g);
                    }
                }
            }
        }
    }
}
