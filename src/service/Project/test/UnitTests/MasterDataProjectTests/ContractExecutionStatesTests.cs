using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ContractExecutionStatesEnumTests
  {
    [Fact]
    public void DynamicAddwithOffsetTest()
    {
      var projectErrorCodesProvider = new ProjectErrorCodesProvider();
      Assert.Equal(64, projectErrorCodesProvider.DynamicCount);
      Assert.Equal("Missing ProjectUID.", projectErrorCodesProvider.FirstNameWithOffset(5));
      Assert.Equal("LegacyImportedFileId has not been generated.", projectErrorCodesProvider.FirstNameWithOffset(50));
    }
  }
}
