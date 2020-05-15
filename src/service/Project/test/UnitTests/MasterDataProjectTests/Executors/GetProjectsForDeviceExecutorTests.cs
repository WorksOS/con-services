using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class GetProjectsForDeviceExecutorTests
  {
    private readonly Dictionary<string, string> _customHeaders;
    private readonly IConfigurationStore _configStore;
    private readonly ILoggerFactory _logger;
    private readonly IServiceExceptionHandler _serviceExceptionHandler;

    private readonly string _customerUid;
    private readonly string _deviceUid;
    private readonly string _projectUid;
    private readonly string _projectName;
    private readonly string _boundaryString;
    private readonly ProjectBoundary _projectBoundary;
    private readonly string _timeZone;


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

      IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
      _serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      _configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      _logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      _customHeaders = new Dictionary<string, string>();

      _customerUid = Guid.NewGuid().ToString();
      _deviceUid = Guid.NewGuid().ToString();
      _projectUid = Guid.NewGuid().ToString();
      _projectName = "the Project Name";
      _boundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";
      _projectBoundary = new ProjectBoundary() {type = "Polygon", coordinates = new List<double[,]> {new [,] {{ 172.6, -43.5 }, { 172.6, -43.5003 }, { 172.603, -43.5003 }, { 172.603, -43.5 }, { 172.6, -43.5 } }}};
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

      var getProjectsForDeviceExecutor = RequestExecutorContainerFactory.Build<GetProjectsForDeviceExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getProjectsForDeviceExecutor.ProcessAsync(new DeviceIsUid(_deviceUid)) as ProjectDataListResult;

      Assert.NotNull(response);
      Assert.Equal(0, response.Code);
      Assert.Equal("success", response.Message);

      Assert.NotNull(response.ProjectDescriptors);
      Assert.Single(response.ProjectDescriptors);
      Assert.Equal(_customerUid, response.ProjectDescriptors[0].CustomerUID);
      Assert.Equal(_projectUid, response.ProjectDescriptors[0].ProjectUID);
      Assert.Equal(0, response.ProjectDescriptors[0].ShortRaptorProjectId);
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

      var getProjectsForDeviceExecutor = RequestExecutorContainerFactory.Build<GetProjectsForDeviceExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getProjectsForDeviceExecutor.ProcessAsync(new DeviceIsUid(_deviceUid)) as ProjectDataListResult;

      Assert.NotNull(response);
      Assert.Equal(105, response.Code);
      Assert.Equal("Unable to locate projects for device in cws", response.Message);

      Assert.Empty(response.ProjectDescriptors);
    }

    [Fact]
    public async Task GetProjects_InternalException_UnhappyPath()
    {
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));

      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetProjectsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ThrowsAsync(exception);

      var getProjectsForDeviceExecutor = RequestExecutorContainerFactory.Build<GetProjectsForDeviceExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders, cwsDeviceClient: cwsDeviceClient.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(() => getProjectsForDeviceExecutor.ProcessAsync(new DeviceIsUid(_deviceUid)));

      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(104, ex.GetResult.Code);
    }
  }
}
