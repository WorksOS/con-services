using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
  }
}