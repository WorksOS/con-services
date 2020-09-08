using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Events;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  public class DesignChangedEventListenerTestsWithFullDIContent : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationRouting() => IgniteMock.Mutable.AddApplicationGridRouting<AddTTMDesignComputeFunc, AddTTMDesignArgument, AddTTMDesignResponse>();
    private const string TESTFILENAME = "Bug36372.ttm";

    [Fact]
    public void Invoke_WithSiteModels()
    {
      var message = new DesignChangedEvent();
      var listener = new DesignChangedEventListener(TRexGrids.ImmutableGridName());
      listener.Invoke(Guid.Empty, message).Should().BeTrue();
    }

    [Fact]
    public void StartListening()
    {
      var listener = new DesignChangedEventListener(TRexGrids.ImmutableGridName())
      {
        MessageTopicName = "TestMessageTopic"
      };
      listener.StartListening();
    }

    [Fact]
    public void StopListening()
    {
      var listener = new DesignChangedEventListener(TRexGrids.ImmutableGridName());
      listener.StopListening();
    }

    [Fact]
    public void TestRemoveDesignFromDesignCache()
    {
      AddApplicationRouting();
 
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);
      var localStorage = Path.Combine(FilePathHelper.GetTempFolderForProject(siteModel.ID), TESTFILENAME);

      var designFiles = DIContext.ObtainOptional<IDesignFiles>();
      designFiles.NumDesignsInCache().Should().Be(0);
      designFiles.Lock(designUid, siteModel, SubGridTreeConsts.DefaultCellSize, out var loadResult);

      File.Exists(localStorage).Should().BeTrue();

      designFiles.NumDesignsInCache().Should().Be(1);
      var design1 = loadResult.Should().Be(DesignLoadResult.Success);
      design1.Should().NotBeNull();

      var message = new DesignChangedEvent()
      {
        SiteModelUid = siteModel.ID,
        DesignUid = designUid,
        DesignRemoved = true,
        FileType = ImportedFileType.DesignSurface
      };

      var listener = new DesignChangedEventListener(TRexGrids.ImmutableGridName());
      listener.Invoke(Guid.Empty, message).Should().BeTrue();
      File.Exists(localStorage).Should().BeFalse();

      designFiles.NumDesignsInCache().Should().Be(0); 

    }

  }
}
