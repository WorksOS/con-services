using System;
using FluentAssertions;
using VSS.AWS.TransferProxy;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.SiteModels.Interfaces.Requests;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Requests
{
  [UnitTestCoveredRequest(RequestType = typeof(RebuildSiteModelRequest))]
  public class RebuildSiteModelRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>, IDisposable
  {
    private void AddPrimaryApplicationGridRouting() => IgniteMock.Mutable.AddApplicationGridRouting<RebuildSiteModelRequestComputeFunc, RebuildSiteModelRequestArgument, RebuildSiteModelRequestResponse>();

    private void AddSecondaryApplicationGridRouting()
    {
      IgniteMock.Mutable.AddApplicationGridRouting<DeleteSiteModelRequestComputeFunc, DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>();
      IgniteMock.Mutable.AddApplicationGridRouting<SubmitTAGFileComputeFunc, SubmitTAGFileRequestArgument, SubmitTAGFileResponse>();
      IgniteMock.Mutable.AddApplicationGridRouting<ProcessTAGFileComputeFunc, ProcessTAGFileRequestArgument, ProcessTAGFileResponse>();
    }

    private void AddApplicationGridRouting()
    {
      AddPrimaryApplicationGridRouting(); // For the rebuild request
      AddSecondaryApplicationGridRouting(); // For the delete request
    }

    public RebuildSiteModelRequestTests()
    {
      // This resets all modified content in the Ignite mocks between tests
      DITAGFileAndSubGridRequestsWithIgniteFixture.ResetDynamicMockedIgniteContent();
    }

    [Fact]
    public void Creation()
    {
      var req = new RebuildSiteModelRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public async void FailWithNonExistentSiteModel()
    {
      AddPrimaryApplicationGridRouting();

      var projectUid = Guid.NewGuid();
      var request = new RebuildSiteModelRequest();
      var arg = new RebuildSiteModelRequestArgument
      {
        DeletionSelectivity = DeleteSiteModelSelectivity.TagFileDerivedData,
        OriginS3TransferProxy = TransferProxyType.TAGFiles,
        ProjectID = projectUid
      };

      var result = await request.ExecuteAsync(arg);

      result.Should().NotBeNull();
      result.RebuildResult.Should().Be(RebuildSiteModelResult.UnableToLocateSiteModel);
    }

    [Fact]
    public async void FailWithPreexistingRebuild()
    {
      AddPrimaryApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      siteModel.SaveMetadataToPersistentStore(siteModel.PrimaryStorageProxy, true);

      // Install an active builder into the manager to cause the failure.
      DIContext.Obtain<ISiteModelRebuilderManager>().AddRebuilder(new SiteModelRebuilder(siteModel.ID, false, TransferProxyType.TAGFiles));

      var request = new RebuildSiteModelRequest();
      var arg = new RebuildSiteModelRequestArgument
      {
        DeletionSelectivity = DeleteSiteModelSelectivity.TagFileDerivedData, OriginS3TransferProxy = TransferProxyType.TAGFiles, ProjectID = siteModel.ID
      };

      var result = await request.ExecuteAsync(arg);

      result.Should().NotBeNull();
      result.RebuildResult.Should().Be(RebuildSiteModelResult.OK);
    }

    [Fact]
    public void RebuildIsReflectedInManagerState()
    {
      AddPrimaryApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      siteModel.SaveMetadataToPersistentStore(siteModel.PrimaryStorageProxy, true);

      var manager = DIContext.Obtain<ISiteModelRebuilderManager>();
      var result = manager.Rebuild(siteModel.ID, false, TransferProxyType.TAGFiles);
      result.Should().BeTrue();

      var state = manager.GetRebuildersState();
      state.Count.Should().Be(1);
    }

    public void Dispose()
    {
      DIContext.Obtain<ISiteModelRebuilderManager>().AbortAll();
    }
  }
}

