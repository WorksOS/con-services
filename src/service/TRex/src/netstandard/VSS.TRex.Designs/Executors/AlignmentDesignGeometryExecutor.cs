using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Interfaces;
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
    public AlignmentDesignGeometryResponse Execute(Guid projectUid, Guid alignmentDesignUid)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);

        if (siteModel == null)
        {
          Log.LogError($"Site model {projectUid} not found");
          return null;
        }

        var design = DIContext.Obtain<IDesignFiles>()?.Lock(alignmentDesignUid, projectUid, siteModel.CellSize, out var lockResult);

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
          return null;
        }

        return new AlignmentDesignGeometryResponse
        (geometryExporter.CalcResult, 
          geometryExporter.Vertices.Vertices.Select(x => new [] { x.X, x.Y, x.Station } ).ToArray(),
          geometryExporter.Labels.ToArray());
      }
      catch (Exception e)
      {
        Log.LogError(e, "Execute: Exception: ");
        return null;
      }
    }
  }
}

