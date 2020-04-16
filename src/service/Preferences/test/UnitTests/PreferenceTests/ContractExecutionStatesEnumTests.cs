using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using Xunit;

namespace CCSS.Productivity3D.Preferences.Tests
{
  public class ContractExecutionStatesEnumTests
  {
    [Fact]
    public void DynamicAddwithOffsetTest()
    {
      var prefErrorCodesProvider = new PreferenceErrorCodesProvider();
      Assert.Equal(13, prefErrorCodesProvider.DynamicCount);
      Assert.Equal("Missing user UID.", prefErrorCodesProvider.FirstNameWithOffset(9));
      Assert.Equal("User preference already exists. {0}", prefErrorCodesProvider.FirstNameWithOffset(13));
    }
  }
}
