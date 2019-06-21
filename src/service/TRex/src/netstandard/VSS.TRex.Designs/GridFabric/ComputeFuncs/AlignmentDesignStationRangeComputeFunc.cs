using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  public class AlignmentDesignStationRangeComputeFunc : BaseComputeFunc, IComputeFunc<DesignSubGridRequestArgumentBase, AlignmentDesignStationRangeResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<AlignmentDesignStationRangeComputeFunc>();

    public AlignmentDesignStationRangeResponse Invoke(DesignSubGridRequestArgumentBase args)
    {
      return null;
    }
  }
}
