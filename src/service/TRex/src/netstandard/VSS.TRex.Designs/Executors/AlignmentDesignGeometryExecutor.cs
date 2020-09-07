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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AlignmentDesignGeometryExecutor>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    public ExportToGeometry Execute(Guid projectUid, Guid alignmentDesignUid, bool convertArcsToPolyLines, double arcChordTolerance)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);

        if (siteModel == null)
        {
          _log.LogError($"Site model {projectUid} not found");
          return null;
        }

        if (convertArcsToPolyLines && arcChordTolerance < 0.001)
        {
          _log.LogError("Arc chord tolerance too small, must be >= 0.001 meters");
          return null;
        }

        var lockResult = DesignLoadResult.UnknownFailure;
        var design = DIContext.ObtainRequired<IDesignFiles>().Lock(alignmentDesignUid, siteModel, siteModel.CellSize, out lockResult);

        if (lockResult != DesignLoadResult.Success)
        {
          _log.LogError($"Failed to lock design with error {lockResult}");
          return null;
        }

        if (design == null)
        {
          _log.LogError($"Failed to read file for alignment {alignmentDesignUid}");
          return null;
        }

        if (!(design is SVLAlignmentDesign alignment))
        {
          _log.LogError($"Design {alignmentDesignUid} is not an alignment");
          return null;
        }

        var master = alignment.GetMasterAlignment();
        if (master == null)
        {
          _log.LogError($"Design {alignmentDesignUid} does not contain a master alignment");
          return null;
        }

        var geometryExporter = new ExportToGeometry
        {
          AlignmentLabelingInterval = 10,
          Units = DistanceUnitsType.Meters,
          ConvertArcsToPolyLines = convertArcsToPolyLines,
          ArcChordTolerance = arcChordTolerance
        };

        var success = geometryExporter.ConstructSVLCenterlineAlignmentGeometry(master);

        if (!success)
        {
          _log.LogError($"Failed to generate geometry for alignment design {alignmentDesignUid}, error = {geometryExporter.CalcResult}");
        }

        return geometryExporter;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Execute: Exception: ");
        return null;
      }
    }
  }
}

