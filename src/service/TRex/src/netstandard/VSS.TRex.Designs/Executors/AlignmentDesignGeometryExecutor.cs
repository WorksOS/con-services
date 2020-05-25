using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class AlignmentDesignGeometryExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<AlignmentDesignGeometryExecutor>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <returns></returns>
    public ExportToGeometry Execute(Guid projectUid, Guid alignmentDesignUid)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);

        if (siteModel == null)
        {
          Log.LogError($"Site model {projectUid} not found");
          return null;
        }

        var lockResult = DesignLoadResult.UnknownFailure;
        var design = DIContext.Obtain<IDesignFiles>()?.Lock(alignmentDesignUid, projectUid, siteModel.CellSize, out lockResult);

        if (lockResult != DesignLoadResult.Success)
        {
          Log.LogError($"Failed to lock design with error {lockResult}");
          return null;
        }

        if (design == null)
        {
          Log.LogError($"Failed to read file for alignment {alignmentDesignUid}");
          return null;
        }

        if (!(design is SVLAlignmentDesign alignment))
        {
          Log.LogError($"Design {alignmentDesignUid} is not an alignment");
          return null;
        }

        var master = alignment.GetMasterAlignment();
        if (master == null)
        {
          Log.LogError($"Design {alignmentDesignUid} does not contain a master alignment");
          return null;
        }

        var geometryExporter = new ExportToGeometry
        {
          AlignmentLabelingInterval = 10,
          Units = DistanceUnitsType.Meters
        };

        var success = geometryExporter.ConstructSVLCenterlineAlignmentGeometry(master);

        if (!success)
        {
          Log.LogError($"Failed to generate geometry for alignment design {alignmentDesignUid}, error = {geometryExporter.CalcResult}");
        }

        return geometryExporter;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Execute: Exception: ");
        return null;
      }
    }
  }
}

