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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<CalculateDesignElevationPatchComputeFunc>();

    public DesignFilterSubGridMaskResponse Invoke(DesignSubGridFilterMaskArgument args)
    {
      try
      {
        // Calculate an elevation patch for the requested location and convert it into a bitmask detailing which cells have non-null values

        var executor = new CalculateDesignElevationPatch();

        var patch = executor.Execute(args.ProjectID, args.ReferenceDesign, args.CellSize, args.OriginX, args.OriginY, out var calcResult);

        var result = new DesignFilterSubGridMaskResponse();

        if (patch == null)
        {
          _log.LogWarning($"Request for design elevation patch that does not exist: Project: {args.ProjectID}, design {args.ReferenceDesign}, location {args.OriginX}:{args.OriginY}, calcResult: {calcResult}");

          result.Bits = null; // Requestors should not ask for sub grids that don't exist in the design.
          result.RequestResult = calcResult;
          return result;
        }

        result.RequestResult = calcResult;

        var patchCells = patch.Cells;
        for (byte i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
        {
          for (byte j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
          {
            result.Bits[i, j] = !patchCells[i, j].Equals(Common.Consts.NullHeight);
          }
        }
        return result;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception calculating design filter sub grid mask");
        return new DesignFilterSubGridMaskResponse { Bits = null, RequestResult = Models.DesignProfilerRequestResult.UnknownError };
      }
    }
  }
}
