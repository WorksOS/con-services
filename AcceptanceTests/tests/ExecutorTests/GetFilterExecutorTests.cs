using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class GetFilterExecutorTests : FilterRepositoryBase
  {
    [TestInitialize]
    public void ClassInit()
    {
      Setup();
    }

    [TestMethod]
    public async Task GetFilterExecutor_NoExistingFilter()
    {
      var request = CreateAndValidateRequest();

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2036", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("GetFilter By filterUid. The requested filter does not exist, or does not belong to the requesting customer; project or user.", StringComparison.Ordinal), "executor threw exception but incorrect message");
    }

    [TestMethod]
    public async Task GetFilterExecutor_ExistingFilter()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = FilterRequestFull.Create(null, custUid, false, userId, projectUid, new FilterRequest { FilterUid = filterUid });

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request).ConfigureAwait(false) as FilterDescriptorSingleResult;

      var filterToTest = new FilterDescriptorSingleResult(
        new FilterDescriptor
        {
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson
        }
      );

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterUid, result.FilterDescriptor.FilterUid,
        Responses.IncorrectFilterDescriptorFilterUid);
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name,
        Responses.IncorrectFilterDescriptorName);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterJson, result.FilterDescriptor.FilterJson,
        Responses.IncorrectFilterDescriptorFilterJson);
    }

    [TestMethod]
    public async Task GetFilterExecutor_ExistingFilter_CaseInsensitive()
    {
      string custUid = Guid.NewGuid().ToString().ToLower(); // actually Guid.Parse converts to lower anyway
      string userId = Guid.NewGuid().ToString().ToLower();
      string projectUid = Guid.NewGuid().ToString().ToLower();
      string filterUid = Guid.NewGuid().ToString().ToLower();
      string name = "blah";
      string filterJson = "theJsonString";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(customerUid: custUid.ToUpper(), userId: userId.ToUpper(), projectUid: projectUid.ToUpper(), filterUid: filterUid.ToUpper());

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterUid, result.FilterDescriptor.FilterUid, Responses.IncorrectFilterDescriptorFilterUid);
      Assert.AreEqual(name, result.FilterDescriptor.Name, Responses.IncorrectFilterDescriptorName);
      Assert.AreEqual(filterJson, result.FilterDescriptor.FilterJson, Responses.IncorrectFilterDescriptorFilterJson);
    }
  }
}