using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_CombinedFilToFromBinary_FilterSetter
  {
    [Fact]
    public void ToFromBinary_FilterSet_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<FilterSet>("Empty FilterSet not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_FilterSet_OneFilter()
    {
      SimpleBinarizableInstanceTester.TestClass<FilterSet>(new FilterSet(new CombinedFilter()),
        "FilterSet with one empty filter not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_FilterSet_TwoFilters()
    {
      SimpleBinarizableInstanceTester.TestClass<FilterSet>(new FilterSet(new CombinedFilter(), new CombinedFilter()),
        "FilterSet with one empty filter not same after round trip serialisation");
    }
  }
}

