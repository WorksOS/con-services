using System.IO;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Files.DXF
{
  public static class DXFFileUtilities
  {
    public static DXFUtilitiesResult RequestBoundariesFromLineWork(StreamReader dxfFile, string baseName, DxfUnitsType units, int maxBoundariesToProcess)
    {
      var reader = new DXFReader(dxfFile);

      if (!reader.FindEntitiesSection())
      {
        return DXFUtilitiesResult.NoEntitiesSectionFound;
      }

      var extractor = new PolyLineExtractor(reader);
      var result = extractor.Extract(baseName, units, maxBoundariesToProcess);

      return result;
    }

    public static DXFUtilitiesResult RequestBoundariesFromLineWork(string dxfFileName, DxfUnitsType units, int maxBoundariesToProcess)
    {
      using var fileStream = new FileStream(dxfFileName, FileMode.Open, FileAccess.Read);
      using var reader = new StreamReader(fileStream);

      return DXFFileUtilities.RequestBoundariesFromLineWork(reader, Path.GetFileNameWithoutExtension(dxfFileName), units, maxBoundariesToProcess);
    }
  }
}
