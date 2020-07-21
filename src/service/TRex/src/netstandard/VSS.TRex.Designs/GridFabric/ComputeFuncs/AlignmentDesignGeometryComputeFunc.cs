using System;
using System.Linq;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  public class AlignmentDesignGeometryComputeFunc : BaseComputeFunc, IComputeFunc<AlignmentDesignGeometryArgument, AlignmentDesignGeometryResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AlignmentDesignGeometryComputeFunc>();

    public AlignmentDesignGeometryResponse Invoke(AlignmentDesignGeometryArgument arg)
    {
      try
      {
        var executor = new AlignmentDesignGeometryExecutor();
        var geometry = executor.Execute(arg.ProjectID, arg.AlignmentDesignID, arg.ConvertArcsToPolyLines, arg.ArcChordTolerance);

        if (geometry != null)
        {
          return new AlignmentDesignGeometryResponse
            (geometry.CalcResult,
             geometry.Vertices.Select(v => v.Vertices.Select(x => new[] {x.X, x.Y, x.Station}).ToArray()).ToArray(),
             geometry.Arcs.ToArray(),
             geometry.Labels.ToArray());
        }
        
        return new AlignmentDesignGeometryResponse(DesignProfilerRequestResult.UnknownError, null, null, null);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception obtaining design geometry boundary");
        return new AlignmentDesignGeometryResponse { RequestResult = DesignProfilerRequestResult.UnknownError };
      }
    }
  }
}
