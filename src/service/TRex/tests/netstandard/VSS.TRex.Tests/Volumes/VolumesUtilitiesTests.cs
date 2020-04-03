using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Volumes;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class VolumesUtilitiesTests
  {
    [Theory]
    [InlineData(VolumeComputationType.Between2Filters, ProdReportSelectionType.Filter, ProdReportSelectionType.Filter)]
    [InlineData(VolumeComputationType.BetweenFilterAndDesign, ProdReportSelectionType.Filter, ProdReportSelectionType.Surface)]
    [InlineData(VolumeComputationType.BetweenDesignAndFilter, ProdReportSelectionType.Surface, ProdReportSelectionType.Filter)]
    [InlineData(VolumeComputationType.None, ProdReportSelectionType.None, ProdReportSelectionType.None)]
    public void TestProdReportSelectionType(VolumeComputationType volumeType,
      ProdReportSelectionType expectedFromSelectionType, ProdReportSelectionType expectedToSelectionType)
    {
      VolumesUtilities.SetProdReportSelectionType(volumeType, out var fromSelectionType, out var toSelectionType);

      fromSelectionType.Should().Be(expectedFromSelectionType);
      toSelectionType.Should().Be(expectedToSelectionType);
    }
  }
}
