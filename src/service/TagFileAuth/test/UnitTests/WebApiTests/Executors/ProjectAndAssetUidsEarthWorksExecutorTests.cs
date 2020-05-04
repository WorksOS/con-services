using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectAndAssetUidsEarthWorksExecutorTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;
    private string _projectUidToBeDiscovered;
    private string _deviceUid;
    private DateTime _timeOfLocation;
    private string _deviceCustomerUid;

    private GetProjectAndAssetUidsEarthWorksRequest _projectAndAssetUidsEarthWorksRequest;
    private string radioSerial = "radSer45";
    private string ec520Serial = "ecSer";
    private DeviceData deviceData;
    private IDictionary<string, string> _customHeaders;


    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();

      _projectUidToBeDiscovered = Guid.NewGuid().ToString();
      _deviceUid = Guid.NewGuid().ToString();
      _timeOfLocation = DateTime.UtcNow;
      _deviceCustomerUid = Guid.NewGuid().ToString();
      _projectAndAssetUidsEarthWorksRequest = new GetProjectAndAssetUidsEarthWorksRequest(string.Empty, radioSerial, 80, 160, _timeOfLocation);
      _loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
      deviceData = new DeviceData {CustomerUID = _deviceCustomerUid, DeviceUID = _deviceUid};
    }

    [TestMethod]
    public async Task ProjectUidExecutor_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, authorizationProxy.Object);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => executor.ProcessAsync((GetProjectAndAssetUidsEarthWorksRequest) null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_NoEC520DeviceFound()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      deviceProxy.Setup(d => d.GetDevice(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync((DeviceData) null);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, authorizationProxy.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, string.Empty, string.Empty, string.Empty, false, 3033);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_NoCBorEC520DeviceFound()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.RadioSerial = radioSerial;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      deviceProxy.Setup(d => d.GetDevice(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync((DeviceData) null);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, authorizationProxy.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, string.Empty, string.Empty, string.Empty, false, 3033);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_MultiProjects()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      // device whose account has licenses
      var deviceLicenseResponseModel = new DeviceLicenseResponseModel() {Total = 2};
      deviceProxy.Setup(d => d.GetDevice(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(deviceData);
      cwsAccountClient.Setup(a => a.GetDeviceLicenses(new Guid(deviceData.CustomerUID), null)).ReturnsAsync(deviceLicenseResponseModel);
      var projects = new ProjectDataResult() {ProjectDescriptors = new List<ProjectData> {new ProjectData {ProjectUID = _projectUidToBeDiscovered, CustomerUID = _deviceCustomerUid, ProjectType = ProjectType.Standard, Name = "thisProject"}, new ProjectData {ProjectUID = _projectUidToBeDiscovered, CustomerUID = _deviceCustomerUid, ProjectType = ProjectType.Standard, Name = "otherProject"}}};
      projectProxy.Setup(d => d.GetIntersectingProjects(_deviceCustomerUid, It.IsAny<double>(), It.IsAny<double>(), null, null)).ReturnsAsync(projects);
      deviceProxy.Setup(d => d.GetProjectsForDevice(_deviceUid, null)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, authorizationProxy.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, string.Empty, _deviceUid, _deviceCustomerUid, true, 3049);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_NoMatchingProjects()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      // device whose account has licenses
      var deviceLicenseResponseModel = new DeviceLicenseResponseModel() {Total = 2};
      deviceProxy.Setup(d => d.GetDevice(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(deviceData);
      cwsAccountClient.Setup(a => a.GetDeviceLicenses(new Guid(deviceData.CustomerUID), null)).ReturnsAsync(deviceLicenseResponseModel);

      projectProxy.Setup(d => d.GetIntersectingProjects(_deviceCustomerUid, It.IsAny<double>(), It.IsAny<double>(), null, null)).ReturnsAsync(new ProjectDataResult());
      deviceProxy.Setup(d => d.GetProjectsForDevice(_deviceUid, null)).ReturnsAsync(new ProjectDataResult());

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, authorizationProxy.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, string.Empty, _deviceUid, _deviceCustomerUid, false, 3044);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_DeviceNoAccessToMatchingProjects()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      // device whose account has licenses
      var deviceLicenseResponseModel = new DeviceLicenseResponseModel() {Total = 2};
      deviceProxy.Setup(d => d.GetDevice(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(deviceData);
      cwsAccountClient.Setup(a => a.GetDeviceLicenses(new Guid(deviceData.CustomerUID), null)).ReturnsAsync(deviceLicenseResponseModel);
      var projects = new ProjectDataResult() {ProjectDescriptors = new List<ProjectData> {new ProjectData {ProjectUID = _projectUidToBeDiscovered, CustomerUID = _deviceCustomerUid, ProjectType = ProjectType.Standard, Name = "thisProject"}, new ProjectData {ProjectUID = _projectUidToBeDiscovered, CustomerUID = _deviceCustomerUid, ProjectType = ProjectType.Standard, Name = "otherProject"}}};
      projectProxy.Setup(d => d.GetIntersectingProjects(_deviceCustomerUid, It.IsAny<double>(), It.IsAny<double>(), null, null)).ReturnsAsync(projects);
      deviceProxy.Setup(d => d.GetProjectsForDevice(_deviceUid, null)).ReturnsAsync(new ProjectDataResult());

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, authorizationProxy.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, string.Empty, _deviceUid, _deviceCustomerUid, false, 3045);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_DeviceAccountHasLicenses()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      // device whose account has licenses
      var deviceDeviceLicenseResponseModel = new DeviceLicenseResponseModel() {Total = 2};
      deviceProxy.Setup(d => d.GetDevice(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(deviceData);
      cwsAccountClient.Setup(a => a.GetDeviceLicenses(new Guid(deviceData.CustomerUID), null)).ReturnsAsync(deviceDeviceLicenseResponseModel);
      var projects = new ProjectDataResult() {ProjectDescriptors = new List<ProjectData>() {new ProjectData() {ProjectUID = _projectUidToBeDiscovered, CustomerUID = _deviceCustomerUid, ProjectType = ProjectType.Standard}}};
      projectProxy.Setup(d => d.GetIntersectingProjects(_deviceCustomerUid, It.IsAny<double>(), It.IsAny<double>(), null, null)).ReturnsAsync(projects);
      deviceProxy.Setup(d => d.GetProjectsForDevice(_deviceUid, null)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, authorizationProxy.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, _projectUidToBeDiscovered, _deviceUid, _deviceCustomerUid, true, 0);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_DeviceAccountHasNoLicenses()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      // device whose account has licenses
      var deviceLicenseResponseModel = new DeviceLicenseResponseModel() {Total = 0};
      deviceProxy.Setup(d => d.GetDevice(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(deviceData);
      cwsAccountClient.Setup(a => a.GetDeviceLicenses(new Guid(deviceData.CustomerUID), null)).ReturnsAsync(deviceLicenseResponseModel);
      var projects = new ProjectDataResult() {ProjectDescriptors = new List<ProjectData>() {new ProjectData() {ProjectUID = _projectUidToBeDiscovered, CustomerUID = _deviceCustomerUid, ProjectType = ProjectType.Standard}}};
      projectProxy.Setup(d => d.GetIntersectingProjects(_deviceCustomerUid, It.IsAny<double>(), It.IsAny<double>(), null, null))
        .ReturnsAsync(projects);
      deviceProxy.Setup(d => d.GetProjectsForDevice(_deviceUid, null)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, authorizationProxy.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, _projectUidToBeDiscovered, _deviceUid, _deviceCustomerUid, false, 0);
    }

    private void ValidateResult(GetProjectAndAssetUidsEarthWorksResult result, string expectedProjectUid, string expectedAssetUid, string expectedCustomerUid, bool expectedHasValidSubscription, int expectedCode)
    {
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(expectedProjectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(expectedAssetUid, result.AssetUid, "executor returned incorrect DeviceUid");
      Assert.AreEqual(expectedCustomerUid, result.CustomerUid, "executor returned incorrect CustomerUid");
      Assert.AreEqual(expectedHasValidSubscription, result.HasValidSub, "executor returned incorrect HasValidSub");
      Assert.AreEqual(expectedCode, result.Code, "executor returned incorrect result code");
    }
  }
}
