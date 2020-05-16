using System;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Files.DXF
{
  public static class DXFFileUtilities
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger("DXFFileUtilities");

    public static DXFUtilitiesResult RequestBoundariesFromLineWork(StreamReader dxfFileStream, string baseName, DxfUnitsType units, uint maxBoundariesToProcess, out PolyLineBoundaries boundaries)
    {
      boundaries = null;
      var reader = new DXFReader(dxfFileStream, units);

      try
      {
        if (!reader.FindEntitiesSection())
        {
          return DXFUtilitiesResult.NoEntitiesSectionFound;
        }

        var extractor = new PolyLineBoundaryExtractor(reader);
        var result = extractor.Extract(baseName, units, maxBoundariesToProcess);

        if (result == DXFUtilitiesResult.Ok)
          boundaries = extractor.Boundaries;

        return result;
      }
      catch (FormatException e)
      {
        Log.LogError(e, "DXF file not in expected format");
        return DXFUtilitiesResult.UnknownFileFormat;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception processing DXF file");
        throw;
      }
    }

    public static DXFUtilitiesResult RequestBoundariesFromLineWork(string dxfFileName, DxfUnitsType units, uint maxBoundariesToProcess, out PolyLineBoundaries boundaries)
    {
      using var fileStream = new FileStream(dxfFileName, FileMode.Open, FileAccess.Read);
      using var reader = new StreamReader(fileStream);

      return DXFFileUtilities.RequestBoundariesFromLineWork(reader, Path.GetFileNameWithoutExtension(dxfFileName), units, maxBoundariesToProcess, out boundaries);
    }
  }
}
