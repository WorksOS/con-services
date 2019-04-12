using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;

namespace VSS.Productivity3D.Filter.Tests
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
      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>($"{{\"dateRangeType\":\"{dateRangeType}\"}}");

      Assert.AreEqual(expectedResult, filter.DateRangeName);
    }
    
    [TestMethod]
    [DataRow(DateRangeType.ProjectExtents)]
    public void ApplyDateRange_null_project_extents(DateRangeType dateRangeType)
    {
      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>($"{{\"dateRangeType\":\"{dateRangeType}\"}}");
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
        JsonConvert.DeserializeObject<Abstractions.Models.Filter>($"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"true\"}}");
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
      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>($"{{\"dateRangeType\":\"0\",\"designUid\":\"{designUid}\",\"alignmentUid\":\"{alignmentUid}\",\"polygonUid\":\"{polygonUid}\"}}");

      Assert.AreEqual(expectedResult, filter.ContainsBoundary);
    }

    [TestMethod]
    [DataRow(555, "The Machine Name", "ce56b132-58a5-4803-b2c6-7a4bc3c2390b", "false")]
    [DataRow(-1, "The Machine Name", "ce56b132-58a5-4803-b2c6-7a4bc3c2390b", "true")]
    [DataRow(555, "The Machine Name", null, "false")]
    [DataRow(555, "", null, "true")]
    public void ContainsContributingMachines_returns_correct_result(long assetId, string machineName, string assetUid, string isJohnDoe)
    {
      var filterJson = $"{{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{{\"assetID\":\"{assetId}\",\"machineName\":\"{machineName}\",\"isJohnDoe\":{isJohnDoe},\"assetUid\":\"{assetUid}\"}}]}}";

      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);

      Assert.AreEqual(1, filter.ContributingMachines.Count);
      Assert.AreEqual(assetId, filter.ContributingMachines[0].AssetId);
      Assert.AreEqual(machineName, filter.ContributingMachines[0].MachineName);

      if (!string.IsNullOrEmpty(assetUid))
      {
        Assert.IsNotNull(filter.ContributingMachines[0].AssetUid);
        Assert.AreEqual(assetUid, filter.ContributingMachines[0].AssetUid.Value.ToString());
      }
      Assert.AreEqual(isJohnDoe == "true", filter.ContributingMachines[0].IsJohnDoe);
    }

    [TestMethod]
    [DataRow(555, "The Machine Name", "false")]
    [DataRow(-1, "The Machine Name", "true")]
    [DataRow(555, "The Machine Name", "false")]
    [DataRow(555, "", "true")]
    public void ContainsContributingMachines_oldFormat(long assetId, string machineName, string isJohnDoe)
    {
      var filterJson = $"{{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{{\"assetID\":\"{assetId}\",\"machineName\":\"{machineName}\",\"isJohnDoe\":{isJohnDoe}}}]}}";

      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);

      Assert.AreEqual(1, filter.ContributingMachines.Count);
      Assert.AreEqual(assetId, filter.ContributingMachines[0].AssetId);
      Assert.AreEqual(machineName, filter.ContributingMachines[0].MachineName);
      Assert.AreEqual(isJohnDoe == "true", filter.ContributingMachines[0].IsJohnDoe);
    }
  }
}
