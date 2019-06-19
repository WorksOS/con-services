using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.IO;

namespace VSS.TRex.SubGridTrees.Server.Helpers
{
  public static class CellPassArrayPoolCacheHelper
  {
    private static IGenericArrayPoolCaches<CellPass> _caches;
    public static IGenericArrayPoolCaches<CellPass> Caches => _caches ?? (_caches = DIContext.Obtain<IGenericArrayPoolCaches<CellPass>>());

    public static void Clear() => _caches = null;
  }
}
