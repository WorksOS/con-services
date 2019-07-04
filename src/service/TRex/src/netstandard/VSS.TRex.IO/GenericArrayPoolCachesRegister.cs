using System.Collections.Generic;

namespace VSS.TRex.IO
{
  public static class GenericArrayPoolCachesRegister
  {
    private static List<IGenericArrayPoolCaches> _arrayPoolCaches = new List<IGenericArrayPoolCaches>();
    public static List<IGenericArrayPoolCaches> ArrayPoolCaches => _arrayPoolCaches;
    
    public static void Add(IGenericArrayPoolCaches arrayPoolCache)
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
