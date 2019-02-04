namespace VSS.Productivity3D.Models.Models
{
  public class DxfFileRequest
  {
    public const int NOT_DEFINED = -1;
    public string Filename { get; set; }
    public byte[] FileData { get; set; }
    public int DxfUnits = NOT_DEFINED;
    public string CoordinateSystemName { get; set; }

    public override string ToString()
    {
       return $"{nameof(DxfFileRequest)}: " +
              $"{nameof(Filename)}='{Filename}', " +
              $"{nameof(DxfUnits)}='{DxfUnits}', " +
              $"{nameof(CoordinateSystemName)}='{CoordinateSystemName}'";
    }
  }
}
