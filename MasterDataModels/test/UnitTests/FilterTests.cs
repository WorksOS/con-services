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
  }
}