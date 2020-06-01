using VSS.Productivity3D.Models.Models.Files;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Files.DXF
{
  /// <summary>
  /// Provides simple semantics for reading a DXF file and extracting all the closed poly lines from it
  /// </summary>
  public class PolyLineBoundaryExtractor
  {
    private readonly DXFReader _reader;

    public PolyLineBoundaries Boundaries;

    public PolyLineBoundaryExtractor(DXFReader reader)
    {
      _reader = reader;
    }

    public DXFUtilitiesResult Extract(DxfUnitsType units, uint maxBoundariesToProcess, bool convertLineStringCoordsToPolygon)
    {
      bool atEOF;

      Boundaries = new PolyLineBoundaries(units, maxBoundariesToProcess);

      do
      {
        if (_reader.GetBoundaryFromPolyLineEntity(!convertLineStringCoordsToPolygon, out atEOF, out var boundary))
        {
          if (boundary != null && boundary.Boundary.Points.Count >= 3)
          {
            if (boundary.Type == DXFLineWorkBoundaryType.Unknown)
              boundary.Type = DXFLineWorkBoundaryType.GenericBoundary;

            if (string.IsNullOrEmpty(boundary.Name))
              boundary.Name = $"{Boundaries.Boundaries.Count + 1}";

            Boundaries.Boundaries.Add(boundary);
          }
        }
      } while (Boundaries.Boundaries.Count < maxBoundariesToProcess && !atEOF);

      // Determine units in file
      return DXFUtilitiesResult.Ok;
    }
  }
}
