using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using Xunit;

namespace VSS.Productivity3D.Filter.Tests
{
  public class FilterResponseHelperTests
  {
    private readonly Mock<IProductivity3dV2ProxyCompaction> _mockedProductivity3dV2ProxyCompaction;
    private readonly Guid _projectGuid = Guid.NewGuid();
    private static readonly DateTime MockedStartTime = new DateTime(2016, 11, 5);
    private readonly DateTime _mockedEndTime = new DateTime(2018, 11, 6);

    public FilterResponseHelperTests()
    {
      _mockedProductivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      _mockedProductivity3dV2ProxyCompaction.Setup(p => p.GetProjectStatistics(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.FromResult(new ProjectStatisticsResult
        {
          startTime = MockedStartTime,
          endTime = _mockedEndTime
        }));
    }

    [Fact]
    public async Task Should_return_When_project_is_null()
    {
      try
      {
        var filter = new MasterData.Repositories.DBModels.Filter
        { FilterJson = "{\"dateRangeType\":\"0\",\"elevationType\":null}" };
        await FilterJsonHelper.ParseFilterJson(null, filter, _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

        var filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filter.FilterJson);
        Assert.Equal(DateRangeType.Today, filterObj.DateRangeType);
      }
      catch (Exception exception)
      {
        Assert.True(false, $"Expected no exception, but got: {exception.Message}");
      }
    }

    [Fact]
    public async Task Should_return_When_filter_is_null()
    {
      try
      {
        await FilterJsonHelper.ParseFilterJson(new ProjectData(), filter: (MasterData.Repositories.DBModels.Filter)null,
          productivity3dV2ProxyCompaction: _mockedProductivity3dV2ProxyCompaction.Object, customHeaders: new Dictionary<string, string>());
      }
      catch (Exception exception)
      {
        Assert.True(false, $"Expected no exception, but got: {exception.Message}");
      }
    }

    [Fact]
    public async Task Should_return_When_filterDescriptor_is_null()
    {
      try
      {
        await FilterJsonHelper.ParseFilterJson(new ProjectData(), filter: (FilterDescriptor)null,
          productivity3dV2ProxyCompaction: _mockedProductivity3dV2ProxyCompaction.Object, customHeaders: new Dictionary<string, string>());
      }
      catch (Exception exception)
      {
        Assert.True(false, $"Expected no exception, but got: {exception.Message}");
      }
    }

    [Fact]
    public async Task Should_return_When_filters_collection_is_null()
    {
      try
      {
        await FilterJsonHelper.ParseFilterJson(new ProjectData(), filters: null, productivity3dV2ProxyCompaction: _mockedProductivity3dV2ProxyCompaction.Object, customHeaders: new Dictionary<string, string>());
      }
      catch (Exception exception)
      {
        Assert.True(false, $"Expected no exception, but got: {exception.Message}");
      }
    }

    [Fact]
    public async Task Should_return_When_project_ianaTimezone_is_null()
    {
      try
      {
        var filter = new MasterData.Repositories.DBModels.Filter
        { FilterJson = "{\"dateRangeType\":\"4\",\"elevationType\":null}" };
        await FilterJsonHelper.ParseFilterJson(new ProjectData(), filter, productivity3dV2ProxyCompaction: _mockedProductivity3dV2ProxyCompaction.Object, customHeaders: new Dictionary<string, string>());

        var filterObj =
          JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filter.FilterJson);
        Assert.Equal(DateRangeType.CurrentMonth, filterObj.DateRangeType);
      }
      catch (Exception exception)
      {
        Assert.True(false, $"Expected no exception, but got: {exception.Message}");
      }
    }

