namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class VehicleBusJ1939StatisticsReportMessage : MachineEventBlockVehicleBusPayload
  {
    public static new readonly int kPacketID = 0x07;
    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }
    
    public VehicleBusJ1939StatisticsReportBlock[] StatisticsReportBlocks;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      
      byte parameterBlockCount = 0;

      if (StatisticsReportBlocks != null)
      {
        parameterBlockCount = (byte)StatisticsReportBlocks.Length;
      }
      StatisticsReportBlocks = (VehicleBusJ1939StatisticsReportBlock[])
         serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 16, StatisticsReportBlocks, typeof(VehicleBusJ1939StatisticsReportBlock));
    }
  }
}
