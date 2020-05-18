namespace VSS.MasterData.Project.WebAPI.Common.Models.DeviceStatus
{
  public class Network
  {
    public string type { get; set; }
    public double signal { get; set; }
    public string state { get; set; }
    public short dbm { get; set; }
    public short uptime { get; set; }
    public string carrier { get; set; }
    public bool roaming { get; set; }
    public string cellTech { get; set; }
    public string mcc { get; set; }
    public string mnc { get; set; }
    public string simId { get; set; }
    public string iccId { get; set; }
    public string phoneNumber { get; set; }
    public long modemResets { get; set; }
    public string apn { get; set; }
    public string apnUsername { get; set; }
    public bool apnPasswordSet { get; set; }
    public short txData { get; set; }
    public short rxData { get; set; }
    public string regulatoryDomain { get; set; }
    public string regulatoryDomainMethod { get; set; }
  }
}