    [Theory]
    [InlineData(DateRangeType.Custom, true)]
    [InlineData(DateRangeType.Custom, false)]
    public async Task Should_not_set_dates_based_on_DateRangeType(DateRangeType dateRangeType, bool asAtDate)
    {
      var startUtc = dateRangeType == DateRangeType.Custom ? new DateTime(2017, 11, 5) : (DateTime?)null;
      var endUtc = dateRangeType == DateRangeType.Custom ? new DateTime(2017, 11, 6) : (DateTime?)null;
      //Json deserialize interprets date as mm/dd/yyyy so format date that way
      var startUtcStr = startUtc?.ToString("MM/dd/yyyy");
      var endUtcStr = endUtc?.ToString("MM/dd/yyyy");
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        FilterJson =
          $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"startUTC\":\"{startUtcStr}\",\"endUTC\":\"{endUtcStr}\",\"elevationType\":null}}"
      };

      await FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filter,
        productivity3dV2ProxyCompaction: _mockedProductivity3dV2ProxyCompaction.Object, customHeaders: new Dictionary<string, string>());

      Abstractions.Models.Filter filterObj =
        JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filter.FilterJson);
      Assert.Equal(dateRangeType, filterObj.DateRangeType);
      if (asAtDate)
        Assert.Equal(MockedStartTime, filterObj.StartUtc);
      else
        Assert.Equal(startUtc, filterObj.StartUtc);
      Assert.Equal(endUtc, filterObj.EndUtc);
    }

    [Theory]
    [InlineData(DateRangeType.Custom, true)]
    [InlineData(DateRangeType.Custom, false)]
    public void Should_not_set_dates_based_on_DateRangeType_When_using_Custom(DateRangeType dateRangeType,
      bool asAtDate)
    {
      var startUtc = dateRangeType == DateRangeType.Custom ? new DateTime(2017, 11, 5) : (DateTime?)null;
      var endUtc = dateRangeType == DateRangeType.Custom ? new DateTime(2017, 11, 6) : (DateTime?)null;


      //Json deserialize interprets date as mm/dd/yyyy so format date that way
      var startUtcStr = startUtc?.ToString("MM/dd/yyyy");
      var endUtcStr = endUtc?.ToString("MM/dd/yyyy");
      var filterDescriptor = new FilterDescriptor
      {
        FilterJson =
          $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"startUTC\":\"{startUtcStr}\",\"endUTC\":\"{endUtcStr}\",\"elevationType\":null}}"
      };

      FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filterDescriptor,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      Abstractions.Models.Filter filterObj =
        JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.Equal(asAtDate ? MockedStartTime : startUtc, filterObj.StartUtc);
      Assert.Equal(endUtc, filterObj.EndUtc);
    }


    [Theory]
    [InlineData(DateRangeType.ProjectExtents, true)]
    [InlineData(DateRangeType.ProjectExtents, false)]
    public void Should_return_project_extents_for_project_extents(DateRangeType dateRangeType, bool useNullDate)
    {
      var startUtc = useNullDate ? (DateTime?)null : new DateTime(2017, 11, 5);
      var endUtc = useNullDate ? (DateTime?)null : new DateTime(2017, 11, 6);

      //Json deserialize interprets date as mm/dd/yyyy so format date that way
      var startUtcStr = startUtc?.ToString("MM/dd/yyyy");
      var endUtcStr = endUtc?.ToString("MM/dd/yyyy");
      var filterDescriptor = new FilterDescriptor
      {
        FilterJson =
          $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"false\",\"startUTC\":\"{startUtcStr}\",\"endUTC\":\"{endUtcStr}\",\"elevationType\":null}}"
      };

      FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filterDescriptor,
        productivity3dV2ProxyCompaction: _mockedProductivity3dV2ProxyCompaction.Object, customHeaders: new Dictionary<string, string>());

      Abstractions.Models.Filter filterObj =
        JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.Equal(MockedStartTime, filterObj.StartUtc);
      Assert.Equal(_mockedEndTime, filterObj.EndUtc);
    }

    [Theory]
    [InlineData(DateRangeType.CurrentMonth, true)]
    [InlineData(DateRangeType.CurrentWeek, true)]
    [InlineData(DateRangeType.PreviousMonth, true)]
    [InlineData(DateRangeType.PreviousWeek, true)]
    [InlineData(DateRangeType.Today, true)]
    [InlineData(DateRangeType.Yesterday, true)]
    [InlineData(DateRangeType.PriorToYesterday, true)]
    [InlineData(DateRangeType.PriorToPreviousWeek, true)]
    [InlineData(DateRangeType.PriorToPreviousMonth, true)]
    [InlineData(DateRangeType.CurrentMonth, false)]
    [InlineData(DateRangeType.CurrentWeek, false)]
    [InlineData(DateRangeType.PreviousMonth, false)]
    [InlineData(DateRangeType.PreviousWeek, false)]
    [InlineData(DateRangeType.Today, false)]
    [InlineData(DateRangeType.Yesterday, false)]
    [InlineData(DateRangeType.PriorToYesterday, false)]
    [InlineData(DateRangeType.PriorToPreviousWeek, false)]
    [InlineData(DateRangeType.PriorToPreviousMonth, false)]
    public async Task Should_set_dates_based_on_DateRangeType_When_using_collection_of_Filters(DateRangeType dateRangeType,
      bool asAtDate)
    {
      var filters = new List<MasterData.Repositories.DBModels.Filter>();

      for (int i = 0; i < 10; i++)
      {
        filters.Add(new MasterData.Repositories.DBModels.Filter
        {
          FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null}}"
        });
      }

      await FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filters,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      foreach (var filter in filters)
      {
        ValidateDates(filter.FilterJson, asAtDate);
      }
    }

    [Theory]
    [InlineData(DateRangeType.CurrentMonth, true)]
    [InlineData(DateRangeType.CurrentWeek, true)]
    [InlineData(DateRangeType.PreviousMonth, true)]
    [InlineData(DateRangeType.PreviousWeek, true)]
    [InlineData(DateRangeType.Today, true)]
    [InlineData(DateRangeType.Yesterday, true)]
    [InlineData(DateRangeType.PriorToYesterday, true)]
    [InlineData(DateRangeType.PriorToPreviousWeek, true)]
    [InlineData(DateRangeType.PriorToPreviousMonth, true)]
    [InlineData(DateRangeType.CurrentMonth, false)]
    [InlineData(DateRangeType.CurrentWeek, false)]
    [InlineData(DateRangeType.PreviousMonth, false)]
    [InlineData(DateRangeType.PreviousWeek, false)]
    [InlineData(DateRangeType.Today, false)]
    [InlineData(DateRangeType.Yesterday, false)]
    [InlineData(DateRangeType.PriorToYesterday, false)]
    [InlineData(DateRangeType.PriorToPreviousWeek, false)]
    [InlineData(DateRangeType.PriorToPreviousMonth, false)]
    public async Task Should_set_dates_based_on_DateRangeType_When_using_Filter(DateRangeType dateRangeType, bool asAtDate)
    {
      var filter = new MasterData.Repositories.DBModels.Filter
      { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null}}" };

      await FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filter,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      ValidateDates(filter.FilterJson, asAtDate);
    }

    [Theory]
    [InlineData(DateRangeType.CurrentMonth, true)]
    [InlineData(DateRangeType.CurrentWeek, true)]
    [InlineData(DateRangeType.PreviousMonth, true)]
    [InlineData(DateRangeType.PreviousWeek, true)]
    [InlineData(DateRangeType.Today, true)]
    [InlineData(DateRangeType.Yesterday, true)]
    [InlineData(DateRangeType.PriorToYesterday, true)]
    [InlineData(DateRangeType.PriorToPreviousWeek, true)]
    [InlineData(DateRangeType.PriorToPreviousMonth, true)]
    [InlineData(DateRangeType.CurrentMonth, false)]
    [InlineData(DateRangeType.CurrentWeek, false)]
    [InlineData(DateRangeType.PreviousMonth, false)]
    [InlineData(DateRangeType.PreviousWeek, false)]
    [InlineData(DateRangeType.Today, false)]
    [InlineData(DateRangeType.Yesterday, false)]
    [InlineData(DateRangeType.PriorToYesterday, false)]
    [InlineData(DateRangeType.PriorToPreviousWeek, false)]
    [InlineData(DateRangeType.PriorToPreviousMonth, false)]
    public void Should_set_dates_based_on_DateRangeType_When_using_FilterDescriptor(DateRangeType dateRangeType,
      bool asAtDate)
    {
      var filterDescriptor = new FilterDescriptor
      { FilterJson = $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null}}" };

      FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filterDescriptor,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      ValidateDates(filterDescriptor.FilterJson, asAtDate);
    }

    [Fact]
    public void Should_handle_nocontributingMachines_using_FilterDescriptor()
    {
      var dateRangeType = DateRangeType.CurrentMonth;
      var asAtDate = true;
      var contributingMachinesString = String.Empty;
      var filterDescriptor = new FilterDescriptor
      {
        FilterJson =
          $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null{contributingMachinesString}}}"
      };
      List<MachineDetails> expectedResult = null;

      FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filterDescriptor,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      var actualResult = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.Equal(expectedResult, actualResult.ContributingMachines);
    }

    [Fact]
    public void Should_match_contributingMachines_contains_legacyAssetId_using_FilterDescriptor()
    {
      var dateRangeType = DateRangeType.CurrentMonth;
      var asAtDate = true;

      long assetId = 999;
      var machineName = "the machine name";
      var isJohnDoe = false;
      var assetUid = Guid.NewGuid();

      var contributingMachinesString =
        $",\"contributingMachines\":[{{\"assetID\":\"{assetId}\",\"machineName\":\"{machineName}\",\"isJohnDoe\":{(isJohnDoe ? "true" : "false")}}}]";
      var filterDescriptor = new FilterDescriptor
      {
        FilterJson =
          $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null{contributingMachinesString}}}"
      };

      var expectedResult = new List<MachineDetails> { new MachineDetails(assetId, machineName, isJohnDoe, assetUid) };

      var getMachinesExecutionResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(assetId, machineName, isJohnDoe,
            string.Empty, 0, null, null, null, null, null,
            assetUid: assetUid)
        }
      );

      _mockedProductivity3dV2ProxyCompaction.Setup(x =>
          x.ExecuteGenericV2Request<MachineExecutionResult>(It.IsAny<String>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(getMachinesExecutionResult);

      FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filterDescriptor,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      var actualResult = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.Single(actualResult.ContributingMachines);
      Assert.Equal(expectedResult[0], actualResult.ContributingMachines[0]);
    }

    [Fact]
    public void Should_match_contributingMachines_contains_assetUid_using_FilterDescriptor()
    {
      var dateRangeType = DateRangeType.CurrentMonth;
      var asAtDate = true;

      long assetId = 999;
      var nullLegacyAssetId = -1;
      var machineName = "the machine name";
      var isJohnDoe = false;
      var assetUid = Guid.NewGuid();

      var contributingMachinesString =
        $",\"contributingMachines\":[{{\"assetID\":\"{nullLegacyAssetId}\",\"machineName\":\"{machineName}\",\"isJohnDoe\":{(isJohnDoe ? "true" : "false")},\"assetUid\":\"{assetUid}\"}}]";
      var filterDescriptor = new FilterDescriptor
      {
        FilterJson =
          $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null{contributingMachinesString}}}"
      };

      var expectedResult = new List<MachineDetails> { new MachineDetails(assetId, machineName, isJohnDoe, assetUid) };

      var getMachinesExecutionResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(assetId, machineName, isJohnDoe,
            string.Empty, 0, null, null, null, null, null,
            assetUid: assetUid)
        }
      );
      _mockedProductivity3dV2ProxyCompaction.Setup(x =>
          x.ExecuteGenericV2Request<MachineExecutionResult>(It.IsAny<String>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(getMachinesExecutionResult);

      FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filterDescriptor,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      var actualResult = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.Single(actualResult.ContributingMachines);
      Assert.Equal(expectedResult[0], actualResult.ContributingMachines[0]);
    }

    [Fact]
    public void Should_match_contributingMachines_contains_both_using_FilterDescriptor()
    {
      var dateRangeType = DateRangeType.CurrentMonth;
      var asAtDate = true;

      long legacyAssetId = 999;
      var machineName = "the machine name";
      var isJohnDoe = false;
      var assetUid = Guid.NewGuid();

      var contributingMachinesString =
        $",\"contributingMachines\":[{{\"assetID\":\"{legacyAssetId}\",\"machineName\":\"{machineName}\",\"isJohnDoe\":{(isJohnDoe ? "true" : "false")},\"assetUid\":\"{assetUid}\"}}]";
      var filterDescriptor = new FilterDescriptor
      {
        FilterJson =
          $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null{contributingMachinesString}}}"
      };

      var expectedResult = new List<MachineDetails> { new MachineDetails(legacyAssetId, machineName, isJohnDoe, assetUid) };

      FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filterDescriptor,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      var actualResult = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.Single(actualResult.ContributingMachines);
      Assert.Equal(expectedResult[0], actualResult.ContributingMachines[0]);
    }

    [Fact]
    public void Should_match_contributingMachines_contains_neither_using_FilterDescriptor()
    {
      var dateRangeType = DateRangeType.CurrentMonth;
      var asAtDate = true;

      var nullLegacyAssetId = -1;
      var machineName = "the machine name";
      var isJohnDoe = false;

      var contributingMachinesString =
        $",\"contributingMachines\":[{{\"assetID\":\"{nullLegacyAssetId}\",\"machineName\":\"{machineName}\",\"isJohnDoe\":{(isJohnDoe ? "true" : "false")},\"assetUid\":\"{Guid.Empty}\"}}]";
      var filterDescriptor = new FilterDescriptor
      {
        FilterJson =
          $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null{contributingMachinesString}}}"
      };

      var expectedResult = new List<MachineDetails> { new MachineDetails(nullLegacyAssetId, machineName, isJohnDoe, Guid.Empty) };

      FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filterDescriptor,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      var actualResult = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.Single(actualResult.ContributingMachines);
      Assert.Equal(expectedResult[0], actualResult.ContributingMachines[0]);
    }

    [Fact]
    public void Should_match_contributingMachines_contains_legacyId_noMatch_using_FilterDescriptor()
    {
      var dateRangeType = DateRangeType.CurrentMonth;
      var asAtDate = true;

      long legacyAssetId = 999;
      var machineName = "the machine name";
      var isJohnDoe = false;
      Guid? assetUid = null;

      var contributingMachinesString =
        $",\"contributingMachines\":[{{\"assetID\":\"{legacyAssetId}\",\"machineName\":\"{machineName}\",\"isJohnDoe\":{(isJohnDoe ? "true" : "false")}}}]";
      var filterDescriptor = new FilterDescriptor
      {
        FilterJson =
          $"{{\"dateRangeType\":\"{dateRangeType}\",\"asAtDate\":\"{asAtDate}\",\"elevationType\":null{contributingMachinesString}}}"
      };

      var expectedResult = new List<MachineDetails> { new MachineDetails(legacyAssetId, machineName, isJohnDoe, assetUid) };

      FilterJsonHelper.ParseFilterJson(
        new ProjectData { IanaTimeZone = "America/Los_Angeles", ProjectUid = _projectGuid.ToString() }, filterDescriptor,
        _mockedProductivity3dV2ProxyCompaction.Object, new Dictionary<string, string>());

      var actualResult = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterDescriptor.FilterJson);
      Assert.Single(actualResult.ContributingMachines);
      Assert.Equal(expectedResult[0], actualResult.ContributingMachines[0]);
    }

    private static void ValidateDates(string filterJson, bool startUtcShouldBeExtents)
    {
      var filter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);
      //todo tidy this up
      if (startUtcShouldBeExtents)
      {
        Assert.Equal(MockedStartTime, filter.StartUtc);
      }
      else
      {
        Assert.NotNull(filter.StartUtc);
      }

      Assert.NotNull(filter.EndUtc);
    }
  }
}
