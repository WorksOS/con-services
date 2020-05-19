using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Clients.CWS;
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
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class GetDeviceByShortRaptorIdExecutorTests
  {
    private IHeaderDictionary _customHeaders;
    private IConfigurationStore _configStore;
    private ILoggerFactory _logger;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private IServiceProvider ServiceProvider;
    private IServiceExceptionHandler ServiceExceptionHandler;

    private string _customerUid;
    private string _deviceUid;
    private string _deviceName;
    private string _serialNumber;
    private RelationStatusEnum _relationStatus;
    private TCCDeviceStatusEnum _tccDeviceStatus;
    private int _shortRaptorAssetId;


    public GetDeviceByShortRaptorIdExecutorTests()
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
      _customHeaders = new HeaderDictionary();

      _customerUid = Guid.NewGuid().ToString();
      _deviceUid = Guid.NewGuid().ToString();
      _deviceName = "the Device Name";
      _serialNumber = "67567576SN";
      _relationStatus = RelationStatusEnum.Active;
      _tccDeviceStatus = TCCDeviceStatusEnum.Registered;
      _shortRaptorAssetId = 4445555;
    }

    [Fact]
    public async Task GetDevice_HappyPath()
    {
      var cwsDevice = new DeviceResponseModel()
      {
        TRN = TRNHelper.MakeTRN(_deviceUid, TRNHelper.TRN_DEVICE),
        DeviceName = _deviceName, SerialNumber = _serialNumber
      };
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceByDeviceUid(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDevice);
      var cwsDeviceAccountList = new DeviceAccountListResponseModel()
      {
        Accounts = new List<DeviceAccountResponseModel>()
        {
          new DeviceAccountResponseModel()
          {
            TRN = TRNHelper.MakeTRN(_customerUid, TRNHelper.TRN_ACCOUNT),
            AccountName = "the customer name", RelationStatus = _relationStatus, TccDeviceStatus = _tccDeviceStatus
          }
        }
      };
      cwsDeviceClient.Setup(pr => pr.GetAccountsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDeviceAccountList);

      var deviceLocalDb = new Device() {DeviceUID = _deviceUid, ShortRaptorAssetID = _shortRaptorAssetId};
      var deviceRepo = new Mock<IDeviceRepository>();
      deviceRepo.Setup(pr => pr.GetDevice(It.IsAny<int>()))
        .ReturnsAsync(deviceLocalDb);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceByShortRaptorIdExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        deviceRepo: deviceRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new ShortRaptorId(_shortRaptorAssetId))
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
      Assert.Equal(_shortRaptorAssetId, response.DeviceDescriptor.ShortRaptorAssetId);
    }

    [Fact]
    public async Task GetDevice_2Accounts1Active_HappyPath()
    {
      var cwsDevice = new DeviceResponseModel()
      {
        TRN = TRNHelper.MakeTRN(_deviceUid, TRNHelper.TRN_DEVICE),
        DeviceName = _deviceName, SerialNumber = _serialNumber
      };
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceByDeviceUid(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDevice);
      var cwsDeviceAccountList = new DeviceAccountListResponseModel {Accounts = new List<DeviceAccountResponseModel>() {new DeviceAccountResponseModel()
      {
        TRN = TRNHelper.MakeTRN(_customerUid,TRNHelper.TRN_ACCOUNT),
        AccountName = "the customer name", RelationStatus = _relationStatus, TccDeviceStatus = _tccDeviceStatus
      }, new DeviceAccountResponseModel()
      {
        TRN = TRNHelper.MakeTRN(Guid.NewGuid(), TRNHelper.TRN_ACCOUNT),
        AccountName = "the other customer name", RelationStatus = RelationStatusEnum.Pending, TccDeviceStatus = _tccDeviceStatus
      }}};
      cwsDeviceClient.Setup(pr => pr.GetAccountsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDeviceAccountList);

      var deviceLocalDb = new Device() {DeviceUID = _deviceUid, ShortRaptorAssetID = _shortRaptorAssetId};
      var deviceRepo = new Mock<IDeviceRepository>();
      deviceRepo.Setup(pr => pr.GetDevice(It.IsAny<int>()))
        .ReturnsAsync(deviceLocalDb);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceByShortRaptorIdExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        deviceRepo: deviceRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new ShortRaptorId(_shortRaptorAssetId))
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
      Assert.Equal(_shortRaptorAssetId, response.DeviceDescriptor.ShortRaptorAssetId);
    }

    [Fact]
    public async Task GetDevice_DeviceNotFoundInCws_UnhappyPath()
    {
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceByDeviceUid(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync((DeviceResponseModel) null);

      var deviceLocalDb = new Device() { DeviceUID = _deviceUid, ShortRaptorAssetID = _shortRaptorAssetId };
      var deviceRepo = new Mock<IDeviceRepository>();
      deviceRepo.Setup(pr => pr.GetDevice(It.IsAny<int>()))
        .ReturnsAsync(deviceLocalDb);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceByShortRaptorIdExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        deviceRepo: deviceRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new ShortRaptorId(_shortRaptorAssetId))
        as DeviceDescriptorSingleResult;

      Assert.NotNull(response);
      Assert.Equal(101, response.Code);
      Assert.Equal("Unable to locate device by serialNumber in cws", response.Message);

      Assert.NotNull(response.DeviceDescriptor);
      Assert.Equal(101, response.DeviceDescriptor.Code);
      Assert.Null(response.DeviceDescriptor.CustomerUID);
      Assert.Equal(_deviceUid, response.DeviceDescriptor.DeviceUID);
      Assert.Null(response.DeviceDescriptor.DeviceName);
      Assert.Null(response.DeviceDescriptor.SerialNumber);
      Assert.Equal("UNKNOWN", response.DeviceDescriptor.RelationStatus.ToString().ToUpper());
      Assert.Equal("Unknown", response.DeviceDescriptor.TccDeviceStatus.ToString());
      Assert.Equal(_shortRaptorAssetId, response.DeviceDescriptor.ShortRaptorAssetId);
    }

    [Fact]
    public async Task GetDevice_DeviceNotFoundInDb_UnhappyPath()
    {
      var cwsDevice = new DeviceResponseModel
      {
        TRN = TRNHelper.MakeTRN(_deviceUid, TRNHelper.TRN_DEVICE),
        DeviceName = _deviceName, SerialNumber = _serialNumber
      };
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceByDeviceUid(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDevice);
    
      var deviceRepo = new Mock<IDeviceRepository>();
      deviceRepo.Setup(pr => pr.GetDevice(It.IsAny<int>()))
        .ReturnsAsync((Device)null);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceByShortRaptorIdExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        deviceRepo: deviceRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new ShortRaptorId(_shortRaptorAssetId))
        as DeviceDescriptorSingleResult;

      Assert.NotNull(response);
      Assert.Equal(100, response.Code);
      Assert.Equal("Unable to locate device in localDB", response.Message);
    }

    [Fact]
    public async Task GetDevice_NoAccountFound_UnhappyPath()
    {
      var cwsDevice = new DeviceResponseModel()
      {
        TRN = TRNHelper.MakeTRN(_deviceUid, TRNHelper.TRN_DEVICE),
        DeviceName = _deviceName, SerialNumber = _serialNumber
      };
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceByDeviceUid(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDevice);
      cwsDeviceClient.Setup(pr => pr.GetAccountsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync((DeviceAccountListResponseModel) null);

      var deviceLocalDb = new Device() { DeviceUID = _deviceUid, ShortRaptorAssetID = _shortRaptorAssetId };
      var deviceRepo = new Mock<IDeviceRepository>();
      deviceRepo.Setup(pr => pr.GetDevice(It.IsAny<int>()))
        .ReturnsAsync(deviceLocalDb);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceByShortRaptorIdExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        deviceRepo: deviceRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new ShortRaptorId(_shortRaptorAssetId))
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
      Assert.Equal(_shortRaptorAssetId, response.DeviceDescriptor.ShortRaptorAssetId);
    }

    [Fact]
    public async Task GetDevice_TooManyActiveAccounts_UnhappyPath()
    {
      var cwsDevice = new DeviceResponseModel()
      {
        TRN = TRNHelper.MakeTRN(_deviceUid, TRNHelper.TRN_DEVICE),
        DeviceName = _deviceName, SerialNumber = _serialNumber
      };
      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceByDeviceUid(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDevice);
      var cwsDeviceAccountList = new DeviceAccountListResponseModel() { Accounts = new List<DeviceAccountResponseModel>()
        {
          new DeviceAccountResponseModel()
          {
            TRN = TRNHelper.MakeTRN(_customerUid,TRNHelper.TRN_ACCOUNT),
            AccountName = "the customer name", RelationStatus = _relationStatus, TccDeviceStatus = _tccDeviceStatus
          },
          new DeviceAccountResponseModel()
          {
            TRN = TRNHelper.MakeTRN(Guid.NewGuid().ToString(),TRNHelper.TRN_ACCOUNT),
            AccountName = "the other customer name", RelationStatus = _relationStatus, TccDeviceStatus = _tccDeviceStatus
          }
        }
      };
      cwsDeviceClient.Setup(pr => pr.GetAccountsForDevice(It.IsAny<Guid>(), _customHeaders))
        .ReturnsAsync(cwsDeviceAccountList);

      var deviceLocalDb = new Device() { DeviceUID = _deviceUid, ShortRaptorAssetID = _shortRaptorAssetId };
      var deviceRepo = new Mock<IDeviceRepository>();
      deviceRepo.Setup(pr => pr.GetDevice(It.IsAny<int>()))
        .ReturnsAsync(deviceLocalDb);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceByShortRaptorIdExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        deviceRepo: deviceRepo.Object, cwsDeviceClient: cwsDeviceClient.Object);
      var response = await getDeviceBySerialExecutor.ProcessAsync(new ShortRaptorId(_shortRaptorAssetId))
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
      Assert.Equal(_shortRaptorAssetId, response.DeviceDescriptor.ShortRaptorAssetId);
    }

    [Fact]
    public async Task GetDevice_InternalException_UnhappyPath()
    {
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));

      var cwsDeviceClient = new Mock<ICwsDeviceClient>();
      cwsDeviceClient.Setup(pr => pr.GetDeviceByDeviceUid(It.IsAny<Guid>(), _customHeaders))
        .ThrowsAsync(exception);

      var getDeviceBySerialExecutor = RequestExecutorContainerFactory.Build<GetDeviceByShortRaptorIdExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        headers: _customHeaders,
        deviceRepo: null, cwsDeviceClient: cwsDeviceClient.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(() => getDeviceBySerialExecutor.ProcessAsync(new ShortRaptorId(_shortRaptorAssetId)));

      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(104, ex.GetResult.Code);
    }
  }
}
