using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        var filter = new MasterData.Repositories.DBModels.Filter { FilterJson = "{\"dateRangeType\":\"0\",\"elevationType\":null}" };
        FilterJsonHelper.ParseFilterJson(null, filter);

        MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filter.FilterJson);
        Assert.AreEqual(DateRangeType.Today, filterObj.DateRangeType);
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
        FilterJsonHelper.ParseFilterJson(new ProjectData(), filter: (MasterData.Repositories.DBModels.Filter)null);
      }
      catch (Exception exception)
      {
        Assert.Fail($"Expected no exception, but got: {exception.Message}");
      }
    }

    [TestMethod]
    public void Should_return_When_filterDescriptor_is_null()
    {
      try
      {
        FilterJsonHelper.ParseFilterJson(new ProjectData(), filter: (FilterDescriptor)null);
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
        FilterJsonHelper.ParseFilterJson(new ProjectData(), filters: null);
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
        var filter = new MasterData.Repositories.DBModels.Filter { FilterJson = "{\"dateRangeType\":\"4\",\"elevationType\":null}" };
        FilterJsonHelper.ParseFilterJson(new ProjectData(), filter);

        MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filter.FilterJson);
        Assert.AreEqual(DateRangeType.CurrentMonth, filterObj.DateRangeType);
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

      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles" }, filter);

      MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filter.FilterJson);
      Assert.AreEqual(dateRangeType, filterObj.DateRangeType);
      Assert.IsNull(filterObj.StartUtc);
      Assert.IsNull(filterObj.EndUtc);
    }

    [TestMethod]
    [DataRow(DateRangeType.Custom)]
    [DataRow(DateRangeType.ProjectExtents)]
    public void Should_not_set_dates_based_on_DateRangeType_When_using_FilterDescriptor(DateRangeType dateRangeType)
    {
      var filterDescriptor = new FilterDescriptor { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"elevationType\":null}}" };

      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles" }, filterDescriptor);

      MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterDescriptor.FilterJson);
      Assert.IsNull(filterObj.StartUtc);
      Assert.IsNull(filterObj.EndUtc);
    }

    [TestMethod]
    [DataRow(DateRangeType.CurrentMonth)]
    [DataRow(DateRangeType.CurrentWeek)]
    [DataRow(DateRangeType.PreviousMonth)]
    [DataRow(DateRangeType.PreviousWeek)]
    [DataRow(DateRangeType.Today)]
    [DataRow(DateRangeType.Yesterday)]
    public void Should_set_dates_based_on_DateRangeType_When_using_collection_of_Filters(DateRangeType dateRangeType)
    {
      var filters = new List<MasterData.Repositories.DBModels.Filter>();

      for (int i = 0; i < 10; i++)
      {
        filters.Add(new MasterData.Repositories.DBModels.Filter { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"elevationType\":null}}" });
      }
      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles" }, filters);

      foreach (var filter in filters)
      {
        ValidateDates(filter.FilterJson);
      }
    }

    [TestMethod]
    [DataRow(DateRangeType.CurrentMonth)]
    [DataRow(DateRangeType.CurrentWeek)]
    [DataRow(DateRangeType.PreviousMonth)]
    [DataRow(DateRangeType.PreviousWeek)]
    [DataRow(DateRangeType.Today)]
    [DataRow(DateRangeType.Yesterday)]
    public void Should_set_dates_based_on_DateRangeType_When_using_Filter(DateRangeType dateRangeType)
    {
      var filter = new MasterData.Repositories.DBModels.Filter { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"elevationType\":null}}" };

      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles" }, filter);

      ValidateDates(filter.FilterJson);
    }

    [TestMethod]
    [DataRow(DateRangeType.CurrentMonth)]
    [DataRow(DateRangeType.CurrentWeek)]
    [DataRow(DateRangeType.PreviousMonth)]
    [DataRow(DateRangeType.PreviousWeek)]
    [DataRow(DateRangeType.Today)]
    [DataRow(DateRangeType.Yesterday)]
    public void Should_set_dates_based_on_DateRangeType_When_using_FilterDescriptor(DateRangeType dateRangeType)
    {
      var filterDescriptor = new FilterDescriptor { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"elevationType\":null}}" };

      FilterJsonHelper.ParseFilterJson(new ProjectData { IanaTimeZone = "America/Los_Angeles" }, filterDescriptor);

      ValidateDates(filterDescriptor.FilterJson);
    }

    private static void ValidateDates(string filterJson)
    {
      MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterJson);

      Assert.IsNotNull(filterObj.StartUtc);
      Assert.IsNotNull(filterObj.EndUtc);
    }
  }
}