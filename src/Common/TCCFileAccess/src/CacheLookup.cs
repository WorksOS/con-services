using System.Collections.Generic;
using System.Linq;

namespace VSS.TCCFileAccess
{
  internal class CacheLookup
  {
    private static readonly Dictionary<string, List<string>> keys = new Dictionary<string, List<string>>();
    private static readonly object lockObject = new object();

    public void AddFile(string filenameKey, string cacheKey)
    {
      lock (lockObject)
      {
        List<string> fileNames = null;
        if (keys.TryGetValue(filenameKey, out fileNames))
          fileNames.Add(cacheKey);
        else
          keys.Add(filenameKey, new List<string>(){cacheKey});
      }
    }

    public List<string> RetrieveCacheKeysExact(string filenameKey)
    {
      lock (lockObject)
      {
        List<string> fileNames = null;
        if (keys.TryGetValue(filenameKey, out fileNames))
          return fileNames;
        return null;
      }
    }


    public List<string> RetrieveCacheKeys(string filenameKey)
    {
      lock (lockObject)
      {
        return  keys.FirstOrDefault(k => k.Key.Contains(filenameKey)).Value;
      }
    }


    public void DropCacheKeys(string filenameKey)
    {
      lock (lockObject)
      {
        if (keys.ContainsKey(filenameKey))
          keys.Remove(filenameKey);
      }
    }

  }
}
