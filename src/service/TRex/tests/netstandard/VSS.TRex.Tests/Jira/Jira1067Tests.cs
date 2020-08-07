using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.Jira
{
  public class Jira1067Tests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private static readonly ILogger _log = VSS.TRex.Logging.Logger.CreateLogger<Jira1067Tests>();

    private void AddApplicationGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting<SimpleVolumesRequestComputeFunc_ApplicationService, SimpleVolumesRequestArgument, SimpleVolumesResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting<SimpleVolumesRequestComputeFunc_ClusterCompute, SimpleVolumesRequestArgument, SimpleVolumesResponse>();
    }

    private void AddDesignProfilerGridRouting()
    {
      IgniteMock.Immutable.AddApplicationGridRouting
        <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();
    }

    private SimpleVolumesRequestArgument SimpleDefaultRequestArg(Guid ProjectUid)
    {
      return new SimpleVolumesRequestArgument
      {
        ProjectID = ProjectUid,
        VolumeType = VolumeComputationType.Between2Filters,
        BaseFilter = new CombinedFilter
        {
          AttributeFilter =
          {
            ReturnEarliestFilteredCellPass = true,
          }
        },
        TopFilter = new CombinedFilter(),
        BaseDesign = new DesignOffset(),
        TopDesign = new DesignOffset(),
        CutTolerance = 0.001,
        FillTolerance = 0.001
      };
    }

    private void CheckDefaultFilterToFilterSingleTAGFileResponse(SimpleVolumesResponse response)
    {
      //Was, response = {Cut:1.00113831634521, Fill:2.48526947021484, Cut Area:117.5652, FillArea: 202.9936, Total Area:353.0424, BoundingGrid:MinX: 537669.2, MaxX:537676.34, MinY:5427391.44, MaxY:5427514.52, MinZ: 1E+308, MaxZ:1E+308, BoundingLLH:MinX: 1E+308, MaxX:1E+308, MinY:1...
      const double EPSILON = 0.000001;
      response.Should().NotBeNull();
      response.Cut.Should().BeApproximately(574.1289522460937, EPSILON);
      response.Fill.Should().BeApproximately(1017.6427069946285, EPSILON);
      response.CutArea.Should().BeApproximately(10256.378800000002, EPSILON);
      response.FillArea.Should().BeApproximately(12345.848800000002, EPSILON);
      response.TotalCoverageArea.Should().BeApproximately(35238.348000000005, EPSILON);
    }

    [Trait("Dev Only", "")]
    [Fact(Skip ="Dev Only")]
    public async Task FilterToFilter_TwoFilters()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = Directory.GetDirectories(@"C:\Temp\Tonsasenfiles").Where(x => x.Contains("2005")).SelectMany(Directory.GetFiles).Take(500).ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(SimpleDefaultRequestArg(siteModel.ID));

      _log.LogInformation($"Volume result = Cut:{response.Cut} CutArea:{response.CutArea} Fill:{response.Fill} FillArea:{response.FillArea}, TotalArea:{response.TotalCoverageArea}");

      //Volume result = Cut:574.1289522460937 CutArea:10256.378800000002 Fill:1017.6427069946285 FillArea:12345.848800000002, TotalArea:35238.348000000005



      CheckDefaultFilterToFilterSingleTAGFileResponse(response);
    }

    [Trait("Dev Only", "")]
    [Fact(Skip ="Dev Only")]
    public async Task FilterToFilter_TwoFilters_WithIntermediary()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = Directory.GetDirectories(@"C:\Temp\Tonsasenfiles").Where(x => x.Contains("2005")).SelectMany(Directory.GetFiles).Take(500).ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ApplicationService();

      var arg = new SimpleVolumesRequestArgument
      {
        ProjectID = siteModel.ID,
        VolumeType = VolumeComputationType.Between2Filters,
        BaseFilter = new CombinedFilter
        {
          AttributeFilter =
          {
            ReturnEarliestFilteredCellPass = false,
            HasTimeFilter = true,
            StartTime = Consts.MIN_DATETIME_AS_UTC,
            EndTime = DateTime.SpecifyKind(new DateTime(2020, 5, 3, 0, 0, 0), DateTimeKind.Utc)
          }
        },
        TopFilter = new CombinedFilter
        {
          AttributeFilter =
          {
            ReturnEarliestFilteredCellPass = false,
            HasTimeFilter = true,
            StartTime = DateTime.SpecifyKind(new DateTime(2020, 5, 3, 0, 0, 0), DateTimeKind.Utc),
            EndTime = DateTime.SpecifyKind(new DateTime(2020, 5, 9, 23, 59, 59), DateTimeKind.Utc)
          }
        },
        BaseDesign = new DesignOffset(),
        TopDesign = new DesignOffset(),
        CutTolerance = 0.001,
        FillTolerance = 0.001
      };

      var response = await request.ExecuteAsync(arg);

      _log.LogInformation($"Volume result = Cut:{response.Cut} CutArea:{response.CutArea} Fill:{response.Fill} FillArea:{response.FillArea}, TotalArea:{response.TotalCoverageArea}");

      // Volume result = Cut:574.2604204345703 CutArea: 10261.9276 Fill: 1016.5434415893552 FillArea: 12337.410000000002, TotalArea: 35233.3772

      //CheckDefaultFilterToFilterSingleTAGFileResponse(response);
    }


    [Trait("Dev Only", "")]
    [Fact(Skip ="Dev Only")]
    public async Task FilterToFilter_TwoFilters_WithIntermediary_SingleCellFromDataWithTwoPasses()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      const double CENTERX = 534043.612;
      const double CENTERY = 6746939.126;
      const double DELTA = 0.2d;

      var tagFiles = Directory.GetDirectories(@"C:\Temp\Tonsasenfiles").Where(x => x.Contains("2005")).SelectMany(Directory.GetFiles).Take(500).ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ApplicationService();

      var arg = new SimpleVolumesRequestArgument
      {
        ProjectID = siteModel.ID,
        VolumeType = VolumeComputationType.Between2Filters,
        BaseFilter = new CombinedFilter
        {
          AttributeFilter =
          {
            ReturnEarliestFilteredCellPass = false,
            HasTimeFilter = true,
            StartTime = Consts.MIN_DATETIME_AS_UTC,
            EndTime = DateTime.SpecifyKind(new DateTime(2020, 5, 3, 0, 0, 0), DateTimeKind.Utc)
          },
          SpatialFilter =
          {
            CoordsAreGrid = true,
            IsSpatial = true,
            Fence = new TRex.Geometry.Fence(CENTERX - DELTA, CENTERY - DELTA, CENTERX + DELTA, CENTERY + DELTA)
          }
        },
        TopFilter = new CombinedFilter
        {
          AttributeFilter =
          {
            ReturnEarliestFilteredCellPass = false,
            HasTimeFilter = true,
            StartTime = DateTime.SpecifyKind(new DateTime(2020, 5, 3, 0, 0, 0), DateTimeKind.Utc),
            EndTime = DateTime.SpecifyKind(new DateTime(2020, 5, 9, 23, 59, 59), DateTimeKind.Utc)
          },
          SpatialFilter =
          {
            CoordsAreGrid = true,
            IsSpatial = true,
            Fence = new TRex.Geometry.Fence(CENTERX - DELTA, CENTERY - DELTA, CENTERX + DELTA, CENTERY + DELTA)
          }
        },
        BaseDesign = new DesignOffset(),
        TopDesign = new DesignOffset(),
        CutTolerance = 0.001,
        FillTolerance = 0.001,
        
      };

      var response = await request.ExecuteAsync(arg);

      _log.LogInformation($"Volume result = Cut:{response.Cut} CutArea:{response.CutArea} Fill:{response.Fill} FillArea:{response.FillArea}, TotalArea:{response.TotalCoverageArea}");

      // Volume result = Cut:574.2604204345703 CutArea: 10261.9276 Fill: 1016.5434415893552 FillArea: 12337.410000000002, TotalArea: 35233.3772

      //CheckDefaultFilterToFilterSingleTAGFileResponse(response);
    }
  }
}
