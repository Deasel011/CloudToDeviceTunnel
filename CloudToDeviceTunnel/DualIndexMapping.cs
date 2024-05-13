public class DualIndexMapping<T, U>
{
    private Dictionary<T,U> _TtoU = new();
    private Dictionary<U,T> _UtoT = new();
    
    public void Add(KeyValuePair<T, U> item)
    {
        _TtoU.Add(item.Key, item.Value);
        _UtoT.Add(item.Value, item.Key);
    }

    public void Add(KeyValuePair<U, T> item)
    {
        _UtoT.Add(item.Key, item.Value);
        _TtoU.Add(item.Value, item.Key);
    }

    public bool Contains(KeyValuePair<U, T> item)
    {
        return _UtoT.ContainsKey(item.Key) && _UtoT[item.Key].Equals(item.Value);
    }

    public bool Remove(KeyValuePair<U, T> item)
    {
        if (Contains(item))
        {
            _UtoT.Remove(item.Key);
            _TtoU.Remove(item.Value);
            return true;
        }
        return false;
    }

    public bool Contains(KeyValuePair<T, U> item)
    {
        return _TtoU.ContainsKey(item.Key) && _TtoU[item.Key].Equals(item.Value);
    }
    
    public bool Remove(KeyValuePair<T, U> item)
    {
        if (Contains(item))
        {
            _TtoU.Remove(item.Key);
            _UtoT.Remove(item.Value);
            return true;
        }
        return false;
    }

    public void Add(T key, U value)
    {
        _TtoU.Add(key, value);
        _UtoT.Add(value, key);
    }

    public bool ContainsKey(T key)
    {
        return _TtoU.ContainsKey(key);
    }

    public bool Remove(T key)
    {
        if (_TtoU.ContainsKey(key))
        {
            _UtoT.Remove(_TtoU[key]);
            _TtoU.Remove(key);
            return true;
        }
        return false;
    }

    public bool TryGetValue(T key, out U value)
    {
        return _TtoU.TryGetValue(key, out value);
    }

    public U this[T key]
    {
        get => _TtoU[key];
        set => _TtoU[key] = value;
    }

    public void Add(U key, T value)
    {
        _UtoT.Add(key, value);
        _TtoU.Add(value, key);
    }

    public bool ContainsKey(U key)
    {
        return _UtoT.ContainsKey(key);
    }

    public bool Remove(U key)
    {
        if (_UtoT.ContainsKey(key))
        {
            _TtoU.Remove(_UtoT[key]);
            _UtoT.Remove(key);
            return true;
        }
        return false;
    }

    public bool TryGetValue(U key, out T value)
    {
        return _UtoT.TryGetValue(key, out value);
    }

    public T this[U key]
    {
        get => _UtoT[key];
        set => _UtoT[key] = value;
    }
}