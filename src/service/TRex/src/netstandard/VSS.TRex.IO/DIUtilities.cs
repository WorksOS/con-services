using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;

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
        }))
        .Add(x => x.AddSingleton<IGenericArrayPoolCaches<byte>>(new GenericArrayPoolCaches<byte>()))
        .Add(x => x.AddSingleton<IGenericArrayPoolCaches<int>>(new GenericArrayPoolCaches<int>()))
        .Add(x => x.AddSingleton<IGenericArrayPoolCaches<long>>(new GenericArrayPoolCaches<long>()))
        .Add(x => x.AddSingleton<IGenericArrayPoolCaches<ulong>>(new GenericArrayPoolCaches<ulong>()))
        .Add(x => x.AddSingleton<IGenericArrayPoolCaches<char>>(new GenericArrayPoolCaches<char>()))
        .Add(x => x.AddSingleton<ITwoDArrayCache<int>>(new TwoDArrayCache<int>(32, 32, 10)));
    }

    public static void AddPoolCachesToDI(IServiceCollection services)
    {
      DIBuilder.Continue(services).Add(x => AddDIEntries(services));
    }
  }
}
