using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;

namespace VSS.TRex.Cells
{
  public static class DIUtilities
  {
    private static void AddDIEntries(IServiceCollection services)
    {
    }

    public static void AddPoolCachesToDI(IServiceCollection services)
    {
      DIBuilder.Continue(services).Add(x => AddDIEntries(services));
    }
  }
}
