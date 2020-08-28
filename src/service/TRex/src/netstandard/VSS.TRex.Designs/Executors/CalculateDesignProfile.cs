using System.Collections.Generic;
using System.Linq;
using CoreX.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateDesignProfile
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignProfile>();

    private static IDesignFiles designs;

    private IDesignFiles Designs => designs ??= DIContext.ObtainRequired<IDesignFiles>();

    /// <summary>
    /// Default no-args constructor
    /// </summary>
    public CalculateDesignProfile()
    {
    }

    /// <summary>
    /// Performs the donkey work of the profile calculation
    /// </summary>
    private List<XYZS> Calc(CalculateDesignProfileArgument arg, out DesignProfilerRequestResult calcResult)
    {
      calcResult = DesignProfilerRequestResult.UnknownError;

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);

      var Design = Designs.Lock(arg.ReferenceDesign.DesignID, siteModel, arg.CellSize, out DesignLoadResult LockResult);

      var arg2 = new CalculateDesignProfileArgument_ClusterCompute
      {
        ProjectID = arg.ProjectID,
        CellSize = arg.CellSize,
        ReferenceDesign = arg.ReferenceDesign,
      };

      if (arg.PositionsAreGrid)
      {
        arg2.ProfilePathNEE = new[] { arg.StartPoint, arg.EndPoint }.Select(x => new XYZ(x.Lon, x.Lat)).ToArray();
      }
      else
      {
        if (siteModel != null)
        {
          arg2.ProfilePathNEE = DIContext.Obtain<IConvertCoordinates>().WGS84ToCalibration(
            siteModel.CSIB(),
            new[] { arg.StartPoint, arg.EndPoint }
            .ToCoreX_WGS84Point(),
            CoreX.Types.InputAs.Radians)
            .ToTRex_XYZ();
        }
      }



      if (Design == null)
      {
        Log.LogWarning($"Failed to read file for design {arg.ReferenceDesign.DesignID} lock result was {LockResult}");
        calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
        return null;
      }

      try
      {

        var result = Design.ComputeProfile(arg2.ProfilePathNEE, arg.CellSize);
        //Apply any offset to the profile
        if (arg.ReferenceDesign.Offset != 0)
        {
          for (var i=0; i<result.Count; i++)
          {
            result[i] = new XYZS(result[i].X, result[i].Y, result[i].Z + arg.ReferenceDesign.Offset, result[i].Station, result[i].TriIndex);
          }
        }
        calcResult = DesignProfilerRequestResult.OK;

        return result;
      }
      finally
      {
        Designs.UnLock(arg.ReferenceDesign.DesignID, Design);
      }
    }

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    public List<XYZS> Execute(CalculateDesignProfileArgument args, out DesignProfilerRequestResult calcResult)
    {
      // Perform the design profile calculation
      var result = Calc(args, out calcResult);

      if (result == null)
      {
        Log.LogInformation($"Unable to calculate a design profiler result for {args}");
        result = new List<XYZS>();
      }

      return result;
    }
  }
}
