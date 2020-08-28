using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.Interfaces;
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
        var executor = new CalculateSurfaceElevationPatch();

        var siteModel = DIContext.ObtainRequired<ISiteModels>().GetSiteModel(arg.SiteModelID);

        if (siteModel == null)
        {
          _log.LogWarning($"Failed to get site model with ID {arg.SiteModelID}");
          return new SerialisedByteArrayWrapper(null);
        }

        var result = executor.Execute(siteModel, arg.OTGCellBottomLeftX, arg.OTGCellBottomLeftY,
          arg.CellSize, arg.SurveyedSurfacePatchType, arg.IncludedSurveyedSurfaces,
          DIContext.ObtainRequired<IDesignFiles>(),
          siteModel.SurveyedSurfaces,
          arg.ProcessingMap);

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
        _log.LogError(e, "Exception requesting surveyed surface elevation patch");
        return new SerialisedByteArrayWrapper(null);
      }
    }
  }
}
