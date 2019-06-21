using VSS.TRex.DI;

namespace VSS.TRex.IO.Helpers
{
  public static class RecyclableMemoryStreamManagerHelper
  {
    private static RecyclableMemoryStreamManager _manager;
    public static RecyclableMemoryStreamManager Manager => _manager ?? (_manager = DIContext.Obtain<RecyclableMemoryStreamManager>());

    public static void Clear() => _manager = null;
  }
}
