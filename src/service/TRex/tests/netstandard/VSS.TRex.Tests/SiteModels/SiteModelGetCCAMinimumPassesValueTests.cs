using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelGetCCAMinimumPassesValueTests: IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const byte LAYER_ID = 1;
    private const byte TARGET_CCA = 5;

    private void BuildModelForSingleCellCCA(out ISiteModel siteModel, byte ccaIncrement, byte targetCCA = CellPassConsts.NullCCATarget)
    {
      var baseTime = DateTime.UtcNow;
      byte baseCCA = 1;

      siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
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
    }

    [Fact]
    public void Test_GetCCAMinimumPassesValue_UnknownMachine_WithTarget()
    {
      BuildModelForSingleCellCCA(out var siteModel, 1, TARGET_CCA);

      var passValue = siteModel.GetCCAMinimumPassesValue(Guid.Empty, TRex.Common.Consts.MIN_DATETIME_AS_UTC, TRex.Common.Consts.MAX_DATETIME_AS_UTC, LAYER_ID);

      passValue.Should().Be(0);
    }

    [Fact]
    public void Test_GetCCAMinimumPassesValue_Zero_LayerID_WithTarget()
    {
      BuildModelForSingleCellCCA(out var siteModel, 1, TARGET_CCA);

      var passValue = siteModel.GetCCAMinimumPassesValue(siteModel.Machines[0].ID, TRex.Common.Consts.MIN_DATETIME_AS_UTC, TRex.Common.Consts.MAX_DATETIME_AS_UTC, 0);

      passValue.Should().Be(TARGET_CCA);
    }

    [Fact]
    public void Test_GetCCAMinimumPassesValue_NoLayerID_WithTarget()
    {
      BuildModelForSingleCellCCA(out var siteModel, 1, TARGET_CCA);

      var passValue = siteModel.GetCCAMinimumPassesValue(siteModel.Machines[0].ID, TRex.Common.Consts.MIN_DATETIME_AS_UTC, TRex.Common.Consts.MAX_DATETIME_AS_UTC, -1);

      passValue.Should().Be(0);
    }

    [Fact]
    public void Test_GetCCAMinimumPassesValue_UnknownMachine_NoTarget()
    {
      BuildModelForSingleCellCCA(out var siteModel, 1);

      var passValue = siteModel.GetCCAMinimumPassesValue(Guid.Empty, TRex.Common.Consts.MIN_DATETIME_AS_UTC, TRex.Common.Consts.MAX_DATETIME_AS_UTC, LAYER_ID);

      passValue.Should().Be(0);
    }

    [Fact]
    public void Test_GetCCAMinimumPassesValue_Zero_LayerID_NoTarget()
    {
      BuildModelForSingleCellCCA(out var siteModel, 1);

      var passValue = siteModel.GetCCAMinimumPassesValue(siteModel.Machines[0].ID, TRex.Common.Consts.MIN_DATETIME_AS_UTC, TRex.Common.Consts.MAX_DATETIME_AS_UTC, 0);

      passValue.Should().Be(0);
    }

    [Fact]
    public void Test_GetCCAMinimumPassesValue_NoLayerID_NoTarget()
    {
      BuildModelForSingleCellCCA(out var siteModel, 1);

      var passValue = siteModel.GetCCAMinimumPassesValue(siteModel.Machines[0].ID, TRex.Common.Consts.MIN_DATETIME_AS_UTC, TRex.Common.Consts.MAX_DATETIME_AS_UTC, -1);

      passValue.Should().Be(0);
    }
    [Fact]
    public void Test_GetCCAMinimumPassesValue_NoTarget()
    {
      BuildModelForSingleCellCCA(out var siteModel, 1);

      var passValue = siteModel.GetCCAMinimumPassesValue(siteModel.Machines[0].ID, TRex.Common.Consts.MIN_DATETIME_AS_UTC, TRex.Common.Consts.MAX_DATETIME_AS_UTC, LAYER_ID);

      passValue.Should().Be(0);
    }

    [Fact]
    public void Test_GetCCAMinimumPassesValue_WithTarget()
    {
      BuildModelForSingleCellCCA(out var siteModel, 1, TARGET_CCA);

      var passValue = siteModel.GetCCAMinimumPassesValue(siteModel.Machines[0].ID, TRex.Common.Consts.MIN_DATETIME_AS_UTC, TRex.Common.Consts.MAX_DATETIME_AS_UTC, LAYER_ID);

      passValue.Should().Be(TARGET_CCA);
    }
  }
}
