using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Alignments;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Requests
{
  [UnitTestCoveredRequest(RequestType = typeof(DeleteSiteModelRequest))]
  public class DeleteSiteModelRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<DeleteSiteModelRequestComputeFunc, DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>();

    private bool IsModelEmpty(ISiteModel model)
    {
      // Check that there are no elements in the storage proxy for the site model
      foreach (var cache in IgniteMock.MockedCacheDictionaries)
      {
        if (cache.Keys.Count > 0)
          return false;
      }

      return true;
    }

    private void VerifyModelIsEmpty(ISiteModel model)
    {
      IsModelEmpty(model).Should().BeTrue();
    }

    private void DeleteTheModel(ISiteModel model)
    {
      var modelId = model.ID;

      var request = new DeleteSiteModelRequest();
      var response = request.Execute(new DeleteSiteModelRequestArgument { ProjectID = modelId });

      response.Result.Should().Be(DeleteSiteModelResult.OK);

      VerifyModelIsEmpty(model);
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

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.SaveMetadataToPersistentStore(model.PrimaryStorageProxy);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteEmptyModel_TAGFileIngestPersistence()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);
      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithProofingRuns()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.SiteProofingRuns.Add(new SiteProofingRun("Test Proofing Run", 0, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, new BoundingWorldExtent3D(0, 0, 1, 1)));
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);

      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithSiteModelMachineDesigns()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.SiteModelMachineDesigns.Add(new SiteModelMachineDesign(-1, "Test Name"));
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);

      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithSiteModelDesigns()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.SiteModelDesigns.Add(new SiteModelDesign("Test name", new BoundingWorldExtent3D(0, 0, 1, 1))); 
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);

      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithSiteDesigns()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.Designs.Add(new Design(Guid.NewGuid(), new DesignDescriptor(Guid.NewGuid(), "", ""), new BoundingWorldExtent3D(0, 0, 1, 1)));
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);

      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithSurveyedSurfaces()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.SurveyedSurfaces.Add(new SurveyedSurface(Guid.NewGuid(), new DesignDescriptor(Guid.NewGuid(), "", ""), DateTime.UtcNow, new BoundingWorldExtent3D(0, 0, 1, 1)));
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);

      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithAlignments()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.Alignments.Add(new Alignment(Guid.NewGuid(), new DesignDescriptor(Guid.NewGuid(), "", ""), new BoundingWorldExtent3D(0, 0, 1, 1)));
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);

      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithCSIB()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      var csibStream = new MemoryStream();
      csibStream.Write(new byte[] { 70, 71, 72, 73 }, 0, 4);
      csibStream.Position = 0;

      model.PrimaryStorageProxy.WriteStreamToPersistentStore(model.ID,
        CoordinateSystemConsts.kCoordinateSystemCSIBStorageKeyName,
        FileSystemStreamType.CoordinateSystemCSIB,
        csibStream, null);

      model.CSIB().Should().NotBeEmpty();
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);

      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithExistenceMap()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.ExistenceMap[0, 0] = true;
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);

      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModelWithTagFile()
    {
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var model = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      model.Should().NotBeNull();

      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy);

      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }
  }
}

