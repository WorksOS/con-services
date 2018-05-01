using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.UnitTests
{
  [TestClass]
  public class FilterTests
  {
    [TestMethod]
    [DataRow(DateRangeType.Today, "Today")]
    [DataRow(DateRangeType.Yesterday, "Yesterday")]
    [DataRow(DateRangeType.CurrentWeek, "CurrentWeek")]
    [DataRow(DateRangeType.CurrentMonth, "CurrentMonth")]
    [DataRow(DateRangeType.PreviousWeek, "PreviousWeek")]
    [DataRow(DateRangeType.PreviousMonth, "PreviousMonth")]
    [DataRow(DateRangeType.ProjectExtents, "ProjectExtents")]
    [DataRow(DateRangeType.Custom, "Custom")]
    [DataRow(DateRangeType.PriorToYesterday, "PriorToYesterday")]
    [DataRow(DateRangeType.PriorToPreviousWeek, "PriorToPreviousWeek")]
    [DataRow(DateRangeType.PriorToPreviousMonth, "PriorToPreviousMonth")]
    public void DateRangeName_returns_correct_string_representation(DateRangeType dateRangeType, string expectedResult)
    {
      var filter = JsonConvert.DeserializeObject<Filter>($"{{\"dateRangeType\":\"{dateRangeType}\"}}");

      Assert.AreEqual(expectedResult, filter.DateRangeName);
    }


    [TestMethod]
    [DataRow(DateRangeType.ProjectExtents)]
    public void ApplyDateRange_null_project_extents(DateRangeType dateRangeType)
    {
      var filter = JsonConvert.DeserializeObject<Filter>($"{{\"dateRangeType\":\"{dateRangeType}\"}}");
      filter.ApplyDateRange("UTC");
      if (dateRangeType == DateRangeType.ProjectExtents)
      {
        Assert.AreEqual(null, filter.StartUtc);
        Assert.AreEqual(null, filter.EndUtc);
      }
    }


    [TestMethod]
    [DataRow(DateRangeType.Today)]
    public void ApplyDateRange_null_project_start_when_asAtDate(DateRangeType dateRangeType)
    {
      var filter =
        JsonConvert.DeserializeObject<Filter>($"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"true\"}}");
      filter.ApplyDateRange("UTC");
      Assert.AreEqual(null, filter.StartUtc);
      Assert.AreEqual(DateTime.UtcNow.Date, filter.EndUtc.Value.Date);
    }


  }
}