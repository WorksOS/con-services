using System;
using System.IO;
using System.Reflection;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Exports.Surfaces.Executors;
using VSS.TRex.Exports.Surfaces.GridDecimator;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Exports.Surfaces.GridFabric
{
  /// <summary>
  /// The grid compute function responsible for coordinating subgrids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  [Serializable]
  public class TINSurfaceRequestComputeFunc : BaseComputeFunc, IComputeFunc<TINSurfaceRequestArgument, TINSurfaceResult>
  {
    [NonSerialized] private static readonly ILogger
      Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Default no-arg constructor that orients the request to the available ASNODE servers on the immutable grid projection
    /// </summary>
    public TINSurfaceRequestComputeFunc() : base(TRexGrids.ImmutableGridName(), ServerRoles.TIN_SURFACE_EXPORT_ROLE)
    {
    }

    public TINSurfaceResult Invoke(TINSurfaceRequestArgument arg)
    {
      Log.LogInformation("In TINSurfaceRequestComputeFunc.Invoke()");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // subgrid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);

        Log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        TINSurfaceExportExecutor request = new TINSurfaceExportExecutor(arg.ProjectID, arg.Filters, arg.Tolerance, arg.TRexNodeID);

        Log.LogInformation("Executing request.Execute()");

        if (!request.Execute())
          Log.LogError($"Request execution failed");

        TINSurfaceResult result = new TINSurfaceResult();
        using (MemoryStream ms = new MemoryStream())
        {
          if (request.SurfaceSubGridsResponse.TIN != null)
          {
            request.SurfaceSubGridsResponse.TIN.SaveToStream(Consts.DefaultCoordinateResolution, Consts.DefaultElevationResolution, false, ms);
            result.data = ms.ToArray();
          }
        }

        return result;
      }
      finally
      {
        Log.LogInformation("Exiting TINSurfaceRequestComputeFunc.Invoke()");
      }
    }
  }
}
