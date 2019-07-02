using System.Collections.Generic;

namespace VSS.TRex.IO
{
  public static class GenericTwoDArrayCacheRegister
  {
    private static readonly List<IGenericTwoDArrayCache> _arrayPoolCaches = new List<IGenericTwoDArrayCache>();
    public static List<IGenericTwoDArrayCache> ArrayPoolCaches => _arrayPoolCaches;
    
    public static void Add(IGenericTwoDArrayCache arrayPoolCache)
    {
      lock (_arrayPoolCaches)
      {
        _arrayPoolCaches.Add(arrayPoolCache);
      }
    }

    public static void ClearAll()
    {
      lock (_arrayPoolCaches)
      {
        _arrayPoolCaches.ForEach(x => x.Clear());
      }
    }
  }
}
