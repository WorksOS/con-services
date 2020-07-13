using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Events.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Requests;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Requests
{
  [UnitTestCoveredRequest(RequestType = typeof(DeleteSiteModelRequest))]
  public class DeleteSiteModelRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private static readonly ILogger _log = TRex.Logging.Logger.CreateLogger<DeleteSiteModelRequestTests>();

    private void AddApplicationGridRouting()
    {
      IgniteMock.Mutable.AddApplicationGridRouting<DeleteSiteModelRequestComputeFunc, DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>();
      IgniteMock.Mutable.AddApplicationGridRouting<AddTTMDesignComputeFunc, AddTTMDesignArgument, AddTTMDesignResponse>();
    }

    public DeleteSiteModelRequestTests(DITAGFileAndSubGridRequestsWithIgniteFixture fixture)
    {
      // This resets all modified content in the Ignite mocks between tests
      fixture.ClearDynamicFixtureContent();
      fixture.SetupFixture();
    }

    private bool IsModelEmpty(ISiteModel model, bool expectedToBeEmpty)
    {
      var clear1MutableCount = IgniteMock.Mutable.MockedCacheDictionaries.Values.Sum(cache => cache.Keys.Count);
      var clear1ImmutableCount = IgniteMock.Immutable.MockedCacheDictionaries.Values.Sum(cache => cache.Keys.Count);
     
      var clear1 = clear1MutableCount == 0 && clear1ImmutableCount == 0;

      if (expectedToBeEmpty && !clear1)
      {
        DumpModelContents($"Pre-commit empty check, clear1MutableCount = {clear1MutableCount}, clear1ImmutableCount = {clear1ImmutableCount}");
      }

      // Perform a belt and braces check to ensure there were no pending uncommitted changes.
      model.PrimaryStorageProxy.Mutability.Should().Be(StorageMutability.Mutable);
      model.PrimaryStorageProxy.ImmutableProxy.Should().NotBeNull();
      model.PrimaryStorageProxy.Commit();

      var clear2MutableCount = IgniteMock.Mutable.MockedCacheDictionaries.Values.Sum(cache => cache.Keys.Count);
      var clear2ImmutableCount = IgniteMock.Immutable.MockedCacheDictionaries.Values.Sum(cache => cache.Keys.Count);

      var clear2 = clear2MutableCount == 0 && clear2ImmutableCount == 0;

      if (expectedToBeEmpty && !(clear1 && clear2))
      {
        DumpModelContents("After full check, clear2MutableCount = {clear2MutableCount}, clear2ImmutableCount = {clear2ImmutableCount}");
      }

      return clear1 && clear2;
    }

    private void DumpModelContents(string title)
    {
      _log.LogInformation($"Model contents - {title}");

      // Log the contents
      _log.LogInformation("Mutable");
      IgniteMock.Mutable.MockedCacheDictionaries.ForEach(x =>
      {
        _log.LogInformation($"{x.Key}: {x.Value.Keys.Count} keys, {x.Value.Values.Count} values");
      });

      _log.LogInformation("Immutable");
      IgniteMock.Immutable.MockedCacheDictionaries.ForEach(x =>
      {
        _log.LogInformation($"{x.Key}: {x.Value.Keys.Count} keys, {x.Value.Values.Count} values");
      });
    }

    private void VerifyModelIsEmpty(ISiteModel model)
    {
      var isModelEmpty = IsModelEmpty(model, true);
      isModelEmpty.Should().BeTrue();
    }

    private void DeleteTheModel(ref ISiteModel model, DeleteSiteModelSelectivity selectivity, bool assertEmpty = true)
    {
      var modelId = model.ID;

      var request = new DeleteSiteModelRequest();
      var response = request.Execute(new DeleteSiteModelRequestArgument
      {
        ProjectID = modelId,
        Selectivity = selectivity
      });

      response.Result.Should().Be(DeleteSiteModelResult.OK);

      if (assertEmpty)
        VerifyModelIsEmpty(model);

      // Re-get the site model to support direct examinations in the case of partial deletions
      // This may return null, which means the site model no longer exists as an identifiable element in the persistent store
      model = DIContext.Obtain<ISiteModels>().GetSiteModel(model.ID);
    }

    private void SaveAndVerifyNotEmpty(ISiteModel model)
    {
      model.Machines.ForEach(x => model.MachinesTargetValues[x.InternalSiteModelMachineIndex]?.SaveMachineEventsToPersistentStore(model.PrimaryStorageProxy));
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);
      model.PrimaryStorageProxy.Commit();
      IsModelEmpty(model, false).Should().BeFalse();
    }

    [Fact]
    public void Creation()
    {
      var req = new DeleteSiteModelRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public void DeleteEmptyModel_StandardMetaDataPersistence()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.SaveMetadataToPersistentStore(model.PrimaryStorageProxy, true);
      IsModelEmpty(model, false).Should().BeFalse();

      DeleteTheModel(ref model, DeleteSiteModelSelectivity.All);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All, 0)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData, 1)]
    public void DeleteModel_WithMachines(DeleteSiteModelSelectivity selectivity, int expectedMachineCount)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.Machines.Add(new Machine("Test Delete Machine", "HardwareId", MachineType.Dozer, DeviceTypeEnum.SNM940, Guid.NewGuid(), 0, false));
      var _ = new SiteProofingRun("Test Proofing Run", 0, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, new BoundingWorldExtent3D(0, 0, 1, 1));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
      (model?.Machines.Count ?? 0).Should().Be(expectedMachineCount);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData)]
    public void DeleteModel_WithMachineEvents(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.MachinesTargetValues[0].AutoVibrationStateEvents.PutValueAtDate(DateTime.UtcNow, AutoVibrationState.Auto);
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
      (model?.MachinesTargetValues[0].AutoVibrationStateEvents.Count() ?? 0).Should().Be(0);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All, 0)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData, 1)]
    public void DeleteModel_WithMachineEvents_WithOverrideEvents_DesignOverride(DeleteSiteModelSelectivity selectivity, int expectedOverrideCount)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.MachinesTargetValues[0].AutoVibrationStateEvents.PutValueAtDate(DateTime.UtcNow, AutoVibrationState.Auto);
      model.MachinesTargetValues[0].DesignOverrideEvents.PutValueAtDate(DateTime.UtcNow, new OverrideEvent<int>(DateTime.UtcNow, 1));

      SaveAndVerifyNotEmpty(model);

      var request = new DeleteSiteModelRequest();
      var response = request.Execute(new DeleteSiteModelRequestArgument {ProjectID = model.ID, Selectivity = selectivity});

      response.Result.Should().Be(DeleteSiteModelResult.OK);

      if (selectivity == DeleteSiteModelSelectivity.All)
        VerifyModelIsEmpty(model);
      else
        IsModelEmpty(model, false).Should().BeFalse(); // Because the override event should not be removed for DeleteSiteModelSelectivity.TagFileDerivedData

      model = DIContext.Obtain<ISiteModels>().GetSiteModel(model.ID);

      (model?.MachinesTargetValues[0]?.AutoVibrationStateEvents.Count() ?? 0).Should().Be(0);
      if (selectivity == DeleteSiteModelSelectivity.TagFileDerivedData)
        model.MachinesTargetValues[0].DesignOverrideEvents.Count().Should().Be(expectedOverrideCount);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All, 0)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData, 1)]
    public void DeleteModel_WithMachineEvents_WithOverrideEvents_LayerOverride(DeleteSiteModelSelectivity selectivity, int expectedOverrideCount)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.MachinesTargetValues[0].AutoVibrationStateEvents.PutValueAtDate(DateTime.UtcNow, AutoVibrationState.Auto);
      model.MachinesTargetValues[0].LayerOverrideEvents.PutValueAtDate(DateTime.UtcNow, new OverrideEvent<ushort>(DateTime.UtcNow, 1));

      SaveAndVerifyNotEmpty(model);

      var request = new DeleteSiteModelRequest();
      var response = request.Execute(new DeleteSiteModelRequestArgument { ProjectID = model.ID, Selectivity = selectivity });

      response.Result.Should().Be(DeleteSiteModelResult.OK);

      if (selectivity == DeleteSiteModelSelectivity.All)
        VerifyModelIsEmpty(model);
      else
        IsModelEmpty(model, false).Should().BeFalse(); // Because the override event should not be removed

      model = DIContext.Obtain<ISiteModels>().GetSiteModel(model.ID);

      (model?.MachinesTargetValues[0]?.AutoVibrationStateEvents.Count() ?? 0).Should().Be(0);

      if (selectivity == DeleteSiteModelSelectivity.TagFileDerivedData)
        model.MachinesTargetValues[0].LayerOverrideEvents.Count().Should().Be(expectedOverrideCount);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData)]
    public void DeleteModel_WithProofingRuns(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.SiteProofingRuns.Add(new SiteProofingRun("Test Proofing Run", 0, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, new BoundingWorldExtent3D(0, 0, 1, 1)));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
      (model?.SiteProofingRuns?.Count ?? 0).Should().Be(0);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData)]
    public void DeleteModel_WithSiteModelMachineDesigns(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.SiteModelMachineDesigns.Add(new SiteModelMachineDesign(-1, "Test Name"));
      model.SiteModelMachineDesigns.Count.Should().Be(2);

      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);

      if (selectivity != DeleteSiteModelSelectivity.All) // Check only the default design is present
      {
        model.SiteModelMachineDesigns.Count.Should().Be(1);
        model.SiteModelMachineDesigns[0].Id.Should().Be(0);
      }
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData)]
    public void DeleteModel_WithSiteModelDesigns(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.SiteModelDesigns.Add(new SiteModelDesign("Test name", new BoundingWorldExtent3D(0, 0, 1, 1)));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
      (model?.SiteModelDesigns?.Count ?? 0).Should().Be(0);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.Designs)]
    public async void DeleteModel_WithSiteDesigns(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.StorageRepresentationToSupply.Should().Be(StorageMutability.Mutable);
      model.PrimaryStorageProxy.Mutability.Should().Be(StorageMutability.Mutable);
      model.PrimaryStorageProxy.ImmutableProxy.Should().NotBeNull();

      var request = new AddTTMDesignRequest();
      var _ = await request.ExecuteAsync(new AddTTMDesignArgument
      {
        ProjectID = model.ID,
        DesignDescriptor = new DesignDescriptor(Guid.NewGuid(), "", ""),
        Extents = new BoundingWorldExtent3D(0, 0, 1, 1),
        ExistenceMap = new SubGridTreeSubGridExistenceBitMask()
      });

      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
      (model?.Designs?.Count ?? 0).Should().Be(0);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.SurveyedSurfaces)]
    public void DeleteModel_WithSurveyedSurfaces(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      // TODO: Convert to request per designs
      DIContext.Obtain<ISurveyedSurfaceManager>().Add(model.ID, new DesignDescriptor(Guid.NewGuid(), "", ""), DateTime.UtcNow, new BoundingWorldExtent3D(0, 0, 1, 1), 
        new SubGridTreeSubGridExistenceBitMask());
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
      (model?.SurveyedSurfaces?.Count ?? 0).Should().Be(0);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.Alignments)]
    public void DeleteModel_WithAlignments(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      DIContext.Obtain<IAlignmentManager>().Add(model.ID, new DesignDescriptor(Guid.NewGuid(), "", ""), new BoundingWorldExtent3D(0, 0, 1, 1));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
      (model?.Alignments.Count ?? 0).Should().Be(0);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.CoordinateSystem)]
    public void DeleteModel_WithCSIB(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.SetCSIB(Encoding.ASCII.GetString(new byte[] {70, 71, 72, 73}));

      model.CSIB().Should().NotBeEmpty();
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
      model?.CSIB().Should().BeNullOrEmpty();
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData)]
    public void DeleteModel_WithSummaryMetadata(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      DIContext.Obtain<ISiteModelMetadataManager>().Add(model.ID, new SiteModelMetadata());
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);

      if (selectivity != DeleteSiteModelSelectivity.All)
      {
        model.Should().NotBeNull();
        DIContext.Obtain<ISiteModelMetadataManager>().Get(model.ID).Should().BeNull();
      }
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData)]
    public void DeleteModel_WithExistenceMap(DeleteSiteModelSelectivity selectivity)
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.ExistenceMap[0, 0] = true;
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
      (model?.ExistenceMap?.CountBits() ?? 0).Should().Be(0);
    }

    [Theory]
    [InlineData(DeleteSiteModelSelectivity.All)]
    [InlineData(DeleteSiteModelSelectivity.TagFileDerivedData)]
    public void DeleteModel_WithTagFile(DeleteSiteModelSelectivity selectivity)
    {
      var tagFiles = new[] {Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),};

      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, true, false);
      model.Should().NotBeNull();

      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, selectivity, selectivity == DeleteSiteModelSelectivity.All);
    }

    [Fact]
    public void PartialDeleteModel_WithTagFile_AllTAGFileDerivedData()
    {
      var tagFiles = new[] { Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"), };

      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, true, false);
      model.Should().NotBeNull();

      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, DeleteSiteModelSelectivity.TagFileDerivedData, false);
      model.Should().NotBeNull();
    }

    [Fact]
    public void PartialDeleteModel_WithTagFile_NonTAGFileDerivedData()
    {
      var tagFiles = new[] { Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"), };

      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, true, false);
      model.Should().NotBeNull();

      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(ref model, DeleteSiteModelSelectivity.NonTagFileDerivedData, false);
      model.Should().NotBeNull();
    }
  }
}
