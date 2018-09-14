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
using VSS.TRex.DI;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class TagfileValidatorTests : IClassFixture<DILoggingFixture>
  {

    [Fact]
    public void Test_TagfileValidator_DIMocking()
    {
      SetupDI(true);
      
      var config = DIContext.Obtain<IConfigurationStore>();

      var tfaServiceEnabled = config.GetValueBool("ENABLE_TFA_SERVICE");
      Assert.True(tfaServiceEnabled);

      var tfaServiceUrl = config.GetValueString("TFA_PROJECTV2_API_URL");
      Assert.Equal("http://localhost:5001/api/v2/project", tfaServiceUrl);

      var minTagFileLength = config.GetValueInt("MIN_TAGFILE_LENGTH");
      Assert.Equal(100, minTagFileLength);
    }

    //[Fact]
    //public void Test_TFAProxy_GoodRequest()
    //{
    //  var projectUid = Guid.NewGuid();
    //  var timeOfPosition = DateTime.UtcNow;
    //  var moqRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid.ToString(), (int) DeviceTypeEnum.SNM940, String.Empty, string.Empty, 0, 0, timeOfPosition);
    //  var moqResult = GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(projectUid.ToString(), string.Empty);
    //  SetupDI(true, moqRequest, moqResult);

    //  var result = TagfileValidator.ValidateWithTfa(moqRequest);

    //  Assert.Equal(0, result.Result.Code);
    //  Assert.Equal(moqRequest.ProjectUid, result.Result.ProjectUid);
    //}

    [Fact]
    public void Test_TFAValidator_NoSerialOrTccOrdId()
    {
      //Log.LogWarning($"Must have either a valid TCCOrgID or RadioSerialNo or ProjectID");
      //return GetValidationResultName(ValidationResult.BadRequest, ref tfaMessage, ref tfaCode);
      //CheckFileIsProcessible
    }

    [Fact]
    public async Task Test_TagfileValidator_TestInvalidTagFileTooSmall()
    {
      SetupDI(false);

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
      Assert.True(result.Code == (int)ValidationResult.InvalidTagfile, "Failed to return a Invalid request");
      Assert.Equal("InvalidTagfile", result.Message);
    }

    [Fact]
    public async Task Test_TagfileValidator_TestInvalidEmptyTagFile()
    {
      SetupDI(false);

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
      Assert.True(result.Code == (int)ValidationResult.InvalidTagfile, "Failed to return a Invalid request");
      Assert.Equal("InvalidTagfile", result.Message);
    }

    [Fact] // (Skip = "Requires live Ignite node")]
    public async Task Test_TagfileValidator_TestValidTagFile_TfaByPassed()
    {
      // ahhhh you can't go into the TAGfile.Read without having all kinds of stuff mocked e.g. 
      // note that assetId is available, thus this comes from the test tool TagFileSubmitted,
      // and although TFA is enabled, it won't be called
      SetupDI();

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

      // Validate tagfile
      var result = await TagfileValidator.ValidSubmission(td).ConfigureAwait(false);
      Assert.True(result.Code == (int) ValidationResult.Valid, "Failed to return a Valid request");
      Assert.Equal("InvalidTagfile", result.Message);
    }

    [Fact(Skip = "Requires live Ignite node")]
    public void Test_TagfileValidator_TestTagFileArchive()
    {
      SetupDI();

      byte[] tagContent;
      using (FileStream tagFileStream =
              new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"),
                      FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int)tagFileStream.Length);
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

    private void SetupDI(bool enableTfaService = true, GetProjectAndAssetUidsRequest getProjectAndAssetUidsRequest = null, GetProjectAndAssetUidsResult getProjectAndAssetUidsResult = null)
    {
      //Moq doesn't support extention methods in IConfiguration/Root.
      var moqConfiguration = new Mock<IConfigurationStore>();
      var moqMinTagFileLength = 100;
      string moqTfaServiceUrl = "http://localhost:5001/api/v2/project";
      moqConfiguration.Setup(x => x.GetValueBool("ENABLE_TFA_SERVICE")).Returns(enableTfaService);
      moqConfiguration.Setup(x => x.GetValueInt("MIN_TAGFILE_LENGTH")).Returns(moqMinTagFileLength);
      moqConfiguration.Setup(x => x.GetValueString("TFA_PROJECTV2_API_URL")).Returns(moqTfaServiceUrl);

      // this is needed for the moqSiteModelFactory x.NewSiteModel setup 
      var moqSurveyedSurfaces = new Mock<ISurveyedSurfaces>();
      DIBuilder.New()
        .Add(x => x.AddSingleton<ISurveyedSurfaces>(moqSurveyedSurfaces.Object))
        .Complete();

      var moqSurveyedSurfaceFactory = new Mock<ISurveyedSurfaceFactory>();
      var moqSiteModelFactory = new Mock<ISiteModelFactory>();
      moqSiteModelFactory.Setup(x => x.NewSiteModel()).Returns(new SiteModel(Guid.NewGuid()));

      var moqTfaProxy = new Mock<ITagFileAuthProjectProxy>();
      if (enableTfaService && getProjectAndAssetUidsRequest != null)
        moqTfaProxy.Setup(x => x.GetProjectAndAssetUids(getProjectAndAssetUidsRequest, It.IsAny<IDictionary<string, string>>())).ReturnsAsync(getProjectAndAssetUidsResult);

      /* TFAFile.Read() requires so much mocking. Do we want to go down this rabbit hole, which whill be hard to maintain
      var moqSiteModels = new Mock<ISiteModels>();
      var moqStorageProxy = new Mock<IStorageProxy>();
      var moqStartEndProductionEvents = new Mock<IStartEndProductionEvents>();
      var moqproductionEventLists = new Mock<IProductionEventLists>();
      var moqProductionEventsFactory = new Mock<IProductionEventsFactory>();
      moqProductionEventsFactory.Setup(x => x.NewEventList(It.IsAny<short>(), It.IsAny<Guid>(), It.IsAny<ProductionEventType>())).Returns(new StartEndProductionEvents());
      moqStartEndProductionEvents.Setup(x => x.PutValueAtDate(It.IsAny<DateTime>(), It.IsAny<ProductionEventType>()));
      moqStartEndProductionEvents.Setup(x => x.LoadFromStore(It.IsAny<IStorageProxy>()));
      //moqprductionEventLists.Setup(x => x.StartEndRecordedDataEvents());
      */

      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore>(moqConfiguration.Object))
        .Add(x => x.AddSingleton<ITagFileAuthProjectProxy>(moqTfaProxy.Object))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(moqSurveyedSurfaceFactory.Object))
        .Add(x => x.AddSingleton<ISiteModelFactory>(moqSiteModelFactory.Object))
        .Add(x => x.AddSingleton<ISurveyedSurfaces>(moqSurveyedSurfaces.Object))

        /* TFAFile.Read() requirements (NOT complete)
        .Add(x => x.AddSingleton<ISiteModels>(moqSiteModels.Object))
        .Add(x => x.AddSingleton<IStorageProxy>(moqStorageProxy.Object))
        .Add(x => x.AddSingleton<IStartEndProductionEvents>(moqStartEndProductionEvents.Object))
        .Add(x => x.AddSingleton<IProductionEventLists>(moqproductionEventLists.Object))
        .Add(x => x.AddSingleton<IProductionEventsFactory>(moqProductionEventsFactory.Object))
        */
        .Complete();
    }
  }
}
