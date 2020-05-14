using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Files.DXF
{
  /// <summary>
  /// Provides simple semantics for reading a DXF file and extracting all the closed poly lines from it
  /// </summary>
  public class PolyLineExtractor
  {
    private readonly DXFReader _reader;

    public PolyLineBoundaries Boundaries;

    public PolyLineExtractor(DXFReader reader)
    {
      _reader = reader;
    }

    public DXFUtilitiesResult Extract(string baseName, DxfUnitsType units, int maxBoundariesToProcess)
    {
      bool atEOF;
      var result = DXFUtilitiesResult.OK;

      Boundaries = new PolyLineBoundaries(units, maxBoundariesToProcess);

      do
      {
        if (_reader.GetBoundaryFromPolyLineEntity(true, out atEOF, out var boundary))
        {
          if (boundary != null)
          {
            if (boundary.Type == DXFLineWorkBoundaryType.Unknown)
              boundary.Type = DXFLineWorkBoundaryType.GenericBoundary;

            if (string.IsNullOrEmpty(boundary.Name))
              boundary.Name = $"{baseName}{Boundaries.Boundaries.Count + 1}";

            Boundaries.Boundaries.Add(boundary);
          }
        }
      } while (Boundaries.Boundaries.Count < maxBoundariesToProcess && result == DXFUtilitiesResult.OK && !atEOF);

      // Determine units in file
      return DXFUtilitiesResult.OK;
    }
  }
}
