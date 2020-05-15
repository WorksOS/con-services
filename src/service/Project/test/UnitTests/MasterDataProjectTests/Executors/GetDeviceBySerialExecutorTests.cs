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
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class GetDeviceBySerialExecutorTests
  {
    private Dictionary<string, string> _customHeaders;
    private IConfigurationStore _configStore;
    private ILoggerFactory _logger;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private IServiceProvider ServiceProvider;

    private string _customerUid;
    private string _deviceUid;
    private string _deviceName;
    private string _serialNumber;
    private RelationStatusEnum _relationStatus;
    private TCCDeviceStatusEnum _tccDeviceStatus;


    public GetDeviceBySerialExecutorTests()
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
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      _customHeaders = new Dictionary<string, string>();

      _customerUid = Guid.NewGuid().ToString();
      _deviceUid = Guid.NewGuid().ToString();
      _deviceName = "the Device Name";
      _serialNumber = "67567576SN";
      _relationStatus = RelationStatusEnum.Active;
      _tccDeviceStatus = TCCDeviceStatusEnum.Registered;
    }

    [Fact]
    public async Task GetDevice_HappyPath()
    {
      var cwsDevice = new DeviceResponseModel() {Id = _deviceUid, DeviceName = _deviceName, SerialNumber = _serialNumber};
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceBySerialNumber(It.IsAny<string>(), _customHeaders))
        .ReturnsAsync(cwsDevice);
      var cwsDeviceAccountList = new DeviceAccountListResponseModel() {Accounts = new List<DeviceAccountResponseModel>() {new DeviceAccountResponseModel() {Id = _customerUid, AccountName = "the customer name", RelationStatus = _relationStatus, TccDeviceStatus = _tccDeviceStatus}}};
      cwsDeviceClient.Setup(pr => pr.GetAccountsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDeviceAccountList);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceBySerialExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new DeviceSerial(_serialNumber))
        as DeviceDescriptorSingleResult;

      Assert.NotNull(response);
      Assert.Equal(0, response.Code);
      Assert.Equal("success", response.Message);

      Assert.NotNull(response.DeviceDescriptor);
      Assert.Equal(_customerUid, response.DeviceDescriptor.CustomerUID);
      Assert.Equal(_deviceUid, response.DeviceDescriptor.DeviceUID);
      Assert.Equal(_deviceName, response.DeviceDescriptor.DeviceName);
      Assert.Equal(_serialNumber, response.DeviceDescriptor.SerialNumber);
      Assert.Equal("ACTIVE", response.DeviceDescriptor.RelationStatus.ToString().ToUpper());
      Assert.Equal("Registered", response.DeviceDescriptor.TccDeviceStatus.ToString());
      Assert.Null(response.DeviceDescriptor.ShortRaptorAssetId);
    }

    [Fact]
    public async Task GetDevice_2Accounts1Active_HappyPath()
    {
      var cwsDevice = new DeviceResponseModel() {Id = _deviceUid, DeviceName = _deviceName, SerialNumber = _serialNumber};
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceBySerialNumber(It.IsAny<string>(), _customHeaders))
        .ReturnsAsync(cwsDevice);
      var cwsDeviceAccountList = new DeviceAccountListResponseModel() {Accounts = new List<DeviceAccountResponseModel>() {new DeviceAccountResponseModel() {Id = _customerUid, AccountName = "the customer name", RelationStatus = _relationStatus, TccDeviceStatus = _tccDeviceStatus}, new DeviceAccountResponseModel() {Id = Guid.NewGuid().ToString(), AccountName = "the other customer name", RelationStatus = RelationStatusEnum.Pending, TccDeviceStatus = _tccDeviceStatus}}};
      cwsDeviceClient.Setup(pr => pr.GetAccountsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDeviceAccountList);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceBySerialExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new DeviceSerial(_serialNumber))
        as DeviceDescriptorSingleResult;

      Assert.NotNull(response);
      Assert.Equal(0, response.Code);
      Assert.Equal("success", response.Message);

      Assert.NotNull(response.DeviceDescriptor);
      Assert.Equal(0, response.DeviceDescriptor.Code);
      Assert.Equal(_customerUid, response.DeviceDescriptor.CustomerUID);
      Assert.Equal(_deviceUid, response.DeviceDescriptor.DeviceUID);
      Assert.Equal(_deviceName, response.DeviceDescriptor.DeviceName);
      Assert.Equal(_serialNumber, response.DeviceDescriptor.SerialNumber);
      Assert.Equal("ACTIVE", response.DeviceDescriptor.RelationStatus.ToString().ToUpper());
      Assert.Equal("Registered", response.DeviceDescriptor.TccDeviceStatus.ToString());
      Assert.Null(response.DeviceDescriptor.ShortRaptorAssetId);
    }

    [Fact]
    public async Task GetDevice_DeviceNotFoundInCws_UnhappyPath()
    {
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceBySerialNumber(It.IsAny<string>(), _customHeaders))
        .ReturnsAsync((DeviceResponseModel) null);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceBySerialExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new DeviceSerial(_serialNumber))
        as DeviceDescriptorSingleResult;

      Assert.NotNull(response);
      Assert.Equal(100, response.Code);
      Assert.Equal("Unable to locate device by serialNumber in cws", response.Message);
    }

    [Fact]
    public async Task GetDevice_NoAccountFound_UnhappyPath()
    {
      var cwsDevice = new DeviceResponseModel() { Id = _deviceUid, DeviceName = _deviceName, SerialNumber = _serialNumber };
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceBySerialNumber(It.IsAny<string>(), _customHeaders))
        .ReturnsAsync(cwsDevice);
      cwsDeviceClient.Setup(pr => pr.GetAccountsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync((DeviceAccountListResponseModel) null);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceBySerialExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new DeviceSerial(_serialNumber))
        as DeviceDescriptorSingleResult;

      Assert.NotNull(response);
      Assert.Equal(102, response.Code);
      Assert.Equal("Unable to locate any account for the device in cws", response.Message);

      Assert.NotNull(response.DeviceDescriptor);
      Assert.Equal(102, response.DeviceDescriptor.Code);
      Assert.Null(response.DeviceDescriptor.CustomerUID);
      Assert.Equal(_deviceUid, response.DeviceDescriptor.DeviceUID);
      Assert.Equal(_deviceName, response.DeviceDescriptor.DeviceName);
      Assert.Equal(_serialNumber, response.DeviceDescriptor.SerialNumber);
      Assert.Equal("UNKNOWN", response.DeviceDescriptor.RelationStatus.ToString().ToUpper());
      Assert.Equal("Unknown", response.DeviceDescriptor.TccDeviceStatus.ToString());
      Assert.Null(response.DeviceDescriptor.ShortRaptorAssetId);
    }

    [Fact]
    public async Task GetDevice_TooManyActiveAccounts_UnhappyPath()
    {
      var cwsDevice = new DeviceResponseModel() { Id = _deviceUid, DeviceName = _deviceName, SerialNumber = _serialNumber };
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceBySerialNumber(It.IsAny<string>(), _customHeaders))
        .ReturnsAsync(cwsDevice);
      var cwsDeviceAccountList = new DeviceAccountListResponseModel() { Accounts = new List<DeviceAccountResponseModel>()
        {
          new DeviceAccountResponseModel() { Id = _customerUid, AccountName = "the customer name", RelationStatus = _relationStatus, TccDeviceStatus = _tccDeviceStatus },
          new DeviceAccountResponseModel() { Id =  Guid.NewGuid().ToString(), AccountName = "the other customer name", RelationStatus = _relationStatus, TccDeviceStatus = _tccDeviceStatus }
        }
      };
      cwsDeviceClient.Setup(pr => pr.GetAccountsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDeviceAccountList);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceBySerialExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new DeviceSerial(_serialNumber))
        as DeviceDescriptorSingleResult;

      Assert.NotNull(response);
      Assert.Equal(103, response.Code);
      Assert.Equal("There is >1 active account for the device in cws", response.Message);

      Assert.NotNull(response.DeviceDescriptor);
      Assert.Equal(103, response.DeviceDescriptor.Code);
      Assert.Null(response.DeviceDescriptor.CustomerUID);
      Assert.Equal(_deviceUid, response.DeviceDescriptor.DeviceUID);
      Assert.Equal(_deviceName, response.DeviceDescriptor.DeviceName);
      Assert.Equal(_serialNumber, response.DeviceDescriptor.SerialNumber);
      Assert.Equal("UNKNOWN", response.DeviceDescriptor.RelationStatus.ToString().ToUpper());
      Assert.Equal("Unknown", response.DeviceDescriptor.TccDeviceStatus.ToString());
      Assert.Null(response.DeviceDescriptor.ShortRaptorAssetId);
    }

    [Fact]
    public async Task GetDevice_InternalException_UnhappyPath()
    {
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));

      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceBySerialNumber(It.IsAny<string>(), _customHeaders))
        .ThrowsAsync(exception);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceBySerialExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,cwsDeviceClient: cwsDeviceClient.Object);
      var ex = await Assert. ThrowsAsync<ServiceException>(() => getDeviceBySerialExecutor.ProcessAsync(new DeviceSerial(_serialNumber)));

      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(104, ex.GetResult.Code);
    }
  }
}
