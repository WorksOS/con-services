using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class GetProjectsForDeviceExecutorTests
  {
    private Dictionary<string, string> _customHeaders;
    private IConfigurationStore _configStore;
    private ILoggerFactory _logger;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private IServiceProvider ServiceProvider;
    private IServiceExceptionHandler ServiceExceptionHandler;

    private string _customerUid;
    private string _deviceUid;
    private string _projectUid;
    private string _projectName;
    private int _shortRaptorAssetId;
    private string _boundaryString;
    private ProjectBoundary _projectBoundary;
    private string _timeZone;


    public GetProjectsForDeviceExecutorTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.WebApi.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      _customHeaders = new Dictionary<string, string>();

      _customerUid = Guid.NewGuid().ToString();
      _deviceUid = Guid.NewGuid().ToString();
      _projectUid = Guid.NewGuid().ToString();
      _projectName = "the Project Name";
      _shortRaptorAssetId = 4445555;
      _boundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";
      _projectBoundary = new ProjectBoundary() {type = "Polygon", coordinates = new List<double[,]>() {{new double[,] {{150.3, 1.2}, {150.4, 1.2}, {150.4, 1.3}, {150.4, 1.4}, {150.3, 1.2}}}}};
      _timeZone = "New Zealand Standard Time";
    }

    [Fact]
    public async Task GetProjects_HappyPath()
    {
      var cwsProjects = new ProjectListResponseModel() 
        {Projects = new List<ProjectResponseModel>()
          { new ProjectResponseModel() {accountId = _customerUid, projectId = _projectUid, projectName = _projectName, timezone = _timeZone, boundary = _projectBoundary}}};
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetProjectsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsProjects);
     
      var projectLocalDb = new Productivity3D.Project.Abstractions.Models.DatabaseModels.Project() {ProjectUID = _projectUid, CustomerUID = _customerUid, ShortRaptorProjectId = _shortRaptorAssetId, Name = _projectName, IsArchived = false, ProjectTimeZone = _timeZone, Boundary = _boundaryString};
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(projectLocalDb);

      var getProjectsForDeviceExecutor = RequestExecutorContainerFactory.Build<GetProjectsForDeviceExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        projectRepo: projectRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getProjectsForDeviceExecutor.ProcessAsync(new DeviceIsUid(_deviceUid)) as ProjectDataListResult;

      Assert.NotNull(response);
      Assert.Equal(0, response.Code);
      Assert.Equal("success", response.Message);

      Assert.NotNull(response.ProjectDescriptors);
      Assert.Single(response.ProjectDescriptors);
      Assert.Equal(_customerUid, response.ProjectDescriptors[0].CustomerUID);
      Assert.Equal(_projectUid, response.ProjectDescriptors[0].ProjectUID);
      Assert.Equal(_shortRaptorAssetId, response.ProjectDescriptors[0].ShortRaptorProjectId);
      Assert.Equal(_projectName, response.ProjectDescriptors[0].Name);
      Assert.False(response.ProjectDescriptors[0].IsArchived);
      Assert.Equal(_boundaryString, response.ProjectDescriptors[0].ProjectGeofenceWKT);
    }

    [Fact]
    public async Task GetProject_1cwsNotInLocalDB_HappyPath()
    {
      var cwsProjects = new ProjectListResponseModel()
      {
        Projects = new List<ProjectResponseModel>()
        {
          new ProjectResponseModel() {accountId = _customerUid, projectId = _projectUid, projectName = _projectName, timezone = _timeZone, boundary = _projectBoundary},
          new ProjectResponseModel() {accountId = _customerUid, projectId = Guid.NewGuid().ToString(), projectName = _projectName, timezone = _timeZone, boundary = _projectBoundary}
        }
      };
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetProjectsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsProjects);

      var projectLocalDb = new Productivity3D.Project.Abstractions.Models.DatabaseModels.Project() { ProjectUID = _projectUid, CustomerUID = _customerUid, ShortRaptorProjectId = _shortRaptorAssetId, Name = _projectName, IsArchived = false, ProjectTimeZone = _timeZone, Boundary = _boundaryString };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.GetProjectOnly(_projectUid))
        .ReturnsAsync(projectLocalDb);

      var getProjectsForDeviceExecutor = RequestExecutorContainerFactory.Build<GetProjectsForDeviceExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        projectRepo: projectRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getProjectsForDeviceExecutor.ProcessAsync(new DeviceIsUid(_deviceUid)) as ProjectDataListResult;

      Assert.NotNull(response);
      Assert.Equal(0, response.Code);
      Assert.Equal("success", response.Message);

      Assert.NotNull(response.ProjectDescriptors);
      Assert.Single(response.ProjectDescriptors);
      Assert.Equal(_customerUid, response.ProjectDescriptors[0].CustomerUID);
      Assert.Equal(_projectUid, response.ProjectDescriptors[0].ProjectUID);
      Assert.Equal(_shortRaptorAssetId, response.ProjectDescriptors[0].ShortRaptorProjectId);
      Assert.Equal(_projectName, response.ProjectDescriptors[0].Name);
      Assert.False(response.ProjectDescriptors[0].IsArchived);
      Assert.Equal(_boundaryString, response.ProjectDescriptors[0].ProjectGeofenceWKT);
    }

    [Fact]
    public async Task GetProjects_NoneFoundInCws_UnhappyPath()
    {
      var cwsProjects = new ProjectListResponseModel();
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetProjectsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsProjects);

      var projectLocalDb = new Productivity3D.Project.Abstractions.Models.DatabaseModels.Project() { ProjectUID = _projectUid, CustomerUID = _customerUid, ShortRaptorProjectId = _shortRaptorAssetId, Name = _projectName, IsArchived = false, ProjectTimeZone = _timeZone, Boundary = _boundaryString };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(projectLocalDb);

      var getProjectsForDeviceExecutor = RequestExecutorContainerFactory.Build<GetProjectsForDeviceExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        projectRepo: projectRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getProjectsForDeviceExecutor.ProcessAsync(new DeviceIsUid(_deviceUid)) as ProjectDataListResult;

      Assert.NotNull(response);
      Assert.Equal(105, response.Code);
      Assert.Equal("Unable to locate projects for device in cws", response.Message);

      Assert.Null(response.ProjectDescriptors);
    }


    [Fact]
    public async Task GetProjects_InternalException_UnhappyPath()
    {
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));

      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetProjectsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ThrowsAsync(exception);
      
      var projectRepo = new Mock<IProjectRepository>();

      var getProjectsForDeviceExecutor = RequestExecutorContainerFactory.Build<GetProjectsForDeviceExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        projectRepo: projectRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(() => getProjectsForDeviceExecutor.ProcessAsync(new DeviceIsUid(_deviceUid)));

      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(104, ex.GetResult.Code);
    }
  }
}
