using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics
{
  public class AnalyticsTestsDIFixture
  {
    private static object Lock = new object();

    public AnalyticsTestsDIFixture()
    {
      lock (Lock)
      {
        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddTransient<IFilterSet>(factory => new FilterSet()))
          .Complete();
      }
    }

    public void Dispose() { } // Nothing needing doing 
  }
}
