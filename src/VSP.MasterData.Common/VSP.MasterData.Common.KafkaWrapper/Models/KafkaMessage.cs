namespace VSP.MasterData.Common.KafkaWrapper.Models
{
  public class KafkaMessage
  {
    public string Key { get; set; }
    public string Value { get; set; }
    public long OffSet { get; set; }
    public int PartitionId { get; set; }
  }
}