namespace VSS.Productivity3D.Models.Models
{
  public class DxfFileRequest
  {
    private int _dxfUnits = NOT_DEFINED;

    public const int NOT_DEFINED = -1;
    public string Filename { get; set; }
    public byte[] FileData { get; set; }
    public int DxfUnits
    {
      get => _dxfUnits == NOT_DEFINED ? NOT_DEFINED : _dxfUnits;
      set => _dxfUnits = value;
    }

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
