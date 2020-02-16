namespace VSS.Hosted.VLCommon.Bss
{
  public class ServiceViewInfoDto
  {
    public long ServiceViewId { get; set; }
    public string ServiceTypeName { get; set; }
    public long CustomerId { get; set; }
    public string CustomerName { get; set; }
    public long AssetId { get; set; }
    public string AssetSerialNumber { get; set; }
    public int StartDateKey { get; set; }
    public int EndDateKey { get; set; }
  }
}