using System;
using System.Collections.Generic;
using System.Reflection;

namespace VSS.Productivity3D.MasterDataProxies.ResultHandling
{
  public abstract class GenericEnum<T, U> where T : GenericEnum<T, U>, new()
  {
    private int _index;
    private readonly List<string> _names;
    private readonly List<U> _values;

    protected GenericEnum()
    {
      Type t = typeof(T);
      Type u = typeof(U);
      if (t == u)
        throw new InvalidOperationException(String.Format("{0} and its underlying type cannot be the same",
          t.Name));
      BindingFlags bf = BindingFlags.Static | BindingFlags.Public;
      FieldInfo[] fia = t.GetFields(bf);
      _names = new List<string>();
      _values = new List<U>();
      for (int i = 0; i < fia.Length; i++)
      {
        if (fia[i].FieldType == u && (fia[i].IsLiteral || fia[i].IsInitOnly))
        {
          _names.Add(fia[i].Name);
          _values.Add((U)fia[i].GetValue(null));
        }
      }
      if (_names.Count == 0)
        throw new InvalidOperationException(String.Format("{0} has no suitable fields", t.Name));
    }

    public bool AllowInstanceExceptions { get; set; }

    public string[] GetNames()
    {
      return _names.ToArray();
    }

    public string[] GetNames(U value)
    {
      List<string> nameList = new List<string>();
      for (int i = 0; i < _values.Count; i++)
      {
        if (_values[i].Equals(value)) nameList.Add(_names[i]);
      }
      return nameList.ToArray();
    }

    public U[] GetValues()
    {
      return _values.ToArray();
    }

    public int[] GetIndices(U value)
    {
      List<int> indexList = new List<int>();
      for (int i = 0; i < _values.Count; i++)
      {
        if (_values[i].Equals(value)) indexList.Add(i);
      }
      return indexList.ToArray();
    }

    public int IndexOf(string name)
    {
      return _names.IndexOf(name);
    }

    public U ValueOf(string name)
    {
      int index = _names.IndexOf(name);
      if (index >= 0)
      {
        return _values[index];
      }
      throw new ArgumentException(String.Format("'{0}' is not a defined name of {1}", name, typeof(T).Name));
    }

    public string FirstNameWith(U value)
    {
      int index = _values.IndexOf(value);
      if (index >= 0)
      {
        return _names[index];
      }
      throw new ArgumentException(String.Format("'{0}' is not a defined value of {1}", value, typeof(T).Name));
    }

    public int FirstIndexWith(U value)
    {
      int index = _values.IndexOf(value);
      if (index >= 0)
      {
        return index;
      }
      throw new ArgumentException(String.Format("'{0}' is not a defined value of {1}", value, typeof(T).Name));
    }

    public string NameAt(int index)
    {
      if (index >= 0 && index < Count)
      {
        return _names[index];
      }
      throw new IndexOutOfRangeException(String.Format("Index must be between 0 and {0}", Count - 1));
    }

    public U ValueAt(int index)
    {
      if (index >= 0 && index < Count)
      {
        return _values[index];
      }
      throw new IndexOutOfRangeException(String.Format("Index must be between 0 and {0}", Count - 1));
    }

    public Type UnderlyingType => typeof(U);

    public int Count => _names.Count;

    public bool IsDefinedName(string name)
    {
      if (_names.IndexOf(name) >= 0) return true;
      return false;
    }

    public bool IsDefinedValue(U value)
    {
      if (_values.IndexOf(value) >= 0) return true;
      return false;
    }

    public bool IsDefinedIndex(int index)
    {
      if (index >= 0 && index < Count) return true;
      return false;
    }

    public T ByName(string name)
    {
      if (!IsDefinedName(name))
      {
        if (AllowInstanceExceptions)
          throw new ArgumentException(String.Format("'{0}' is not a defined name of {1}", name,
            typeof(T).Name));
        return null;
      }
      T t = new T { _index = _names.IndexOf(name) };
      return t;
    }

    public T ByValue(U value)
    {
      if (!IsDefinedValue(value))
      {
        if (AllowInstanceExceptions)
          throw new ArgumentException(String.Format("'{0}' is not a defined value of {1}", value,
            typeof(T).Name));
        return null;
      }
      T t = new T { _index = _values.IndexOf(value) };
      return t;
    }

    public T ByIndex(int index)
    {
      if (index < 0 || index >= Count)
      {
        if (AllowInstanceExceptions)
          throw new ArgumentException(String.Format("Index must be between 0 and {0}", Count - 1));
        return null;
      }
      T t = new T { _index = index };
      return t;
    }

    public int Index
    {
      get => _index;
      set
      {
        if (value < 0 || value >= Count)
        {
          if (AllowInstanceExceptions)
            throw new ArgumentException(String.Format("Index must be between 0 and {0}", Count - 1));
          return;
        }
        _index = value;
      }
    }

    public string Name
    {
      get => _names[_index];
      set
      {
        int index = _names.IndexOf(value);
        if (index == -1)
        {
          if (AllowInstanceExceptions)
            throw new ArgumentException(String.Format("'{0}' is not a defined name of {1}", value,
              typeof(T).Name));
          return;
        }
        _index = index;
      }
    }

    public U Value
    {
      get => _values[_index];
      set
      {
        int index = _values.IndexOf(value);
        if (index == -1)
        {
          if (AllowInstanceExceptions)
            throw new ArgumentException(String.Format("'{0}' is not a defined value of {1}", value,
              typeof(T).Name));
          return;
        }
        _index = index;
      }
    }

    public override string ToString()
    {
      return _names[_index];
    }

    public void ClearDynamic()
    {
      _names.RemoveRange(_names.Count - DynamicCount, DynamicCount);
      _values.RemoveRange(_values.Count - DynamicCount, DynamicCount);
      DynamicCount = 0;
    }

    public int DynamicCount { get; private set; }

    public void DynamicAdd(string name, U value)
    {
      if (_names.IndexOf(name) == -1)
      {
        _names.Add(name);
        _values.Add(value);
        DynamicCount++;
      }
      else
      {
        throw new InvalidOperationException(String.Format("'{0}' is already an element of {1}", name,
          typeof(T).Name));
      }
    }
  }
}