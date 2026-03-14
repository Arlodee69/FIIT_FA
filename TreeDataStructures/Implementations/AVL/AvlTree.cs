using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        var current = newNode.Parent; 

        while (current != null)
        {
            Balance(current);
            current = current.Parent;
        }
    }
    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        var current = parent;  

        while (current != null)
        {
            Balance(current);
            current = current.Parent;
        }
    }

    private int GetBalance(AvlNode<TKey, TValue>? node) => node == null ? 0 : (node.Left?.Height ?? 0) - (node.Right?.Height ?? 0);


    private void UpdateHeight(AvlNode<TKey, TValue> node)
    {
        node.Height = 1 + Math.Max(node.Left?.Height ?? 0, node.Right?.Height ?? 0);
    }
    
    private void Balance(AvlNode<TKey, TValue> node)
    {
        UpdateHeight(node);
        int balance = GetBalance(node);
        if (balance == 2) 
        {
            // левый ребенок перевешивает вправо
            if (GetBalance(node.Left) < 0)
            {
                RotateBigLeft(node);
            }
        }

        else if (balance == -2)
        {
            // правый ребенок перевешивает влево
            if (GetBalance(node.Right) > 0)
            {
                RotateBigRight(node.Right!);
            }
        }      
    }
}