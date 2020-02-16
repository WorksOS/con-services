using System;

using System.Text;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class VehicleBusJ1939ParametersReportBlock : NestedMessage
  {
    public byte CANBusInstance;
    public byte SourceAddress;
    public byte ProtectStatus;
    public byte AmberWarningStatus;
    public byte RedStopLampStatus;
    public byte MalfunctionIndicatorLampStatus;
    public ushort PGN;
    public int SPN;
    public byte[] ParameterPayload;
    public J1939Parameter parameter;
    
    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref CANBusInstance);
      serializer(action, raw, ref bitPosition, 8, ref SourceAddress);
      serializer(action, raw, ref bitPosition, 2, ref ProtectStatus);
      serializer(action, raw, ref bitPosition, 2, ref AmberWarningStatus);
      serializer(action, raw, ref bitPosition, 2, ref RedStopLampStatus);
      serializer(action, raw, ref bitPosition, 2, ref MalfunctionIndicatorLampStatus);
      serializer(action, raw, ref bitPosition, 16, ref PGN);
      serializer(action, raw, ref bitPosition, 24, ref SPN);
      if (action != SerializationAction.Hydrate && parameter != null && (ParameterPayload == null || ParameterPayload.Count() == 0))
      {
        ParameterPayload = parameter.GetBytes();
      }

      serializeLengthPrefixedBytes(action, raw, ref bitPosition, 8, ref ParameterPayload);
      
        if (action == SerializationAction.Hydrate)
      {
        parameter = J1939Parameter.Parse(PGN, SPN, SourceAddress, ParameterPayload);
      }
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder("VehicleBusJ1939ParametersReportBlock");
      builder.AppendFormat("\nCANBusInstance:  {0}", CANBusInstance.ToString());
      builder.AppendFormat("\nSourceAddress:  {0}", SourceAddress.ToString());
      builder.AppendFormat("\nProtectStatus:  {0}", ProtectStatus.ToString());
      builder.AppendFormat("\nAmberWarningStatus:  {0}", AmberWarningStatus.ToString());
      builder.AppendFormat("\nRedStopLampStatus:  {0}", RedStopLampStatus.ToString());
      builder.AppendFormat("\nMalfunctionIndicatorLampStatus:  {0}", MalfunctionIndicatorLampStatus.ToString());
      builder.AppendFormat("\nPGN:  {0}", PGN.ToString());
      builder.AppendFormat("\nSPN:  {0}", SPN.ToString());

      if (parameter != null)
      {
        builder.Append("\nParameters:");

        builder.AppendFormat("\n{0}", parameter.ToString().Replace("\0",string.Empty));
      }

      if (ParameterPayload != null)
      {
        builder.Append("\nPayload:");
        foreach (byte b in ParameterPayload)
        {
          builder.AppendFormat("\n{0}", b);
        }
      }
      return builder.ToString();
    }

    public class J1939Parameter
    {
      public float? DoubleValue = null;
      public string StringValue = null;
      private string PayloadString = null;
      public int unitType;

      public static J1939Parameter Parse(int pgn, int spn, byte sourceAddress, byte[] payload)
      {
        J1939Parameter j1939 = new J1939Parameter();
        try
        {
          UnitTypeEnum unitType = (UnitTypeEnum)J1939ParameterReportPayloadParser.GetUnitTypeID(pgn, spn, sourceAddress);
          
          if ((unitType != UnitTypeEnum.ASCII && unitType != UnitTypeEnum.Unknown) || (unitType == UnitTypeEnum.Unknown && payload.Length == 4))
          {
            j1939.DoubleValue = BitConverter.ToSingle(payload, 0);
            j1939.unitType = (int)unitType;
          }
          else
          {
            j1939.PayloadString = Encoding.ASCII.GetString(payload, 0, payload.Length);
            int pos = J1939ParameterReportPayloadParser.PositionInString(pgn, spn, sourceAddress);
            if (!string.IsNullOrEmpty(j1939.PayloadString))
            {
              string[] strArray = j1939.PayloadString.Split('*');
              if (strArray.Length > pos)
              {
                j1939.StringValue = strArray[pos];
                j1939.unitType = (int)UnitTypeEnum.ASCII;
                }
              else if (strArray.Length == 1)
              {
                j1939.StringValue = strArray[0];
                j1939.unitType = (int)UnitTypeEnum.ASCII;
                }
            }
          }
        }
        catch (Exception){}

        if (j1939 != null && (j1939.DoubleValue.HasValue || !string.IsNullOrEmpty(j1939.StringValue)))
          return j1939;
        else
          return null;
      }

      public byte[] GetBytes()
      {
        if (DoubleValue.HasValue)
          return BitConverter.GetBytes(DoubleValue.Value);
        if (!string.IsNullOrEmpty(PayloadString))
          return Encoding.ASCII.GetBytes(PayloadString);

        return null;
      }

      public override string ToString()
      {
        StringBuilder builder = new StringBuilder("J1939ParameterValue");
        if(DoubleValue.HasValue)
          builder.AppendFormat("\nDoubleValue:  {0}", DoubleValue.ToString());
        if(!string.IsNullOrEmpty(StringValue))
          builder.AppendFormat("\nStringValue:  {0}", PayloadString);
        if(unitType != 0)
          builder.AppendFormat("\nUnitType:  {0}", ((UnitTypeEnum)unitType).ToString());
        return builder.ToString();
      }
    }
  }
}
