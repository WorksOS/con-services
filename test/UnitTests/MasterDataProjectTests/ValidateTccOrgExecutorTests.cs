using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TCCFileAccess;
using VSS.TCCFileAccess.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ValidateTccOrgExecutorTests : ExecutorBaseTests
  {
    protected ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();
    private static string _customerUid;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      _customerUid = Guid.NewGuid().ToString();
    }

    [TestMethod]
    public void TccAuthorizationRequestValidate_HappyPath()
    {
      var request = ValidateTccAuthorizationRequest.CreateValidateTccAuthorizationRequest("tccTnzOrg");
      request.Validate();
    }

    [TestMethod]
    public void TccAuthorizationRequestValidate_Invalid()
    {
      var request = ValidateTccAuthorizationRequest.CreateValidateTccAuthorizationRequest("");
      var ex = Assert.ThrowsException<ServiceException>(
        () => request.Validate());
      Assert.AreNotEqual(-1, ex.Content.IndexOf("2086", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ValidateTccOrgExecutor_HappyPath()
    {
      var customHeaders = new Dictionary<string, string>();

      var request = ValidateTccAuthorizationRequest.CreateValidateTccAuthorizationRequest("tccTnzOrg");
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      // 1) there must be an org with the shortName from the request
      // 2) the customer from TID must have that orgs orgId in our database 
      var organizations = new List<Organization>()
      {
        new Organization()
        {
          filespaceId = "5u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
          orgDisplayName = "the orgDisplayName",
          orgId = "u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
          orgTitle = "the orgTitle",
          shortName = request.OrgShortName
        },
        new Organization()
        {
          filespaceId = "efsdf45345-9f59-41c9-a5e2-e19f922f91d8",
          orgDisplayName = "the Other orgDisplayName",
          orgId = "u3333a0-9f59-41c9-a5e2-e19f922f91d8",
          orgTitle = "the other orgTitle",
          shortName = "the other shortname"
        }
      };
      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.ListOrganizations()).ReturnsAsync(organizations);

      var customerTccOrg = new CustomerTccOrg()
      {
        CustomerType = CustomerType.Customer,
        CustomerUID = _customerUid,
        IsDeleted = false,
        TCCOrgID = organizations[0].orgId
      };
      var customerRepo = new Mock<ICustomerRepository>();
      customerRepo.Setup(c => c.GetCustomerWithTccOrg(Guid.Parse(_customerUid))).ReturnsAsync(customerTccOrg);
    
      var executor = RequestExecutorContainerFactory
           .Build<ValidateTccOrgExecutor>(logger, configStore, serviceExceptionHandler,
            _customerUid, null, null, customHeaders,
            null, null,
            null, null, null,
            null, null, fileRepo.Object, customerRepo.Object);

      await executor.ProcessAsync(request);
    }
  }
}
