using System.Collections;

namespace LucideSharp.WinForms;

/// <summary>
/// A designer-serializable collection of <see cref="MultiSplitPanel"/> instances.
/// </summary>
public class MultiSplitPanelCollection : IList
{
    private readonly MultiSplitContainer _owner;
    private readonly List<MultiSplitPanel> _panels = new();

    internal MultiSplitPanelCollection(MultiSplitContainer owner) => _owner = owner;

    public int Count => _panels.Count;
    public bool IsFixedSize => false;
    public bool IsReadOnly => false;
    public bool IsSynchronized => false;
    public object SyncRoot => ((ICollection)_panels).SyncRoot;

    public MultiSplitPanel this[int index]
    {
        get => _panels[index];
        set => throw new NotSupportedException("Use Add or Insert to modify the panel collection.");
    }

    object? IList.this[int index]
    {
        get => _panels[index];
        set => throw new NotSupportedException("Use Add or Insert to modify the panel collection.");
    }

    public int IndexOf(MultiSplitPanel panel) => _panels.IndexOf(panel);

    public void Add(MultiSplitPanel panel) => _owner.InsertPanel(_panels.Count, panel);

    public void Insert(int index, MultiSplitPanel panel) => _owner.InsertPanel(index, panel);

    public void Remove(MultiSplitPanel panel)
    {
        var index = _panels.IndexOf(panel);
        if (index >= 0)
        {
            _owner.RemovePanelAt(index);
        }
    }

    public void RemoveAt(int index) => _owner.RemovePanelAt(index);

    public void Clear()
    {
        while (_panels.Count > 1)
        {
            _owner.RemovePanelAt(_panels.Count - 1);
        }
    }

    public bool Contains(MultiSplitPanel panel) => _panels.Contains(panel);

    public void CopyTo(MultiSplitPanel[] array, int index) => _panels.CopyTo(array, index);

    public IEnumerator<MultiSplitPanel> GetEnumerator() => _panels.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    int IList.Add(object? value)
    {
        if (value is not MultiSplitPanel panel)
        {
            throw new ArgumentException("Value must be a MultiSplitPanel.", nameof(value));
        }

        Add(panel);
        return _panels.Count - 1;
    }

    bool IList.Contains(object? value) => value is MultiSplitPanel panel && Contains(panel);

    int IList.IndexOf(object? value) => value is MultiSplitPanel panel ? IndexOf(panel) : -1;

    void IList.Insert(int index, object? value)
    {
        if (value is not MultiSplitPanel panel)
        {
            throw new ArgumentException("Value must be a MultiSplitPanel.", nameof(value));
        }

        Insert(index, panel);
    }

    void IList.Remove(object? value)
    {
        if (value is MultiSplitPanel panel)
        {
            Remove(panel);
        }
    }

    void ICollection.CopyTo(Array array, int index) => ((ICollection)_panels).CopyTo(array, index);

    internal void AddInternal(MultiSplitPanel panel) => _panels.Add(panel);

    internal void InsertInternal(int index, MultiSplitPanel panel) => _panels.Insert(index, panel);

    internal void RemoveInternal(MultiSplitPanel panel) => _panels.Remove(panel);

    internal void ClearInternal() => _panels.Clear();
}