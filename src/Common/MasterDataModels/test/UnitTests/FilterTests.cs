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

    [TestMethod]
    [DataRow(null, null, null, false)]
    [DataRow("f76fe65a-5715-4a3b-9df4-284b7425d097", null, null, true)]
    [DataRow(null, "3e8ebc3f-a82f-4ed6-95f8-47c0f3a80e8b", null, true)]
    [DataRow(null, null, "ce56b132-58a5-4803-b2c6-7a4bc3c2390b", true)]
    public void ContainsBoundary_returns_correct_result(string designUid, string alignmentUid, string polygonUid, bool expectedResult)
    { 
      var filter = JsonConvert.DeserializeObject<Filter>($"{{\"dateRangeType\":\"0\",\"designUid\":\"{designUid}\",\"alignmentUid\":\"{alignmentUid}\",\"polygonUid\":\"{polygonUid}\"}}");

      Assert.AreEqual(expectedResult, filter.ContainsBoundary);
    }
  }
}
