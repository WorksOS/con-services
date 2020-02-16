using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss
{
  public class Inputs
  {
    private readonly IDictionary<string, object> _dictionary = new Dictionary<string, object>();
    public void Add<T>(object value)
    {
      Add(typeof (T).ToString(), value);
    }

    public void Add(string key, object value)
    {
      _dictionary[key] = value;
    }

    /// <summary>
    /// "Design By Contract" method that verifies a non-null value of a specific type can be returned.
    /// </summary>
    /// <typeparam name="T">Type of the entry to look for.</typeparam>
    public T Get<T>() where T : class
    {
      object value;
      if (!_dictionary.TryGetValue(typeof(T).ToString(), out value))
      {
        string message = string.Format(CoreConstants.INPUT_DICTIONARY_KEY_NOT_FOUND, typeof(T));
        throw new KeyNotFoundException(message);
      }

      if(value == null)
      {
        string message = string.Format(CoreConstants.INPUT_DICTIONARY_ARGUMENT_EXCEPTION, typeof(T));
        throw new ArgumentException(message);
      }

      var typedValue = value as T;
      if (typedValue == null)
      {
        string message = string.Format(CoreConstants.INPUT_DICTIONARY_INVALID_CAST, typeof(T), value.GetType());
        throw new InvalidCastException(message);
      }
        
      return typedValue;
    }

    public T GetOrNew<T>() where T : class, new()
    {
      object existing;
      if (!_dictionary.TryGetValue(typeof(T).ToString(), out existing))
      {
        T value = new T();
        Add<T>(value);
        return value;
      }
      return (T)existing;
    }

    public T Get<T>(string key)
    {
      object value;
      if(_dictionary.TryGetValue(key, out value))
      {
        return (T) value;
      }
     
      return default(T);
    }

    public bool ContainsKey<T>() where T : class
    {
      return _dictionary.ContainsKey(typeof(T).ToString());
    }
  }
}