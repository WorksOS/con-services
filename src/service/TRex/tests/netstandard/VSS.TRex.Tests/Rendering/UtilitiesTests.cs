using System;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.Rendering;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Rendering
{
  public class UtilitiesTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const byte CCA_INCREMENT = 1;
    private const byte LAYER_ID = 1;

    private ISiteModel BuildModelForSingleCellCCA(byte ccaIncrement, byte targetCCA = CellPassConsts.NullCCATarget)
    {
      var baseTime = DateTime.UtcNow;
      byte baseCCA = 1;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      if (targetCCA != CellPassConsts.NullCCATarget)
        siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetCCAStateEvents.PutValueAtDate(VSS.TRex.Common.Consts.MIN_DATETIME_AS_UTC, targetCCA);

      var referenceDate = DateTime.UtcNow;
      var startReportPeriod1 = referenceDate.AddMinutes(-60);
      var endReportPeriod1 = referenceDate.AddMinutes(-30);

      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startReportPeriod1, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endReportPeriod1, ProductionEventType.EndEvent);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].LayerIDStateEvents.PutValueAtDate(endReportPeriod1, LAYER_ID);

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          CCA = (byte)(baseCCA + x * ccaIncrement),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    [Fact]
    public void Test_ComputeCCAPalette_Details()
    {
      const byte TARGET_CCA = 5;

      var siteModel = BuildModelForSingleCellCCA(CCA_INCREMENT, TARGET_CCA);

      var filter = new CellPassAttributeFilter() { MachinesList = new []{ siteModel.Machines[0].ID }, LayerID = LAYER_ID };
        
      var ccaPalette = Utilities.ComputeCCAPalette(siteModel, filter, DisplayMode.CCA) as CCAPalette;

      ccaPalette.Should().NotBe(null);

      ccaPalette.PaletteTransitions.Length.Should().Be(TARGET_CCA);

      // The first colour...
      ccaPalette.PaletteTransitions[0].Color.R.Should().Be(192);
      ccaPalette.PaletteTransitions[0].Color.G.Should().Be(192);
      ccaPalette.PaletteTransitions[0].Color.B.Should().Be(0);
      // The second colour...
      ccaPalette.PaletteTransitions[1].Color.R.Should().Be(255);
      ccaPalette.PaletteTransitions[1].Color.G.Should().Be(0);
      ccaPalette.PaletteTransitions[1].Color.B.Should().Be(0);
      // The third colour...
      ccaPalette.PaletteTransitions[2].Color.R.Should().Be(0);
      ccaPalette.PaletteTransitions[2].Color.G.Should().Be(255);
      ccaPalette.PaletteTransitions[2].Color.B.Should().Be(255);
      // The fourth colour...
      ccaPalette.PaletteTransitions[3].Color.R.Should().Be(0);
      ccaPalette.PaletteTransitions[3].Color.G.Should().Be(192);
      ccaPalette.PaletteTransitions[3].Color.B.Should().Be(192);
      // The fifth colour...
      ccaPalette.PaletteTransitions[4].Color.R.Should().Be(0);
      ccaPalette.PaletteTransitions[4].Color.G.Should().Be(128);
      ccaPalette.PaletteTransitions[4].Color.B.Should().Be(0);
    }

    [Fact]
    public void Test_ComputeCCAPalette_Summary()
    {
      const byte TARGET_CCA = 5;

      var siteModel = BuildModelForSingleCellCCA(CCA_INCREMENT, TARGET_CCA);

      var filter = new CellPassAttributeFilter()
      {
        MachinesList = new[] {siteModel.Machines[0].ID},
        LayerID = LAYER_ID
      };

      var ccaSummaryPalette = Utilities.ComputeCCAPalette(siteModel, filter, DisplayMode.CCASummary) as CCASummaryPalette;

      ccaSummaryPalette.Should().NotBe(null);

      // The undercompacted colour...
      ccaSummaryPalette.UndercompactedColour.R.Should().Be(192);
      ccaSummaryPalette.UndercompactedColour.G.Should().Be(192);
      ccaSummaryPalette.UndercompactedColour.B.Should().Be(0);
      // The compacted colour...
      ccaSummaryPalette.CompactedColour.R.Should().Be(255);
      ccaSummaryPalette.CompactedColour.G.Should().Be(0);
      ccaSummaryPalette.CompactedColour.B.Should().Be(0);
      // The overcompacted colour...
      ccaSummaryPalette.OvercompactedColour.R.Should().Be(0);
      ccaSummaryPalette.OvercompactedColour.G.Should().Be(255);
      ccaSummaryPalette.OvercompactedColour.B.Should().Be(255);
    }
  }
}

