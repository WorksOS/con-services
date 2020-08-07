using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.Designs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AlignmentDesignGeometryRequest))]
  public class AlignmentDesignGeometryRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    // Needed for the out parameter on the Lock function
    delegate IDesignBase GobbleDesignFilesLockReturns(Guid designUid, Guid datamodelUid, double cellSize, out DesignLoadResult result);      

    public AlignmentDesignGeometryRequestTests(DITAGFileAndSubGridRequestsWithIgniteFixture fixture)
    {
      fixture.ClearDynamicFixtureContent();
      fixture.SetupFixture();

      // Fresh the DesignFiles DI for each test as some tests mock it specially
      DIBuilder.
        Continue()
        .Add(x => x.AddSingleton<IDesignFiles>(new DesignFiles()))
        .Complete();
    }

    private void AddDesignProfilerGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <AlignmentDesignGeometryComputeFunc, AlignmentDesignGeometryArgument, AlignmentDesignGeometryResponse>();

    [Fact]
    public void Creation()
    {
      var request = new AlignmentDesignGeometryRequest();

      request.Should().NotBeNull();
    }

    /// <summary>
    /// Constructs a SVL alignment with a single poly line element with two vertices
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Geometry_SimpleLine()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var polyline = new NFFLineworkPolyLineEntity();
      polyline.Vertices.Add(new NFFLineworkPolyLineVertexEntity(polyline, 1, 2, 3, 0));
      polyline.Vertices.Add(new NFFLineworkPolyLineVertexEntity(polyline, 2, 2, 4, 1));

      var alignment = new NFFGuidableAlignmentEntity();
      alignment.Entities.Add(polyline);

      var alignmentGuid = Guid.NewGuid();
      var testDesign = new SVLAlignmentDesign(alignment);

      siteModel.Alignments.AddAlignmentDetails(alignmentGuid, new DesignDescriptor(alignmentGuid, "", ""), BoundingWorldExtent3D.Full());

      var mockDesignFiles = new Mock<IDesignFiles>();
      mockDesignFiles.Setup(x => x.Lock(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<double>(), out It.Ref<DesignLoadResult>.IsAny))
        .Returns(new GobbleDesignFilesLockReturns((Guid designUid, Guid datamodelUid, double cellSize, out DesignLoadResult result) =>
        {
          result = DesignLoadResult.Success;
          return testDesign;
        }));

      DIBuilder.
        Continue()
        .Add(x => x.AddSingleton(mockDesignFiles.Object))
        .Complete();

      var request = new AlignmentDesignGeometryRequest();
      var response = await request.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
        AlignmentDesignID = alignmentGuid,
        ConvertArcsToPolyLines = false,
        ArcChordTolerance = 0.0
      });

      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Vertices.Should().NotBeNull();
      response.Vertices.Length.Should().Be(1);
      response.Vertices[0].Length.Should().Be(2);
      response.Vertices[0][0].Length.Should().Be(3);
      response.Vertices[0][1].Length.Should().Be(3);

      response.Vertices[0][0].Should().BeEquivalentTo(new double[] { 1, 2, 0 });
      response.Vertices[0][1].Should().BeEquivalentTo(new double[] { 2, 2, 1 });

      response.Arcs.Should().BeNullOrEmpty();

      response.Labels.Length.Should().Be(2);

      response.Labels[0].Station.Should().BeApproximately(0.0, 0.001);
      response.Labels[0].X.Should().BeApproximately(1.0, 0.001);
      response.Labels[0].Y.Should().BeApproximately(2.0, 0.001);
      response.Labels[0].Rotation.Should().BeApproximately(Math.PI / 2, 0.001);

      response.Labels[1].Station.Should().BeApproximately(1.0, 0.001);
      response.Labels[1].X.Should().BeApproximately(2.0, 0.001);
      response.Labels[1].Y.Should().BeApproximately(2.0, 0.001);
      response.Labels[1].Rotation.Should().BeApproximately(Math.PI / 2, 0.001);
    }

    private (ISiteModel siteModelId, Guid alignmentId) ConstructSimpleArcNFFFileModel()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var arc = new NFFLineworkArcEntity(0, 0.0, 0.0, 0.0, 1.0, 1.0, 0.0, 1.0, 0.0, 0.0, true, false)
      {
        StartStation = 0
      };

      var alignment = new NFFGuidableAlignmentEntity();
      alignment.Entities.Add(arc);

      var alignmentGuid = Guid.NewGuid();
      var testDesign = new SVLAlignmentDesign(alignment);

      siteModel.Alignments.AddAlignmentDetails(alignmentGuid, new DesignDescriptor(alignmentGuid, "", ""), BoundingWorldExtent3D.Full());

      var mockDesignFiles = new Mock<IDesignFiles>();
      mockDesignFiles.Setup(x => x.Lock(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<double>(), out It.Ref<DesignLoadResult>.IsAny))
        .Returns(new GobbleDesignFilesLockReturns((Guid designUid, Guid datamodelUid, double cellSize, out DesignLoadResult result) =>
        {
          result = DesignLoadResult.Success;
          return testDesign;
        }));

      DIBuilder.
        Continue()
        .Add(x => x.AddSingleton(mockDesignFiles.Object))
        .Complete();

      return (siteModel, alignmentGuid);
    }

    /// <summary>
    /// Constructs a SVL alignment with a single poly line element with two vertices
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Geometry_SimpleArc()
    {
      AddDesignProfilerGridRouting();

      var (siteModel, alignmentGuid) = ConstructSimpleArcNFFFileModel();

      var request = new AlignmentDesignGeometryRequest();
      var response = await request.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
        AlignmentDesignID = alignmentGuid,
        ConvertArcsToPolyLines = false,
        ArcChordTolerance = 0.0
      });

      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Vertices.Should().BeNullOrEmpty();

      response.Arcs.Should().NotBeNullOrEmpty();
      response.Arcs.Length.Should().Be(1);

      response.Arcs[0].Should().BeEquivalentTo(new AlignmentGeometryResponseArc(0.0, 0.0, 0.0, 1.0, 1.0, 0.0, 1.0, 0.0, 0.0, true));

      response.Labels.Length.Should().Be(2);

      response.Labels[0].Station.Should().BeApproximately(0, 0.001);
      response.Labels[0].X.Should().BeApproximately(0, 0.001);
      response.Labels[0].Y.Should().BeApproximately(0, 0.001);
      response.Labels[0].Rotation.Should().BeApproximately(Math.PI, 0.000001);

      response.Labels[1].Station.Should().BeApproximately(Math.PI / 2, 0.001);
      response.Labels[1].X.Should().BeApproximately(1.0, 0.001);
      response.Labels[1].Y.Should().BeApproximately(1.0, 0.001);

      response.Labels[1].Rotation.Should().BeApproximately(Math.PI / 2, 0.000001);
    }

    /// <summary>
    /// Constructs a SVL alignment with a single poly line element with two vertices
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Geometry_SimpleArcWithChords()
    {
      AddDesignProfilerGridRouting();

      var (siteModel, alignmentGuid) = ConstructSimpleArcNFFFileModel();

      var request = new AlignmentDesignGeometryRequest();
      var response = await request.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
        AlignmentDesignID = alignmentGuid,
        ConvertArcsToPolyLines = true,
        ArcChordTolerance = 0.01
      });

      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Vertices.Should().NotBeNullOrEmpty();
      response.Vertices.Length.Should().Be(1);
      response.Vertices[0].Length.Should().Be(7);

      response.Labels.Length.Should().Be(2);

      response.Labels[0].Station.Should().BeApproximately(0, 0.001);
      response.Labels[0].X.Should().BeApproximately(0, 0.001);
      response.Labels[0].Y.Should().BeApproximately(0, 0.001);
      response.Labels[0].Rotation.Should().BeApproximately(Math.PI, 0.000001);

      response.Labels[1].Station.Should().BeApproximately(Math.PI / 2, 0.001);
      response.Labels[1].X.Should().BeApproximately(1.0, 0.001);
      response.Labels[1].Y.Should().BeApproximately(1.0, 0.001);

      response.Labels[1].Rotation.Should().BeApproximately(Math.PI / 2, 0.000001);
    }

    [Fact]
    public async Task Geometry_FromFile_NoArcConversion()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddSVLAlignmentDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Large Sites Road - Trimble Road.svl");

      var request = new AlignmentDesignGeometryRequest();
      var response = await request.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
        AlignmentDesignID = designUid,
        ConvertArcsToPolyLines = false,
        ArcChordTolerance = 0.0
      });

      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Vertices.Should().NotBeNull();
      response.Vertices.Length.Should().Be(2);

      response.Arcs.Should().NotBeNull();
      response.Arcs.Length.Should().Be(2);

      response.Labels.Should().NotBeNull();
      response.Labels.Length.Should().Be(21);
    }

    [Fact]
    public async Task Geometry_FromFile_WithArcConversion()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddSVLAlignmentDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Large Sites Road - Trimble Road.svl");

      var request = new AlignmentDesignGeometryRequest();
      var response = await request.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
        AlignmentDesignID = designUid,
        ConvertArcsToPolyLines = true,
        ArcChordTolerance = 0.1
      });

      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Vertices.Should().NotBeNull();
      response.Vertices.Length.Should().Be(1);

      response.Arcs.Should().NotBeNull();
      response.Arcs.Length.Should().Be(0);

      response.Labels.Should().NotBeNull();
      response.Labels.Length.Should().Be(21);
    }

  }
}
