namespace VSS.Hosted.VLCommon
{
  public class AssetUtilDailyFlattened
  {
    public long AssetID { get; set; }
    public string AssetName { get; set; }
    public string EquipmentVIN { get; set; }
    public int AssetIconID { get; set; }
    public int DeviceTypeID { get; set; }
    public string AssetSerialNumber { get; set; }
    public int DayKeyDate { get; set; }
    public double? IdleHours { get; set; }
    public double? WorkingHours { get; set; }
    public double? RuntimeHours { get; set; }
    public int RuntimeHoursCalloutTypeID { get; set; }
    public int IdleHoursCalloutTypeID { get; set; }
    public int WorkingHoursCalloutTypeID { get; set; }
  };
}