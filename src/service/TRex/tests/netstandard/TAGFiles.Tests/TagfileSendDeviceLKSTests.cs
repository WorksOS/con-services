using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.Tests.TestFixtures;
using VSS.WebApi.Common;
using Xunit;

namespace TAGFiles.Tests
{
  public class TagfileSendDeviceLKSTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void DeviceLKS_DIMocking()
    {
      SetupDIDeviceGateay(true);

      var config = DIContext.Obtain<IConfigurationStore>();

      var isDeviceGatewayEnabled = config.GetValueBool("ENABLE_DEVICE_GATEWAY");
      Assert.True(isDeviceGatewayEnabled);
    }
   
    [Fact]
    public async Task DeviceLKS_CB460_SentOk()
    {
      var projectUid = Guid.NewGuid();
      SetupDIDeviceGateay(true);

      byte[] tagContent;
      using (var tagFileStream =
        new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"),
          FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int)tagFileStream.Length);
      }

      var td = new TagFileDetail()
      {
        assetId = null,
        projectId = projectUid,
        tagFileName = "Test.tag",
        tagFileContent = tagContent,
        tccOrgId = "",
        IsJohnDoe = false
      };

      var tagFilePreScan = new TAGFilePreScan();
      await using (var stream = new MemoryStream(td.tagFileContent))
        tagFilePreScan.Execute(stream);
      Assert.NotNull(tagFilePreScan);
      Assert.Equal("0523J019SW", tagFilePreScan.HardwareID);

      var executor = new SubmitTAGFileExecutor();
      executor.SendDeviceStatusToDeviceGateway(td, tagFilePreScan);
    }
   
    private void SetupDIDeviceGateay(bool enableDeviceGateway = true)
    {
      var moqCustomHeaders = new HeaderDictionary() {};

      var moqTPaaSApplicationAuthentication = new Mock<ITPaaSApplicationAuthentication>();
      var moqCwsDeviceGateway = new Mock<ICwsDeviceGatewayClient>();

      moqTPaaSApplicationAuthentication.Setup(mk => mk.CustomHeaders()).Returns(moqCustomHeaders);
      moqCwsDeviceGateway.Setup(mk => mk.CreateDeviceLKS(It.IsAny<string>(), It.IsAny<DeviceLKSModel>(), It.IsAny<HeaderDictionary>()));
     
      //Moq doesn't support extension methods in IConfiguration/Root.
      var moqConfiguration = DIContext.Obtain<Mock<IConfigurationStore>>();
      moqConfiguration.Setup(x => x.GetValueBool("ENABLE_DEVICE_GATEWAY", It.IsAny<bool>())).Returns(enableDeviceGateway);
      moqConfiguration.Setup(x => x.GetValueBool("ENABLE_DEVICE_GATEWAY")).Returns(enableDeviceGateway);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<ITPaaSApplicationAuthentication>(moqTPaaSApplicationAuthentication.Object))
        .Add(x => x.AddSingleton<ICwsDeviceGatewayClient>(moqCwsDeviceGateway.Object))
        .Add(x => x.AddSingleton<IConfigurationStore>(moqConfiguration.Object))
        .Build();
    }
  }
}
