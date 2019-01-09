using Microsoft.AspNetCore.Http;

namespace VSS.Productivity3D.Models.Models
{
  public class DxfFileRequest
  {
    public string Filename { get; set; }
    public IFormFile FileData { get; set; }
    public int DistanceUnits { get; set; }
    public string CoordinateSystemName { get; set; }

    public override string ToString()
    {
       return $"{nameof(DxfFileRequest)}: " +
              $"{nameof(Filename)}='{Filename}', " +
              $"{nameof(DistanceUnits)}='{DistanceUnits}', " +
              $"{nameof(CoordinateSystemName)}='{CoordinateSystemName}'";
    }
  }
}
