using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VSS.Hosted.VLCommon;
using Microsoft.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class MachineEventBlock : NestedMessage
  {
    public enum VehicleBusProtocolEnum
    {
      J1939Public = 0,
      J1939ProprietaryCAT = 1,
      J1939ProprietaryCNH = 2,
      ISO11783Public = 3,
      ISO11783ProprietaryCNH = 4,
    }

    public override MessageCategory Category
    {
      get { return MessageCategory.MachineEventBlock; }
    }

    public TimeSpan DeltaTime
    {
      get { return TimeSpan.FromSeconds(Delta); }
      set { Delta = (short)value.TotalSeconds; }
    }

    public MachineEventSourceEnum Source
    {
      get { return (MachineEventSourceEnum)SourceRaw; }
      set { SourceRaw = (byte)value; }
    }

    private short Delta;
    private byte SourceRaw;
    public MachineEventBlockRadioPayload RadioData = null;
    public MachineEventBlockGatewayPayload GatewayData = null;
    public MachineEventBlockVehicleBusPayload VehicleBusData = null;
    public bool? IsVehicleBusTrimbleAbstraction
    {
      get 
      {
        if (Source != MachineEventSourceEnum.VehicleBus)
          return null;
        else return isVehicleBusTrimbleAbstraction;
      }
      set
      {
        if (Source == MachineEventSourceEnum.VehicleBus && value.HasValue)
          isVehicleBusTrimbleAbstraction = value.Value;
      }
    }
    private bool isVehicleBusTrimbleAbstraction;

    public VehicleBusProtocolEnum? Protocol
    {
      get
      {
        if (Source != MachineEventSourceEnum.VehicleBus)
          return null;
        else
          return (VehicleBusProtocolEnum)protocol;
      }
      set
      {
        if (Source == MachineEventSourceEnum.VehicleBus && value.HasValue)
          protocol = (byte)value.Value;
      }
    }
    private byte protocol;

    public byte[] VehicleBusBinary = null;
    private lengthBackfill length;
    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 16, ref Delta);
      serializer(action, raw, ref bitPosition, 8, ref SourceRaw);

      if (Source == MachineEventSourceEnum.Radio)
        serializeSingleEmbeddedMachineEventPayloadRadioMessage(action, raw, ref bitPosition, 16, ref RadioData);
      else if(Source == MachineEventSourceEnum.Gateway)
        serializeSingleEmbeddedMachineEventPayloadGatewayMessage(action, raw, ref bitPosition, 16, ref GatewayData);
      else
      {
        length = lengthBackfill.Mark(action, raw, ref bitPosition, 16);
        serializer(action, raw, ref bitPosition, 3, ref isVehicleBusTrimbleAbstraction);
        serializer(action, raw, ref bitPosition, 5, ref protocol);
        if (IsVehicleBusTrimbleAbstraction.HasValue && !IsVehicleBusTrimbleAbstraction.Value)
        {
          SerializeVehicleBusInfo(action, raw, ref bitPosition);
        }
        else
        {
          serializeSingleEmbeddedMachineEventPayloadVehicleBusMessage(action, raw, ref bitPosition, 0, ref VehicleBusData);
        }
        length.Backfill(bitPosition);
      }
    }

    private void SerializeVehicleBusInfo(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      if (Protocol.HasValue)
      {
        switch (Protocol.Value)
        {
          case VehicleBusProtocolEnum.J1939ProprietaryCNH:
            if (action == SerializationAction.Hydrate)
              serializeFixedLengthBytes(action, raw, ref bitPosition, length.BytesRemaining(bitPosition), ref VehicleBusBinary);
            else
              serializeFixedLengthBytes(action, raw, ref bitPosition, (uint)VehicleBusBinary.Length, ref VehicleBusBinary);
            break;
          default:
            throw new NotImplementedException(string.Format("Protcol {0} not yet implemented", Protocol.Value.ToString()));
        }
      }
    }
  }

  public abstract class MachineEventBlockPayload : UserDataMessage
  {
      public override MessageCategory Category
      {
        get { return MessageCategory.MachineEventBlockPayload; }
      }
  }

  public class UnkownMachineEventData : MachineEventBlockPayload
  {
    public static new readonly int kPacketID = 0x81;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      if (action == SerializationAction.Hydrate)
      {
        uint realDataLength = bytesLeftInMessage(bitPosition);

        serializeFixedLengthBytes(action, raw, ref bitPosition, realDataLength, ref Data);
      }
      else
      {
        serializeFixedLengthBytes(action, raw, ref bitPosition, (uint)(Data == null ? 0 : Data.Length), ref Data);
      }
    }

    public byte[] Data;
  }
}
