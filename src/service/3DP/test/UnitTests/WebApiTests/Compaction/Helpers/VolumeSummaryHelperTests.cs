using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class VolumeSummaryHelperTests
  {
    [TestClass]
    public class GetVolumesTypeTests : VolumeSummaryHelperTests
    {
      private readonly SummaryDataHelper volumeSummaryDataHelper = new SummaryDataHelper();
      private static FilterResult filterResult;

      [ClassInitialize]
      public static void ClassInit(TestContext context)
      {
        filterResult = FilterResult.CreateFilterObsolete(
          0, null, "name", "desc", DateTime.Now, DateTime.Now, 1, "designName", null, false, false, ElevationType.Highest,
          new List<WGSPoint>(), new List<Point>(), false, null, 0, 0, 0, 0, 
          FilterLayerMethod.None, null, 0, 0, 0, new List<MachineDetails>(), new List<long>(),
          true, GPSAccuracy.Coarse, false, false, false, false);
      }

      [TestMethod]
      public void Should_return_GroundToGround_When_both_filters_are_set()
      {
        Assert.AreEqual(VolumesType.Between2Filters, volumeSummaryDataHelper.GetVolumesType(filterResult, filterResult));
      }

      [TestMethod]
      public void Should_return_GroundToDesign_When_only_baseFilter_is_set()
      {
        Assert.AreEqual(VolumesType.BetweenFilterAndDesign, volumeSummaryDataHelper.GetVolumesType(filterResult, null));
      }

      [TestMethod]
      public void Should_return_DesignToGround_When_only_topFilter_is_set()
      {
        Assert.AreEqual(VolumesType.BetweenDesignAndFilter, volumeSummaryDataHelper.GetVolumesType(null, filterResult));
      }
    }

    [TestClass]
    public class DoGroundToGroundComparisonTests : VolumeSummaryHelperTests
    {
      [TestMethod]
      public void Should_return_False_When_baseFilter_is_null()
      {
        Assert.IsFalse(SummaryDataHelper.DoGroundToGroundComparison(null, new Filter.Abstractions.Models.Filter()));
      }

      [TestMethod]
      public void Should_return_False_When_topFilter_is_null()
      {
        Assert.IsFalse(SummaryDataHelper.DoGroundToGroundComparison(new Filter.Abstractions.Models.Filter(), null));
      }
    }
  }
}
