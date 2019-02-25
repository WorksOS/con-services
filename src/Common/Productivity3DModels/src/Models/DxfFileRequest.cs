using System.IO;
using Microsoft.AspNetCore.Http;

namespace VSS.Productivity3D.Models.Models
{
  public class DxfFileRequest
  {
    private int _dxfUnits = NOT_DEFINED;
    public const int NOT_DEFINED = -1;

    public IFormFile DxfFile { get; set; }
    public IFormFile CoordinateSystemFile { get; set; }
    public int DxfUnits
    {
      get => _dxfUnits == NOT_DEFINED ? NOT_DEFINED : _dxfUnits;
      set => _dxfUnits = value;
    }

    public int MaxBoundariesToProcess { get;set; }

    public override string ToString()
    {
      return $"{nameof(DxfFileRequest)}: " +
             $"{nameof(DxfUnits)}='{DxfUnits}', " +
             $"{nameof(MaxBoundariesToProcess)}='{MaxBoundariesToProcess}'";
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
