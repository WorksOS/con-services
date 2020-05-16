using System.IO;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Files.DXF
{
  public static class DXFFileUtilities
  {
    public static DXFUtilitiesResult RequestBoundariesFromLineWork(StreamReader dxfFileStream, string baseName, DxfUnitsType units, uint maxBoundariesToProcess, out PolyLineBoundaries boundaries)
    {
      boundaries = null;
      var reader = new DXFReader(dxfFileStream, units);

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

    public static DXFUtilitiesResult RequestBoundariesFromLineWork(string dxfFileName, DxfUnitsType units, uint maxBoundariesToProcess, out PolyLineBoundaries boundaries)
    {
      using var fileStream = new FileStream(dxfFileName, FileMode.Open, FileAccess.Read);
      using var reader = new StreamReader(fileStream);

      return DXFFileUtilities.RequestBoundariesFromLineWork(reader, Path.GetFileNameWithoutExtension(dxfFileName), units, maxBoundariesToProcess, out boundaries);
    }
  }
}
