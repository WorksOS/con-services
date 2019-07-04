using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.IO.Heartbeats;

namespace VSS.TRex.IO
{
  public static class DIUtilities
  {
    private static void AddDIEntries(IServiceCollection services)
    {
      DIBuilder.Continue(services)
        .Add(x => x.AddSingleton(new RecyclableMemoryStreamManager
        {
          // Allow up to 256Mb worth of freed small blocks used by the recyclable streams for later reuse
          // Note: The default value for this setting is zero which means every block allocated to a
          // recyclable stream is freed when the stream is disposed.
          MaximumFreeSmallPoolBytes = 256 * 1024 * 1024
        }));
    }

    public static void AddPoolCachesToDI(IServiceCollection services)
    {
      DIBuilder.Continue(services).Add(x => AddDIEntries(services));
    }

    public static void AddHeartBeatLoggers()
    {
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new GenericSlabAllocatedPoolRegisterHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new GenericArrayPoolRegisterHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new GenericTwoDArrayCacheRegisterHeartBeatLogger());
    }
  }
}
