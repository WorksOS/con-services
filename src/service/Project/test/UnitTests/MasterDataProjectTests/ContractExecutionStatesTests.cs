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
      Assert.Equal(124, projectErrorCodesProvider.DynamicCount);
      Assert.Equal("Supplied CoordinateSystem filename is not valid. Exceeds the length limit of 256, is empty, or contains illegal characters.", projectErrorCodesProvider.FirstNameWithOffset(2));
      Assert.Equal("LegacyImportedFileId has not been generated.", projectErrorCodesProvider.FirstNameWithOffset(50));
    }
  }
}
