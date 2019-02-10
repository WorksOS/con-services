using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics
{
  public class AnalyticsTestsDIFixture
  {
    public AnalyticsTestsDIFixture()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddTransient<IFilterSet>(factory => new FilterSet()))
        .Complete();
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}
