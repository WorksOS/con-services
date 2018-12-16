using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class TagfileValidatorTests : IClassFixture<DILoggingFixture>
  {
    private static Guid NewSiteModelGuidTfa = Guid.NewGuid();
    private static object Lock = new object();

    [Fact]
    public void Test_TFASpecific_DIMocking()
    {
      SetupDITfa(true);

      var config = DIContext.Obtain<IConfigurationStore>();

      var tfaServiceEnabled = config.GetValueBool("ENABLE_TFA_SERVICE");
      Assert.True(tfaServiceEnabled);

      var tfaServiceUrl = config.GetValueString("TFA_PROJECTV2_API_URL");
      Assert.Equal("http://localhost:5001/api/v2/project", tfaServiceUrl);

      var minTagFileLength = config.GetValueInt("MIN_TAGFILE_LENGTH");
      Assert.Equal(100, minTagFileLength);
    }


    [Fact]
    public async Task Test_InvalidTagFile_TooSmall()
    {
      SetupDITfa(false);

      TagFileDetail td = new TagFileDetail()
      {
        assetId = Guid.NewGuid(),
        projectId = Guid.NewGuid(),
        tagFileName = "Test.tag",
        tagFileContent = new byte[1],
        tccOrgId = "",
        IsJohnDoe = false
      };

      // Validate tagfile 
      var result = await TagfileValidator.ValidSubmission(td).ConfigureAwait(false);
      Assert.True(result.Code == (int) TRexTagFileResultCode.TRexInvalidTagfile, "Failed to return correct error code");
      Assert.Equal("TRexInvalidTagfile", result.Message);
    }

    [Fact]
    public async Task Test_InvalidTagFile_UnableToRead()
    {
      SetupDITfa(false);

      TagFileDetail td = new TagFileDetail()
      {
        assetId = Guid.NewGuid(),
        projectId = Guid.NewGuid(),
        tagFileName = "Test.tag",
        tagFileContent = new byte[101],
        tccOrgId = "",
        IsJohnDoe = false
      };

      // Validate tagfile
      var result = await TagfileValidator.ValidSubmission(td).ConfigureAwait(false);
      Assert.True(result.Code == (int)TRexTagFileResultCode.TrexTagFileReaderError, "Failed to return correct error code");
      Assert.Equal("InvalidValueTypeID", result.Message);
    }

    [Fact]
    public async Task Test_HasAssetUid_TfaByPassed()
    {
      // note that assetId is available, thus this comes from the test tool TagFileSubmitted,
      // and although TFA is enabled, it won't be called
      SetupDITfa();

      byte[] tagContent;
      using (FileStream tagFileStream =
        new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"),
          FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int) tagFileStream.Length);
      }

      TagFileDetail td = new TagFileDetail()
      {
        assetId = Guid.NewGuid(),
        projectId = Guid.NewGuid(),
        tagFileName = "Test.tag",
        tagFileContent = tagContent,
        tccOrgId = "",
        IsJohnDoe = false
      };

      var result = await TagfileValidator.ValidSubmission(td).ConfigureAwait(false);
      Assert.True(result.Code == (int) TRexTagFileResultCode.Valid, "Failed to return a Valid request");
      Assert.Equal("success", result.Message);
    }

    [Fact]
    public async Task Test_ValidateOk()
    {
      var projectUid = Guid.NewGuid();
      var timeOfPosition = DateTime.UtcNow;
      var moqRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid.ToString(), (int) DeviceTypeEnum.SNM940, string.Empty, string.Empty, 0, 0, timeOfPosition);
      var moqResult = GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(projectUid.ToString(), string.Empty, 0, "success");
      SetupDITfa(true, moqRequest, moqResult);

      byte[] tagContent;
      using (FileStream tagFileStream =
        new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"),
          FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int) tagFileStream.Length);
      }

      TagFileDetail td = new TagFileDetail()
      {
        assetId = null,
        projectId = projectUid,
        tagFileName = "Test.tag",
        tagFileContent = tagContent,
        tccOrgId = "",
        IsJohnDoe = false
      };

      var result = await TagfileValidator.ValidSubmission(td).ConfigureAwait(false);
      Assert.True(result.Code == (int) TRexTagFileResultCode.Valid, "Failed to return a Valid request");
      Assert.Equal("success", result.Message);
    }

    [Fact]
    public async Task Test_ValidateFailed_InvalidManualProjectType()
    {
      var projectUid = Guid.NewGuid();
      var timeOfPosition = DateTime.UtcNow;
      var moqRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid.ToString(), (int) DeviceTypeEnum.SNM940, string.Empty, string.Empty, 0, 0, timeOfPosition);
      var moqResult = GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(string.Empty, string.Empty, 3044, "Manual Import: cannot import to a Civil type project");
      SetupDITfa(true, moqRequest, moqResult);

      byte[] tagContent;
      using (FileStream tagFileStream =
        new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"),
          FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int) tagFileStream.Length);
      }

      TagFileDetail td = new TagFileDetail()
      {
        assetId = null,
        projectId = projectUid,
        tagFileName = "Test.tag",
        tagFileContent = tagContent,
        tccOrgId = "",
        IsJohnDoe = false
      };

      var result = await TagfileValidator.ValidSubmission(td).ConfigureAwait(false);
      Assert.True(result.Code == 3044, "Failed to return correct error code");
      Assert.Equal("Manual Import: cannot import to a Civil type project", result.Message);
    }


    [Fact(Skip = "Requires live Ignite node")]
    public void Test_TagFileArchive()
    {
      SetupDITfa();

      byte[] tagContent;
      using (FileStream tagFileStream =
        new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"),
          FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int) tagFileStream.Length);
      }

      TagFileDetail td = new TagFileDetail()
      {
        assetId = Guid.Parse("{00000000-0000-0000-0000-000000000001}"),
        projectId = Guid.Parse("{00000000-0000-0000-0000-000000000001}"),
        tagFileName = "Test.tag",
        tagFileContent = tagContent,
        tccOrgId = "",
        IsJohnDoe = false
      };

      Assert.True(TagFileRepository.ArchiveTagfile(td), "Failed to archive tagfile");
    }

    private void SetupDITfa(bool enableTfaService = true, GetProjectAndAssetUidsRequest getProjectAndAssetUidsRequest = null, GetProjectAndAssetUidsResult getProjectAndAssetUidsResult = null)
    {
      // this setup includes the DITagFileFixture. Done here to try to avoid random test failures.
      lock (Lock)
      {
        var moqStorageProxy = new Mock<IStorageProxy>();

        var moqStorageProxyFactory = new Mock<IStorageProxyFactory>();
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Immutable)).Returns(moqStorageProxy.Object);
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Mutable)).Returns(moqStorageProxy.Object);
        moqStorageProxyFactory.Setup(mk => mk.MutableGridStorage()).Returns(moqStorageProxy.Object);
        moqStorageProxyFactory.Setup(mk => mk.ImmutableGridStorage()).Returns(moqStorageProxy.Object);

        var moqSurveyedSurfaces = new Mock<ISurveyedSurfaces>();

        var moqSiteModels = new Mock<ISiteModels>();
        moqSiteModels.Setup(mk => mk.StorageProxy).Returns(moqStorageProxy.Object);

        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
          .Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))
          .Add(x => x.AddSingleton<ISiteModels>(moqSiteModels.Object))

          .Add(x => x.AddSingleton<ISurveyedSurfaces>(moqSurveyedSurfaces.Object))
          .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
          .Build();

        ISiteModel mockedSiteModel = new SiteModel(NewSiteModelGuidTfa);

        var moqSiteModelFactory = new Mock<ISiteModelFactory>();
        moqSiteModelFactory.Setup(mk => mk.NewSiteModel()).Returns(mockedSiteModel);
        moqSiteModelFactory.Setup(mk => mk.NewSiteModel(NewSiteModelGuidTfa)).Returns(mockedSiteModel);

        moqSiteModels.Setup(mk => mk.GetSiteModel(NewSiteModelGuidTfa)).Returns(mockedSiteModel);

        // Mock the new sitemodel creation API to return just a new sitemodel
        moqSiteModels.Setup(mk => mk.GetSiteModel(moqStorageProxy.Object, NewSiteModelGuidTfa, true)).Returns(mockedSiteModel);

        //Moq doesn't support extention methods in IConfiguration/Root.
        var moqConfiguration = new Mock<IConfigurationStore>();
        var moqMinTagFileLength = 100;
        string moqTfaServiceUrl = "http://localhost:5001/api/v2/project";
        moqConfiguration.Setup(x => x.GetValueBool("ENABLE_TFA_SERVICE", It.IsAny<bool>())).Returns(enableTfaService);
        moqConfiguration.Setup(x => x.GetValueBool("ENABLE_TFA_SERVICE")).Returns(enableTfaService);
        moqConfiguration.Setup(x => x.GetValueInt("MIN_TAGFILE_LENGTH", It.IsAny<int>())).Returns(moqMinTagFileLength);
        moqConfiguration.Setup(x => x.GetValueInt("MIN_TAGFILE_LENGTH")).Returns(moqMinTagFileLength);
        moqConfiguration.Setup(x => x.GetValueString("TFA_PROJECTV2_API_URL")).Returns(moqTfaServiceUrl);

        var moqTfaProxy = new Mock<ITagFileAuthProjectProxy>();
        if (enableTfaService && getProjectAndAssetUidsRequest != null)
          moqTfaProxy.Setup(x => x.GetProjectAndAssetUids(It.IsAny<GetProjectAndAssetUidsRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(getProjectAndAssetUidsResult);

        DIBuilder
          .Continue()
          .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
          .Add(x => x.AddSingleton<IConfigurationStore>(moqConfiguration.Object))
          .Add(x => x.AddSingleton<ITagFileAuthProjectProxy>(moqTfaProxy.Object))
          .Complete();
      }
    }
  }
}
