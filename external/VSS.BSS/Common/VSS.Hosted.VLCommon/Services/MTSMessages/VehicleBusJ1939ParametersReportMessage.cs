namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class VehicleBusJ1939ParametersReportMessage : MachineEventBlockVehicleBusPayload
  {
    public static new readonly int kPacketID = 0x06;
    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }

    public VehicleBusJ1939ParametersReportTypeEnum ReportType
    {
      get { return (VehicleBusJ1939ParametersReportTypeEnum)_reportType; }
      set { _reportType = (byte)value; }
    }
    internal byte _reportType;

    public VehicleBusJ1939ParametersReportBlock[] ParametersReportBlocks;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref _reportType);

      ParametersReportBlocks = (VehicleBusJ1939ParametersReportBlock[])
         serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 16, ParametersReportBlocks, typeof(VehicleBusJ1939ParametersReportBlock));
    }
  }
}
