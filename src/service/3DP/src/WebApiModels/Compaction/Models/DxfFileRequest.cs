using System.IO;
using Microsoft.AspNetCore.Http;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  public class DxfFileRequest
  {
    public const int NOT_DEFINED = -1;

    private int _dxfUnits = NOT_DEFINED;

    public IFormFile DxfFile { get; set; }
    public IFormFile CoordinateSystemFile { get; set; }
    public int DxfUnits
    {
      get => _dxfUnits == NOT_DEFINED ? NOT_DEFINED : _dxfUnits;
      set => _dxfUnits = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of points the returned GeoJSON should include. 
    /// </summary>
    public int MaxVerticesPerBoundary { get; set; } = NOT_DEFINED;

    /// <summary>
    /// Gets or sets the maximum number of boundies within the DXF file to process.
    /// Starts at index 0.
    /// </summary>
    public int MaxBoundariesToProcess { get; set; }

    /// <summary>
    /// Gets or sets whether to convert the polyline to a closed polygon if it's defined as implicit.
    /// </summary>
    public bool ConvertLineStringCoordsToPolygon { get; set; }

    public override string ToString()
    {
      return $"{nameof(DxfFileRequest)}: " +
             $"{nameof(DxfUnits)}='{DxfUnits}', " +
             $"{nameof(MaxVerticesPerBoundary)}='{MaxVerticesPerBoundary}', " +
             $"{nameof(MaxBoundariesToProcess)}='{MaxBoundariesToProcess}', " +
             $"{nameof(ConvertLineStringCoordsToPolygon)}='{ConvertLineStringCoordsToPolygon}'";
    }

    public byte[] GetFileAsByteArray(IFormFile file)
    {
      if (file == null || file.Length <= 0) { return null; }

      using (var ms = new MemoryStream())
      {
        file.CopyTo(ms);

        return ms.ToArray();
      }
    }
  }
}
