using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.WebApi.Common;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class ProjectChangedExecutorTests : UnitTestsDIFixture<ProjectChangedExecutorTests>
  {
    public ProjectChangedExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task ProjectChangedExecutor_MetadataChanged_BothAccountAndProject()
    {
      var projectUid = Guid.NewGuid();
      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_ACCOUNT);

      var notificationHubClient = new Mock<INotificationHubClient>();
      notificationHubClient.Setup(n => n.Notify(It.IsAny<Notification>())).Returns(Task.CompletedTask);

      var request = new ProjectChangeNotificationDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = projectTrn,
        NotificationType = NotificationType.MetaData,
        CoordinateSystemFileName = null,
        CoordinateSystemFileContent = null
      };
      var executor = RequestExecutorContainerFactory.Build<ProjectChangedExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders, notificationHubClient: notificationHubClient.Object);
      var result = await executor.ProcessAsync(request);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ProjectChangedExecutor_MetadataChanged_Account()
    {
      var projectUid = Guid.NewGuid();
      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_ACCOUNT);

      var notificationHubClient = new Mock<INotificationHubClient>();
      notificationHubClient.Setup(n => n.Notify(It.IsAny<Notification>())).Returns(Task.CompletedTask);

      var request = new ProjectChangeNotificationDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        NotificationType = NotificationType.MetaData,
        CoordinateSystemFileName = null,
        CoordinateSystemFileContent = null
      };
      var executor = RequestExecutorContainerFactory.Build<ProjectChangedExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders, notificationHubClient: notificationHubClient.Object);
      var result = await executor.ProcessAsync(request);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ProjectChangedExecutor_MetadataChanged_Project()
    {
      var projectUid = Guid.NewGuid();
      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_ACCOUNT);

      var notificationHubClient = new Mock<INotificationHubClient>();
      notificationHubClient.Setup(n => n.Notify(It.IsAny<Notification>())).Returns(Task.CompletedTask);

      var request = new ProjectChangeNotificationDto
      {
        AccountTrn = null,
        ProjectTrn = projectTrn,
        NotificationType = NotificationType.MetaData,
        CoordinateSystemFileName = null,
        CoordinateSystemFileContent = null
      };
      var executor = RequestExecutorContainerFactory.Build<ProjectChangedExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders, notificationHubClient: notificationHubClient.Object);
      var result = await executor.ProcessAsync(request);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ProjectChangedExecutor_CoordSystemChanged_HappyPath()
    {
      var projectUid = Guid.NewGuid();
      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_ACCOUNT);

      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemPost(projectUid, It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);
      
      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.IsAny<HeaderDictionary>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var request = new ProjectChangeNotificationDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = projectTrn,
        NotificationType = NotificationType.CoordinateSystem,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var executor = RequestExecutorContainerFactory.Build<ProjectChangedExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        coordProxy.Object, dataOceanClient:dataOceanClient.Object, authn: authn.Object);
      var result = await executor.ProcessAsync(request);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ProjectChangedExecutor_CoordSystemChanged_MissingProjectUid()
    {
      var request = new ProjectChangeNotificationDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        NotificationType = NotificationType.CoordinateSystem,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var executor = RequestExecutorContainerFactory.Build<ProjectChangedExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders);

      var ex = await Assert.ThrowsAsync<ServiceException>(() => executor.ProcessAsync(request));

      Assert.Contains("2005", ex.GetContent);
      Assert.Contains("Missing ProjectUID.", ex.GetContent);
    }

    [Fact]
    public async Task ProjectChangedExecutor_CoordSystemChanged_MismatchedCustomerUid()
    {
      var projectUid = Guid.NewGuid();
      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_ACCOUNT);

      var request = new ProjectChangeNotificationDto
      {
        AccountTrn = TRNHelper.MakeTRN(Guid.NewGuid(), TRNHelper.TRN_ACCOUNT),
        ProjectTrn = projectTrn,
        NotificationType = NotificationType.CoordinateSystem,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var executor = RequestExecutorContainerFactory.Build<ProjectChangedExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders);

      var ex = await Assert.ThrowsAsync<ServiceException>(() => executor.ProcessAsync(request));

      Assert.Contains("2135", ex.GetContent);
      Assert.Contains("Mismatched customerUid.", ex.GetContent);
    }

  }

}
