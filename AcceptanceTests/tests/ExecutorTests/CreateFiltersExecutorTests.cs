using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class CreateFiltersExecutorTests : TestControllerBase
  {
    [TestInitialize]
    public void Init()
    {
      SetupDI();
    }
    
    [TestMethod]
    public async Task CreateFiltersExecutor_Transient_3Good()
    {
      // a list of 3 valid transient filters are sent in request to creat
      // a list of 3 should be returned
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string name1 = "";
      string name2 = "";
      string name3 = "";
      string filterJson1 = "";
      string filterJson2 = "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\"}";
      string filterJson3 = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"}";

      // request data:
      var requestList = new List<FilterRequest>()
      {
        FilterRequest.Create("", name1, filterJson1),
        FilterRequest.Create("", name2, filterJson2),
        FilterRequest.Create("", name3, filterJson3)
      };

      FilterListRequest filterListRequest = new FilterListRequest()
      {
        filterRequests = new List<FilterRequest>()
      };
      filterListRequest.filterRequests = requestList.ToImmutableList();

      var filterListRequestFull = new FilterListRequestFull()
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        filterRequests = filterListRequest.filterRequests
      };

      var executor =
        RequestExecutorContainer.Build<CreateFiltersExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(filterListRequestFull).ConfigureAwait(false) as FilterDescriptorListResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(3, result.filterDescriptors.Count, "Wrong result count returned");
      Assert.AreNotEqual("", result.filterDescriptors[0].FilterUid, "first filterUid incorrect");
      Assert.AreEqual(filterJson1, result.filterDescriptors[0].FilterJson, "first filterJson incorrect");
      Assert.AreEqual(filterJson2, result.filterDescriptors[1].FilterJson, "second filterJson incorrect");
      Assert.AreEqual(filterJson3, result.filterDescriptors[2].FilterJson, "third filterJson incorrect");
    }
  }
}
