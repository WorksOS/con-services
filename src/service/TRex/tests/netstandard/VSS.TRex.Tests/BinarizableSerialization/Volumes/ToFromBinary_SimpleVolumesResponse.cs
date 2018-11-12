using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Volumes
{
  public class ToFromBinary_SimpleVolumesResponse
  {
    private const string CLUSTER_NODE = "1";
    private const int NUMBER_SUBGRIDS = 12345;
    private const int NUMBER_PROD_DATA_SUBGRIDS = 1111;
    private const int NUMBER_SS_SUBGRIDS = 555;

    [Fact]
    public void Test_SimpleVolumesResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SimpleVolumesResponse>("Empty SimpleVolumesResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_CMVStatisticsResponse()
    {
      var response = new SimpleVolumesResponse()
      {
        ResponseCode = SubGridRequestsResponseResult.OK,
        ClusterNode = CLUSTER_NODE,
        NumSubgridsProcessed = NUMBER_SUBGRIDS,
        NumSubgridsExamined = NUMBER_SUBGRIDS,
        NumProdDataSubGridsProcessed = NUMBER_PROD_DATA_SUBGRIDS,
        NumProdDataSubGridsExamined = NUMBER_PROD_DATA_SUBGRIDS,
        NumSurveyedSurfaceSubGridsProcessed = NUMBER_SS_SUBGRIDS,
        NumSurveyedSurfaceSubGridsExamined = NUMBER_SS_SUBGRIDS,
        Cut = 10.0,
        Fill = 20.0,
        BoundingExtentGrid = new BoundingWorldExtent3D(1.0, 2.0, 3.0, 4.0, 5.0, 6.0),
        CutArea = 30.0,
        FillArea = 40.0,
        TotalCoverageArea = 100.0
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom SimpleVolumesResponse not same after round trip serialisation");
    }
  }
}
