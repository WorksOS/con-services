using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using Filter = VSS.Productivity3D.Common.Models.Filter;
using WGSPoint = VSS.Productivity3D.Common.Models.WGSPoint;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class VolumeSummaryHelperTests
  {


    [TestClassAttribute]
    public class GetVolumesType : VolumeSummaryHelperTests
    {
      private readonly SummaryDataDataHelper volumeSummaryDataHelper = new SummaryDataDataHelper();
      private static Filter filter;

      [ClassInitialize]
      public static void ClassInit(TestContext context)
      {
        filter = Filter.CreateFilter(
            0, "name", "desc", DateTime.Now, DateTime.Now, 1, null, false, false, ElevationType.Highest, 
            new List<WGSPoint>(), new List<Point>(), false, null, 0, 0, 0, 0, "designName", 
            FilterLayerMethod.None, null, 0, 0, 0, new List<MachineDetails>(), new List<long>(), 
            true, GPSAccuracy.Coarse, false, false, false, false, null);
      }

      [TestMethod]
      public void Should_return_GroundToGround_When_both_filters_are_set()
      {
        Assert.AreEqual(RaptorConverters.VolumesType.Between2Filters, this.volumeSummaryDataHelper.GetVolumesType(filter, filter));
      }

      [TestMethod]
      public void Should_return_GroundToDesign_When_only_baseFilter_is_set()
      {
        Assert.AreEqual(RaptorConverters.VolumesType.BetweenFilterAndDesign, this.volumeSummaryDataHelper.GetVolumesType(filter, null));
      }

      [TestMethod]
      public void Should_return_DesignToGround_When_only_topFilter_is_set()
      {
        Assert.AreEqual(RaptorConverters.VolumesType.BetweenDesignAndFilter, this.volumeSummaryDataHelper.GetVolumesType(null, filter));
      }
    }
  }
}