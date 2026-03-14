using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null)
    : ITree<TKey, TValue> 
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => InOrder().Select(e => e.Key).ToList();
    public ICollection<TValue> Values => InOrder().Select(e => e.Value).ToList();
    
    
    public virtual void Add(TKey key, TValue value)
    {
        if (Root == null)
        {
            Root = CreateNode(key, value);
            Root.Parent = null;
            Count = 1;
            OnNodeAdded(Root);
            return;
        }

        TNode current = Root;
        TNode? parent = null;
        int cmp = 0;

        while(current != null)
        {
            parent = current;
            cmp = Comparer.Compare(key, current.Key);

            if(cmp == 0)
            {
                current.Value = value;
                return;
            }

            else if(cmp < 0)
            {
                current = current.Left!;
            }
            else
            {
                current = current.Right!;
            }
        }

        TNode newNode = CreateNode(key, value);
        newNode.Parent = parent;

        if (cmp < 0)
        {
            parent!.Left = newNode;
        }
        else
        {
            parent!.Right = newNode;
        }
        Count++;
        OnNodeAdded(newNode);
    }

    
    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }
    
    
    protected virtual void RemoveNode(TNode node)
    {
        if (node.Left == null && node.Right == null)
        {
            Transplant(node, null);
            OnNodeRemoved(node.Parent, null);
            return;

        }
        else if (node.Left == null)
        {
            Transplant(node, node.Right);
            OnNodeRemoved(node.Parent, node.Right);
            return;
        }

        else if (node.Right == null)
        {
            Transplant(node, node.Left);
            OnNodeRemoved(node.Parent, node.Left);
            return;
        }

        else
        {
            TNode successor = node.Right;
            while (successor.Left != null)
            {
                successor = successor.Left;
            }
            if (successor.Parent != node)  //глубоко
            {
                Transplant(successor, successor.Right);
                successor.Right = node.Right;
                successor.Right.Parent = successor;
            }

            Transplant(node, successor); 
            successor.Left = node.Left;
            successor.Left.Parent = successor;

            OnNodeRemoved(successor.Parent, successor);
        }
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        if (x == null || x.Right == null) throw new Exception();

        TNode y = x.Right;

        x.Right = y.Left;
        if (y.Left != null) y.Left.Parent = x;

        y.Parent = x.Parent;
        if (x.Parent == null)
            Root = y;
        else if (x.IsLeftChild)
            x.Parent.Left = y;
        else
            x.Parent.Right = y;

        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        if (y == null || y.Left == null) throw new Exception();

        TNode x = y.Left;

        y.Left = x.Right;
        if (x.Right != null) x.Right.Parent = y;

        x.Parent = y.Parent;
        if (y.Parent == null)
            Root = x;
        else if (y.IsLeftChild)
            y.Parent.Left = x;
        else
            y.Parent.Right = x;

        x.Right = y;
        y.Parent = x;
    }
    
    protected void RotateBigLeft(TNode x)
    {
        if (x.Right != null)
        {
            RotateRight(x.Right);
        }
        RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        if (y.Left != null) 
        {
            RotateLeft(y.Left);
        }
        RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        var son = x.Right ?? throw new Exception();
        RotateLeft(x);
        RotateLeft(son);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        var son = y.Left ?? throw new Exception();
        RotateRight(y);
        RotateRight(son);
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion
    
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse);
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
private struct TreeIterator : IEnumerable<TreeEntry<TKey, TValue>>, IEnumerator<TreeEntry<TKey, TValue>>
{
    private readonly TNode? _root;
    private readonly TraversalStrategy _strategy;
    private TNode? _cursor;
    private TNode? _origin;
    private TreeEntry<TKey, TValue> _current;

    public TreeIterator(TNode? root, TraversalStrategy strategy)
    {
        _root = root;
        _strategy = strategy;
        _cursor = _root; //нынешний
        _origin = null; //предыдущий
        _current = default;
    }

    public TreeEntry<TKey, TValue> Current => _current;
    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        while (_cursor != null)
        {
            TNode node = _cursor;

            bool rev = IsReverseAction(_strategy);
            TNode? primary = rev ? node.Right : node.Left;
            TNode? secondary = rev ? node.Left : node.Right;
            int triggerStage = GetTargetStage(_strategy);

            // А
            if (_origin == node.Parent)
            {
                if (triggerStage == 0) 
                {
                    _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, CalculateLevel(node));
                    _origin = node;
                    if (primary != null) _cursor = primary;
                    else if (secondary != null) _cursor = secondary;
                    else _cursor = node.Parent;
                    return true;
                }

                if (primary != null)
                {
                    _origin = node;
                    _cursor = primary;
                    continue;
                }   
            }

            // Б
            if (_origin == primary || (_origin == node.Parent && primary == null))
            {
                if (triggerStage == 1)
                {
                    _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, CalculateLevel(node));
                    _origin = node;
                    if (secondary != null) _cursor = secondary;
                    else _cursor = node.Parent;
                    return true;
                }

                if (secondary != null)
                {
                    _origin = node;
                    _cursor = secondary;
                    continue;
                }
            }
            if (triggerStage == 2)
            {
                _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, CalculateLevel(node));
                _origin = node;
                _cursor = node.Parent;
                return true;
            }

            _origin = node;
            _cursor = node.Parent;
            }
        return false;
    }

    private int CalculateLevel(TNode? node)
    {
        int l = 0;
        for (var p = node?.Parent; p != null; p = p.Parent) l++;
        return l;
    }

    private static bool IsReverseAction(TraversalStrategy s) => 
        s == TraversalStrategy.InOrderReverse || s == TraversalStrategy.PreOrderReverse || s == TraversalStrategy.PostOrderReverse;

    private static int GetTargetStage(TraversalStrategy s) => s switch  
    {
        TraversalStrategy.PreOrder or TraversalStrategy.PostOrderReverse => 0,    // узел лево право
        TraversalStrategy.InOrder or TraversalStrategy.InOrderReverse => 1,    // лево узел право
        _ => 2    // лево право узел
    };

    public void Reset() { _cursor = _root; _origin = null; _current = default; }
    public void Dispose() { }
    public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return InOrder().Select(element => new KeyValuePair<TKey, TValue>(element.Key, element.Value)).GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count) throw new ArgumentException("Недостаточно места в массиве.");
        var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
        while (iterator.MoveNext())
        {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(iterator.Current.Key, iterator.Current.Value);
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}