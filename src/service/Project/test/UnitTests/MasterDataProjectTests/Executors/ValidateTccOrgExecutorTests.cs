using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;
using VSS.TCCFileAccess.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class ValidateTccOrgExecutorTestsDiFixture : UnitTestsDIFixture<ValidateTccOrgExecutorTestsDiFixture>
  {
    [Fact]
    public void TccAuthorizationRequestValidate_HappyPath()
    {
      ValidateTccAuthorizationRequest.CreateValidateTccAuthorizationRequest("tccTnzOrg")
                                     .Validate();
    }

    [Fact]
    public void TccAuthorizationRequestValidate_Invalid()
    {
      var request = ValidateTccAuthorizationRequest.CreateValidateTccAuthorizationRequest("");
      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2086", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateTccOrgExecutor_HappyPath()
    {
      var customHeaders = new Dictionary<string, string>();

      var request = ValidateTccAuthorizationRequest.CreateValidateTccAuthorizationRequest("tccTnzOrg");
      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      // 1) there must be an org with the shortName from the request
      // 2) the customer from TID must have that orgs orgId in our database 
      var organizations = new List<Organization>
                          {
        new Organization
        {
          filespaceId = "5u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
          orgDisplayName = "the orgDisplayName",
          orgId = "u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
          orgTitle = "the orgTitle",
          shortName = request.OrgShortName
        },
        new Organization
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

      var customerUid = Guid.NewGuid().ToString();

      var customerTccOrg = new CustomerTccOrg
      {
        CustomerType = CustomerType.Customer,
        CustomerUID = customerUid,
        IsDeleted = false,
        TCCOrgID = organizations[0].orgId
      };
      var customerRepo = new Mock<ICustomerRepository>();
      customerRepo.Setup(c => c.GetCustomerWithTccOrg(Guid.Parse(customerUid))).ReturnsAsync(customerTccOrg);

      var executor = RequestExecutorContainerFactory
           .Build<ValidateTccOrgExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, null, null, customHeaders,
            null, null,
            null, null, null, null, null,
            null, null, fileRepo.Object, customerRepo.Object);

      await executor.ProcessAsync(request);
    }
  }
}
