using System.Collections.Generic;

namespace VSS.TRex.IO
{
  public static class GenericArrayPoolCachesRegister
  {
    private static List<IGenericArrayPoolCaches> _arrayPoolCaches = new List<IGenericArrayPoolCaches>();
    public static List<IGenericArrayPoolCaches> ArrayPoolCaches => _arrayPoolCaches;
    
    public static void Add(IGenericArrayPoolCaches arrayPoolCache)
    {
      _arrayPoolCaches.Add(arrayPoolCache);
    }

    public static void Remove(IGenericArrayPoolCaches arrayPoolCache)
    {
      _arrayPoolCaches.Remove(arrayPoolCache);
    }
  }
}
