using System;
using System.Collections.Generic;
using System.Reflection;

namespace VSS.MasterData.Models.ResultHandling.Abstractions
{
  public abstract class GenericEnum<T, U> where T : GenericEnum<T, U>, new()
  {
    private readonly List<string> names;
    private readonly List<U> values;

    public GenericEnum()
    {
      Type t = typeof(T);
      Type u = typeof(U);
      if (t == u)
        throw new InvalidOperationException($"{t.Name} and its underlying type cannot be the same");
      BindingFlags bf = BindingFlags.Static | BindingFlags.Public;
      FieldInfo[] fia = t.GetFields(bf);
      names = new List<string>();
      values = new List<U>();
      for (int i = 0; i < fia.Length; i++)
      {
        if (fia[i].FieldType == u && (fia[i].IsLiteral || fia[i].IsInitOnly))
        {
          names.Add(fia[i].Name);
          values.Add((U)fia[i].GetValue(null));
        }
      }
      if (names.Count == 0)
        throw new InvalidOperationException($"{t.Name} has no suitable fields");
    }

    public bool AllowInstanceExceptions { get; set; }

    public string[] GetNames()
    {
      return names.ToArray();
    }

    public string[] GetNames(U value)
    {
      List<string> nameList = new List<string>();
      for (int i = 0; i < values.Count; i++)
      {
        if (values[i].Equals(value)) nameList.Add(names[i]);
      }
      return nameList.ToArray();
    }

    public U[] GetValues()
    {
      return values.ToArray();
    }

    public int[] GetIndices(U value)
    {
      List<int> indexList = new List<int>();
      for (int i = 0; i < values.Count; i++)
      {
        if (values[i].Equals(value)) indexList.Add(i);
      }
      return indexList.ToArray();
    }

    public int IndexOf(string name)
    {
      return names.IndexOf(name);
    }

    public U ValueOf(string name)
    {
      int index = names.IndexOf(name);
      if (index >= 0)
      {
        return values[index];
      }
      throw new ArgumentException($"'{name}' is not a defined name of {typeof(T).Name}");
    }

    public string FirstNameWith(U value)
    {
      var index = values.IndexOf(value);
      return index >= 0
        ? names[index]
        : $"ERROR: '{value}' is not a defined value of {typeof(T).Name}";
    }

    public int FirstIndexWith(U value)
    {
      int index = values.IndexOf(value);
      if (index >= 0)
      {
        return index;
      }
      throw new ArgumentException($"'{value}' is not a defined value of {typeof(T).Name}");
    }

    public string NameAt(int index)
    {
      if (index >= 0 && index < Count)
      {
        return names[index];
      }
      throw new IndexOutOfRangeException($"Index must be between 0 and {Count - 1}");
    }

    public U ValueAt(int index)
    {
      if (index >= 0 && index < Count)
      {
        return values[index];
      }
      throw new IndexOutOfRangeException($"Index must be between 0 and {Count - 1}");
    }

    public Type UnderlyingType => typeof(U);

    public int Count => names.Count;

    public bool IsDefinedName(string name)
    {
      return names.IndexOf(name) >= 0;
    }

    public bool IsDefinedValue(U value)
    {
      return values.IndexOf(value) >= 0;
    }

    public bool IsDefinedIndex(int index)
    {
      return index >= 0 && index < Count;
    }

    public T ByName(string name)
    {
      if (!IsDefinedName(name))
      {
        if (AllowInstanceExceptions)
          throw new ArgumentException($"'{name}' is not a defined name of {typeof(T).Name}");
        return null;
      }
      T t = new T { _index = names.IndexOf(name) };
      return t;
    }

    public T ByValue(U value)
    {
      if (!IsDefinedValue(value))
      {
        if (AllowInstanceExceptions)
          throw new ArgumentException($"'{value}' is not a defined value of {typeof(T).Name}");
        return null;
      }
      T t = new T { _index = values.IndexOf(value) };
      return t;
    }

    public T ByIndex(int index)
    {
      if (index < 0 || index >= Count)
      {
        if (AllowInstanceExceptions)
          throw new ArgumentException($"Index must be between 0 and {Count - 1}");
        return null;
      }
      T t = new T { _index = index };
      return t;
    }

    protected int _index;

    public int Index
    {
      get => _index;
      set
      {
        if (value < 0 || value >= Count)
        {
          if (AllowInstanceExceptions)
            throw new ArgumentException($"Index must be between 0 and {Count - 1}");
          return;
        }
        _index = value;
      }
    }

    public string Name
    {
      get => names[_index];
      set
      {
        int index = names.IndexOf(value);
        if (index == -1)
        {
          if (AllowInstanceExceptions)
            throw new ArgumentException($"'{value}' is not a defined name of {typeof(T).Name}");
          return;
        }
        _index = index;
      }
    }

    public U Value
    {
      get => values[_index];
      set
      {
        int index = values.IndexOf(value);
        if (index == -1)
        {
          if (AllowInstanceExceptions)
            throw new ArgumentException($"'{value}' is not a defined value of {typeof(T).Name}");
          return;
        }
        _index = index;
      }
    }

    public override string ToString()
    {
      return names[_index];
    }

    public void ClearDynamic()
    {
      names.RemoveRange(names.Count - DynamicCount, DynamicCount);
      values.RemoveRange(values.Count - DynamicCount, DynamicCount);
      DynamicCount = 0;
    }

    public int DynamicCount { get; private set; }

    public void DynamicAdd(string name, U value)
    {
      if (names.IndexOf(name) == -1)
      {
        names.Add(name);
        values.Add(value);
        DynamicCount++;
      }
      else
      {
        throw new InvalidOperationException($"'{name}' is already an element of {typeof(T).Name}");
      }
    }
  }
}
