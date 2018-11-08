namespace VSS.TRex.CoordinateSystems.Models
{
  public class CoordinateSystem
  {
    public string Id; // Represents the returned CSIB
    public int SystemId;
    public string SystemName;
    public string RecordName;
    public DatumInfo DatumInfo;
    public GeoidInfo GeooidInfo;
    public ZoneInfo ZoneInfo;
    public int DatumSystemId;
    public int GoidSystemId;
    public int ZoneSystemId;
    public int CompdCsepsg;
    public int ProjEpsg;
    public int DatumTransfoEpsg;
    public int EllipsoidEpsg;
    public int LocalDatumEpsg;
    public int GlobalDatumEpsg;
    public int LocalGeogCsepsg;
    public int LocalGeocCsepsg;
    public int GlobalGeogCsepsg;
    public int GlobalGeocCsepsg;
    public int VertCsepsg;
    public int VerticalDatumEpsg;
    public object Files;
  }
}
