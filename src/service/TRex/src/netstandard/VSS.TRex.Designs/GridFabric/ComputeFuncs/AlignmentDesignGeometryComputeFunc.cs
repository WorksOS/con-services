using System;
using Apache.Ignite.Core.Compute;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  public class AlignmentDesignGeometryComputeFunc : BaseComputeFunc, IComputeFunc<AlignmentDesignGeometryArgument, AlignmentDesignGeometryResponse>
  {
    public AlignmentDesignGeometryResponse Invoke(AlignmentDesignGeometryArgument arg)
    {
      throw new NotImplementedException();
    }
  }
}
