using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;
using Xunit;

namespace VSS.Productivity3D.Filter.Tests
{
  public class FilterTests
  {
    [Theory]
    [InlineData(DateRangeType.Today, "Today")]
    [InlineData(DateRangeType.Yesterday, "Yesterday")]
    [InlineData(DateRangeType.CurrentWeek, "CurrentWeek")]
    [InlineData(DateRangeType.CurrentMonth, "CurrentMonth")]
    [InlineData(DateRangeType.PreviousWeek, "PreviousWeek")]
    [InlineData(DateRangeType.PreviousMonth, "PreviousMonth")]
    [InlineData(DateRangeType.ProjectExtents, "ProjectExtents")]
    [InlineData(DateRangeType.Custom, "Custom")]
    [InlineData(DateRangeType.PriorToYesterday, "PriorToYesterday")]
    [InlineData(DateRangeType.PriorToPreviousWeek, "PriorToPreviousWeek")]
    [InlineData(DateRangeType.PriorToPreviousMonth, "PriorToPreviousMonth")]
    public void DateRangeName_returns_correct_string_representation(DateRangeType dateRangeType, string expectedResult)
    {
      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>($"{{\"dateRangeType\":\"{dateRangeType}\"}}");

      Assert.Equal(expectedResult, filter.DateRangeName);
    }
    
    [Theory]
    [InlineData(DateRangeType.ProjectExtents)]
    public void ApplyDateRange_null_project_extents(DateRangeType dateRangeType)
    {
      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>($"{{\"dateRangeType\":\"{dateRangeType}\"}}");
      filter.ApplyDateRange("UTC");
      if (dateRangeType == DateRangeType.ProjectExtents)
      {
        Assert.Null(filter.StartUtc);
        Assert.Null(filter.EndUtc);
      }
    }

    [Theory]
    [InlineData(DateRangeType.Today)]
    public void ApplyDateRange_null_project_start_when_asAtDate(DateRangeType dateRangeType)
    {
      var filter =
        JsonConvert.DeserializeObject<Abstractions.Models.Filter>($"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"true\"}}");
      filter.ApplyDateRange("UTC");
      Assert.Null(filter.StartUtc);
      Assert.Equal(DateTime.UtcNow.Date, filter.EndUtc.Value.Date);
    }

    [Theory]
    [InlineData(null, null, null, false)]
    [InlineData("f76fe65a-5715-4a3b-9df4-284b7425d097", null, null, true)]
    [InlineData(null, "3e8ebc3f-a82f-4ed6-95f8-47c0f3a80e8b", null, true)]
    [InlineData(null, null, "ce56b132-58a5-4803-b2c6-7a4bc3c2390b", true)]
    public void ContainsBoundary_returns_correct_result(string designUid, string alignmentUid, string polygonUid, bool expectedResult)
    { 
      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>($"{{\"dateRangeType\":\"0\",\"designUid\":\"{designUid}\",\"alignmentUid\":\"{alignmentUid}\",\"polygonUid\":\"{polygonUid}\"}}");

      Assert.Equal(expectedResult, filter.ContainsBoundary);
    }

    [Theory]
    [InlineData(555, "The Machine Name", "ce56b132-58a5-4803-b2c6-7a4bc3c2390b", "false")]
    [InlineData(-1, "The Machine Name", "ce56b132-58a5-4803-b2c6-7a4bc3c2390b", "true")]
    [InlineData(555, "The Machine Name", null, "false")]
    [InlineData(555, "", null, "true")]
    public void ContainsContributingMachines_returns_correct_result(long assetId, string machineName, string assetUid, string isJohnDoe)
    {
      var filterJson = $"{{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{{\"assetID\":\"{assetId}\",\"machineName\":\"{machineName}\",\"isJohnDoe\":{isJohnDoe},\"assetUid\":\"{assetUid}\"}}]}}";

      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);

      Assert.Single(filter.ContributingMachines);
      Assert.Equal(assetId, filter.ContributingMachines[0].AssetId);
      Assert.Equal(machineName, filter.ContributingMachines[0].MachineName);

      if (!string.IsNullOrEmpty(assetUid))
      {
        Assert.NotNull(filter.ContributingMachines[0].AssetUid);
        Assert.Equal(assetUid, filter.ContributingMachines[0].AssetUid.Value.ToString());
      }
      Assert.Equal(isJohnDoe == "true", filter.ContributingMachines[0].IsJohnDoe);
    }

    [Theory]
    [InlineData(555, "The Machine Name", "false")]
    [InlineData(-1, "The Machine Name", "true")]
    [InlineData(555, "", "true")]
    public void ContainsContributingMachines_oldFormat(long assetId, string machineName, string isJohnDoe)
    {
      var filterJson = $"{{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{{\"assetID\":\"{assetId}\",\"machineName\":\"{machineName}\",\"isJohnDoe\":{isJohnDoe}}}]}}";

      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);

      Assert.Single(filter.ContributingMachines);
      Assert.Equal(assetId, filter.ContributingMachines[0].AssetId);
      Assert.Equal(machineName, filter.ContributingMachines[0].MachineName);
      Assert.Equal(isJohnDoe == "true", filter.ContributingMachines[0].IsJohnDoe);
    }
  }
}
