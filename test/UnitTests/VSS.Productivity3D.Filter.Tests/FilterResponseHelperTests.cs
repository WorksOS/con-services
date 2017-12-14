using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Utilities;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class FilterResponseHelperTests
  {
    [TestMethod]
    public void Should_return_When_project_is_null()
    {
      try
      {
        FilterResponseHelper.SetStartEndDates(null, new MasterData.Repositories.DBModels.Filter());
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    public void Should_return_When_filter_is_null()
    {
      try
      {
        FilterResponseHelper.SetStartEndDates(new ProjectData(), filter: null);
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    public void Should_return_When_filters_collection_is_null()
    {
      try
      {
        FilterResponseHelper.SetStartEndDates(new ProjectData(), filters: null);
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    public void Should_return_When_project_ianaTimezone_is_null()
    {

      try
      {
        FilterResponseHelper.SetStartEndDates(new ProjectData(), new MasterData.Repositories.DBModels.Filter());
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    [DataRow(DateRangeType.Custom)]
    [DataRow(DateRangeType.ProjectExtents)]
    public void Should_not_set_dates_based_on_DateRangeType(DateRangeType dateRangeType)
    {
      var filter = new MasterData.Repositories.DBModels.Filter { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"elevationType\":null}}" };

      FilterResponseHelper.SetStartEndDates(new ProjectData { IanaTimeZone = "America/Los_Angeles" }, filter);

      MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filter.FilterJson);

      Assert.IsNull(filterObj.startUTC);
      Assert.IsNull(filterObj.endUTC);
    }

    [TestMethod]
    [DataRow(DateRangeType.CurrentMonth)]
    [DataRow(DateRangeType.CurrentWeek)]
    [DataRow(DateRangeType.PreviousMonth)]
    [DataRow(DateRangeType.PreviousWeek)]
    [DataRow(DateRangeType.Today)]
    [DataRow(DateRangeType.Yesterday)]
    public void Should_set_dates_based_on_DateRangeType(DateRangeType dateRangeType)
    {
      var filter = new MasterData.Repositories.DBModels.Filter { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"elevationType\":null}}" };

      FilterResponseHelper.SetStartEndDates(new ProjectData { IanaTimeZone = "America/Los_Angeles" }, filter);

      MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filter.FilterJson);

      Assert.IsNotNull(filterObj.startUTC);
      Assert.IsNotNull(filterObj.endUTC);
    }
  }
}