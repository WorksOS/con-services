using System;
using System.Collections.Generic;
using System.Reflection;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{

  public abstract class GenericEnum<T, U> where T : GenericEnum<T, U>, new()
  {
    private readonly List<string> names;
    private readonly List<U> values;
    private bool allowInstanceExceptions;

    public GenericEnum()
    {
      Type t = typeof(T);
      Type u = typeof(U);
      if (t == u)
        throw new InvalidOperationException(String.Format("{0} and its underlying type cannot be the same",
          t.Name));
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
        throw new InvalidOperationException(String.Format("{0} has no suitable fields", t.Name));
    }

    public bool AllowInstanceExceptions
    {
      get { return allowInstanceExceptions; }
      set { allowInstanceExceptions = value; }
    }

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
      throw new ArgumentException(String.Format("'{0}' is not a defined name of {1}", name, typeof(T).Name));
    }

    public string FirstNameWith(U value)
    {
      int index = values.IndexOf(value);
      if (index >= 0)
      {
        return names[index];
      }
      throw new ArgumentException(String.Format("'{0}' is not a defined value of {1}", value, typeof(T).Name));
    }

    public int FirstIndexWith(U value)
    {
      int index = values.IndexOf(value);
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
        return names[index];
      }
      throw new IndexOutOfRangeException(String.Format("Index must be between 0 and {0}", Count - 1));
    }

    public U ValueAt(int index)
    {
      if (index >= 0 && index < Count)
      {
        return values[index];
      }
      throw new IndexOutOfRangeException(String.Format("Index must be between 0 and {0}", Count - 1));
    }

    public Type UnderlyingType
    {
      get { return typeof(U); }
    }

    public int Count
    {
      get { return names.Count; }
    }

    public bool IsDefinedName(string name)
    {
      if (names.IndexOf(name) >= 0) return true;
      return false;
    }

    public bool IsDefinedValue(U value)
    {
      if (values.IndexOf(value) >= 0) return true;
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
        if (allowInstanceExceptions)
          throw new ArgumentException(String.Format("'{0}' is not a defined name of {1}", name,
            typeof(T).Name));
        return null;
      }
      T t = new T { _index = names.IndexOf(name) };
      return t;
    }

    public T ByValue(U value)
    {
      if (!IsDefinedValue(value))
      {
        if (allowInstanceExceptions)
          throw new ArgumentException(String.Format("'{0}' is not a defined value of {1}", value,
            typeof(T).Name));
        return null;
      }
      T t = new T { _index = values.IndexOf(value) };
      return t;
    }

    public T ByIndex(int index)
    {
      if (index < 0 || index >= Count)
      {
        if (allowInstanceExceptions)
          throw new ArgumentException(String.Format("Index must be between 0 and {0}", Count - 1));
        return null;
      }
      T t = new T { _index = index };
      return t;
    }

    protected int _index;

    public int Index
    {
      get { return _index; }
      set
      {
        if (value < 0 || value >= Count)
        {
          if (allowInstanceExceptions)
            throw new ArgumentException(String.Format("Index must be between 0 and {0}", Count - 1));
          return;
        }
        _index = value;
      }
    }

    public string Name
    {
      get { return names[_index]; }
      set
      {
        int index = names.IndexOf(value);
        if (index == -1)
        {
          if (allowInstanceExceptions)
            throw new ArgumentException(String.Format("'{0}' is not a defined name of {1}", value,
              typeof(T).Name));
          return;
        }
        _index = index;
      }
    }

    public U Value
    {
      get { return values[_index]; }
      set
      {
        int index = values.IndexOf(value);
        if (index == -1)
        {
          if (allowInstanceExceptions)
            throw new ArgumentException(String.Format("'{0}' is not a defined value of {1}", value,
              typeof(T).Name));
          return;
        }
        _index = index;
      }
    }

    public override string ToString()
    {
      return names[_index];
    }

    private int dynamicCount;

    public void ClearDynamic()
    {
      names.RemoveRange(names.Count - dynamicCount, dynamicCount);
      values.RemoveRange(values.Count - dynamicCount, dynamicCount);
      dynamicCount = 0;
    }

    public int DynamicCount
    {
      get { return dynamicCount; }
    }

    public void DynamicAdd(string name, U value)
    {
      if (names.IndexOf(name) == -1)
      {
        names.Add(name);
        values.Add(value);
        dynamicCount++;
      }
      else
      {
        throw new InvalidOperationException(String.Format("'{0}' is already an element of {1}", name,
          typeof(T).Name));
      }
    }
  }



}

