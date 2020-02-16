using System;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class VehicleBusJ1939StatisticsReportBlock : NestedMessage
  {
    public byte SourceAddress;
    public ushort PGN;
    public int SPN;
    public short UTCDelta;
    public short SMUDelta;
    public int Minimum;
    public int Maximum;
    public int Average;
    public int StandardDeviation;
    public byte ScaleFactorExponent;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref SourceAddress);
      serializer(action, raw, ref bitPosition, 16, ref PGN);
      serializer(action, raw, ref bitPosition, 24, ref SPN);
      serializer(action, raw, ref bitPosition, 16, ref UTCDelta);
      serializer(action, raw, ref bitPosition, 16, ref SMUDelta);
      serializer(action, raw, ref bitPosition, 32, ref Minimum);
      serializer(action, raw, ref bitPosition, 32, ref Maximum);
      serializer(action, raw, ref bitPosition, 32, ref Average);
      serializer(action, raw, ref bitPosition, 32, ref StandardDeviation);
      serializer(action, raw, ref bitPosition, 8, ref ScaleFactorExponent);
    }

    public int GetUnitTypeID(int pgn, int spn, int sourceAddress)
    {
      return J1939ParameterReportPayloadParser.GetUnitTypeID(pgn, spn, sourceAddress);
    }
  }
}
