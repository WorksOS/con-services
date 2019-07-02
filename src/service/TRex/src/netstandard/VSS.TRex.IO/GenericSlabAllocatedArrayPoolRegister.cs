using System.Collections.Generic;

namespace VSS.TRex.IO
{
  public static class GenericSlabAllocatedArrayPoolRegister
  {
    private static readonly List<ISlabAllocatedArrayPool> _arrayPoolCaches = new List<ISlabAllocatedArrayPool>();
    public static List<ISlabAllocatedArrayPool> ArrayPoolCaches => _arrayPoolCaches;
    
    public static void Add(ISlabAllocatedArrayPool arrayPoolCache)
    {
      _arrayPoolCaches.Add(arrayPoolCache);
    }

    public static void ClearAll()
    {
      _arrayPoolCaches.ForEach(x => x.Clear());
    }
  }
}
