using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.IO;

namespace VSS.TRex.Cells
{
  public static class DIUtilities
  {
    private static void AddDIEntries(IServiceCollection services)
    {
      DIBuilder.Continue(services)
        .Add(x => x.AddSingleton<IGenericArrayPoolCaches<CellPass>>(new GenericArrayPoolCaches<CellPass>()))
        .Add(x => x.AddSingleton<ISlabAllocatedArrayPool<CellPass>>(new SlabAllocatedArrayPool<CellPass>()));
    }

    public static void AddPoolCachesToDI(IServiceCollection services)
    {
      DIBuilder.Continue(services).Add(x => AddDIEntries(services));
    }
  }
}
