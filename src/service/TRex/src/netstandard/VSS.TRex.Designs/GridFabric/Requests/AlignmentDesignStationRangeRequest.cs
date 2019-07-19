using System.Threading.Tasks;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  public class AlignmentDesignStationRangeRequest : DesignProfilerRequest<DesignSubGridRequestArgumentBase, AlignmentDesignStationRangeResponse>
  {
    public override AlignmentDesignStationRangeResponse Execute(DesignSubGridRequestArgumentBase arg)
    {
      // Construct the function to be used
      var func = new AlignmentDesignStationRangeComputeFunc();

      return Compute.Apply(func, arg);
    }

    public override Task<AlignmentDesignStationRangeResponse> ExecuteAsync(DesignSubGridRequestArgumentBase arg)
    {
      // Construct the function to be used
      var func = new AlignmentDesignStationRangeComputeFunc();

      return Compute.ApplyAsync(func, arg);
    }

    public static AlignmentDesignStationRangeResponse Execute(DesignOffset referenceDesign)
    {
      var request = new AlignmentDesignStationRangeRequest();

      return request.Execute(new DesignSubGridRequestArgumentBase
      {
        ReferenceDesign = referenceDesign
      });
    }

    public static Task<AlignmentDesignStationRangeResponse> ExecuteAsync(DesignOffset referenceDesign)
    {
      var request = new AlignmentDesignStationRangeRequest();

      return request.ExecuteAsync(new DesignSubGridRequestArgumentBase
      {
        ReferenceDesign = referenceDesign
      });
    }
  }
}
