using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.CoordinateSystems;
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
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AlignmentDesignGeometryRequest))]
  public class AlignmentDesignGeometryRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    // Needed for the out parameter on the Lock function
    delegate IDesignBase GobbleDesignFilesLockReturns(Guid designUid, Guid datamodelUid, double cellSize, out DesignLoadResult result);      

    private void AddDesignProfilerGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <AlignmentDesignGeometryComputeFunc, AlignmentDesignGeometryArgument, AlignmentDesignGeometryResponse>();

    public AlignmentDesignGeometryRequestTests()
    {
      // LL to NEE
//      var convertCoordinatesMock = new Mock<IConvertCoordinates>();
//      convertCoordinatesMock.Setup(x => x.NEEToLLH(It.IsAny<string>(), It.IsAny<XYZ[]>())).Returns<string, XYZ[]>((csib, coords) =>
//      {
//        // Return the coordinates with the X and Y reversed to mimic the X <-> Lon, Y <-> Lat ordering change (NEE -> LLH) in coordinate conversions
//        return Task.FromResult((RequestErrorStatus.OK, coords.Select(x => new XYZ(x.Y, x.X, x.Z)).ToArray()));
//      });

//      DIBuilder.
//        Continue()
//        .Add(x => x.AddSingleton(convertCoordinatesMock.Object))
//        .Complete();
    }

    [Fact]
    public void Creation()
    {
      var request = new AlignmentDesignFilterBoundaryRequest();

      request.Should().NotBeNull();
    }

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
        AlignmentDesignID = alignmentGuid
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
      response.Labels[0].Should().BeEquivalentTo(new AlignmentGeometryResponseLabel(0.0, 1.0, 2.0, Math.PI / 2));
      response.Labels[1].Should().BeEquivalentTo(new AlignmentGeometryResponseLabel(1.0, 2.0, 2.0, Math.PI / 2));
    }

    [Fact]
    public async Task Geometry_FromFile()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddSVLAlignmentDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Large Sites Road - Trimble Road.svl", false);

      var request = new AlignmentDesignGeometryRequest();
      var response = await request.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
        AlignmentDesignID = designUid
      });

      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Vertices.Should().NotBeNull();
      response.Vertices.Length.Should().Be(1);

      (response.Arcs?.Length ?? 0).Should().Be(0);

      response.Labels.Should().NotBeNull();
      response.Labels.Length.Should().Be(21);
    }
  }
}
