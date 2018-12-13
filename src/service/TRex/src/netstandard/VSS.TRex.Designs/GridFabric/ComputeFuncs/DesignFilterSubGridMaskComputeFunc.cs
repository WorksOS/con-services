using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  public class DesignFilterSubGridMaskComputeFunc : BaseComputeFunc, IComputeFunc<DesignSubGridFilterMaskArgument, DesignFilterSubGridMaskResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationPatchComputeFunc>();

    public DesignFilterSubGridMaskResponse Invoke(DesignSubGridFilterMaskArgument args)
    {
      try
      {
        // Calculate an elevation patch for the requested location and convert it into a bitmask detailing which cells have non-null values

        CalculateDesignElevationPatch Executor = new CalculateDesignElevationPatch();

        var patch = Executor.Execute(args.ProjectID, args.ReferenceDesignUID, args.CellSize, args.OriginX, args.OriginY, 0);

        if (patch == null)
          return null; // This may seem harsh, but callers should not ask for patches that do not exist

        var result = new DesignFilterSubGridMaskResponse();

        for (byte i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          for (byte j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
             result.Bits[i, j] = patch.Cells[i, j].Equals(Common.Consts.NullHeight);

        return result;
      }
      catch (Exception E)
      {
        Log.LogError("Exception: ", E);
        return null;
      }
    }
  }
}
