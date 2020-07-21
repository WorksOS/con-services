using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces.Executors;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs
{
  public class SurfaceElevationPatchComputeFunc : BaseComputeFunc, IComputeFunc<ISurfaceElevationPatchArgument, ISerialisedByteArrayWrapper>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SurfaceElevationPatchComputeFunc>();

    /// <summary>
    /// Invokes the surface elevation patch computation function on the server nodes the request has been sent to
    /// </summary>
    public ISerialisedByteArrayWrapper Invoke(ISurfaceElevationPatchArgument arg)
    {
      try
      {
        _log.LogDebug($"CalculateDesignElevationPatchComputeFunc: Arg = {arg}");

        var executor = new CalculateSurfaceElevationPatch(arg);
        var result = executor.Execute();

        byte[] resultAsBytes = null;
        if (result != null)
        {
          try
          {
            resultAsBytes = result.ToBytes();
          }
          finally
          {
            DIContext.Obtain<IClientLeafSubGridFactory>().ReturnClientSubGrid(ref result);
          }
        }

        return new SerialisedByteArrayWrapper(resultAsBytes);
      }
      catch (Exception e)
      {
        _log.LogError(e, $"{nameof(SurfaceElevationPatchComputeFunc)}.Invoke: Exception:");
        return null;
      }
    }
  }
}
