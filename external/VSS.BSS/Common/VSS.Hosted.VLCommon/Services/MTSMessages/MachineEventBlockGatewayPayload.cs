using System;
using System.Text;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public abstract class MachineEventBlockGatewayPayload : MachineEventBlockPayload
  {
    public override MessageCategory Category
    {
      get
      {
        return MessageCategory.MachineEventBlockGatewayPayload;
      }
    }
  }

  public class MachineActivityEventBlock : MachineEventBlockGatewayPayload
  {
    public static new readonly int kPacketID = 0x46;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte SubType;

    public NestedMessage Message;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref SubType);

      if (SubType == (byte)MachineActivityEventSubTypes.IdleStartStopReport && Message == null)
      {
        IdleStartStopReport idle = new IdleStartStopReport();
        Message = idle;
      }
      else if (SubType == (byte)MachineActivityEventSubTypes.MSSKeyIDReport && Message == null)
      {
        MSSKeyIDReport mss = new MSSKeyIDReport();
        Message = mss;
      }
      else if (SubType == (byte)MachineActivityEventSubTypes.TamperSecurityStatus && Message == null)
      {
        TamperSecurityStatusInformationMessage msg = new TamperSecurityStatusInformationMessage();
        Message = msg;
      }

      if (Message != null)
      {
        Message.Parent = this;
        Message.Serialize(action, raw, ref bitPosition);
      }
    }

    public enum MachineActivityEventSubTypes
    {
      IdleStartStopReport = 0x01,
      MSSKeyIDReport = 0x05,
      TamperSecurityStatus = 0x06
    }
  }

  public class FuelEnginePayloadCycleCountBlock : MachineEventBlockGatewayPayload
  {
    public static new readonly int kPacketID = 0x45;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte SubType;

    public NestedMessage Message;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref SubType);

      if (SubType == (byte)FuelPayloadCycleSubTypes.FuelEngineReport && Message == null)
      {
        FuelEngineReport fuel = new FuelEngineReport();
        Message = fuel;
      }
      else if (SubType == (byte)FuelPayloadCycleSubTypes.PayloadAndCycleCount && Message == null)
      {
        PayloadAndCycleCountReport pcc = new PayloadAndCycleCountReport();
        Message = pcc;
      }

      if (Message != null)
      {
        Message.Parent = this;
        Message.Serialize(action, raw, ref bitPosition);
      }
    }

    public enum FuelPayloadCycleSubTypes
    {
      FuelEngineReport = 0x00,
      PayloadAndCycleCount = 0x03
    }
  }

  public class FaultEventReporting : MachineEventBlockGatewayPayload
  {
    public static new readonly int kPacketID = 0x21;
    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte SubType = 0x00;
    public byte TransactionVersion = 0x01;
    public byte EventLevel;
    public ushort ECMIdentifier;
    public ushort EventIdentifier;
    public byte NumberOfOccurences;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref SubType);
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      serializer(action, raw, ref bitPosition, 8, ref EventLevel);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref ECMIdentifier);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref EventIdentifier);
      serializer(action, raw, ref bitPosition, 8, ref NumberOfOccurences);
    }
  }

  public class DiagnosticReporting : MachineEventBlockGatewayPayload
  {
    public static new readonly int kPacketID = 0x22;
    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte SubType = 0x00;
    public byte TransactionVersion = 0x00;
    public byte DiagnosticLevel;
    public ushort ECMIdentifier;
    public ushort ComponentIdentifier;
    public byte FailureModeIdentifier;
    public byte NumberOfOccurences;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref SubType);
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      serializer(action, raw, ref bitPosition, 8, ref DiagnosticLevel);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref ECMIdentifier);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref ComponentIdentifier);
      serializer(action, raw, ref bitPosition, 8, ref FailureModeIdentifier);
      serializer(action, raw, ref bitPosition, 8, ref NumberOfOccurences);
    }
  }

  public class J1939DiagnosticReporting : MachineEventBlockGatewayPayload
  {
    public static new readonly int kPacketID = 0x23;
    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte SubType = 0x01;
    public byte TransactionVersion = 0x00;
    public byte DataLinkType;
    public ushort ECMIdentifier; 
    public byte FailureModeIdentifier;

    public byte? OccurrenceCount
    {
      get
      {
        if (occurrenceCount == 0xFF)
          return null;

        return occurrenceCount;
      }
      set
      {
        if (!value.HasValue)
          occurrenceCount = 0xFF;
        else
          occurrenceCount = value.Value;
      }
    }
    private byte occurrenceCount;

    public int SignificantPartNumber
    {
      get
      {
        return (significantPartNumber & 0x07FFFF);
      }
      set
      {
        significantPartNumber = value;
      }
    }
    private int significantPartNumber;

    public bool ProtectLampStatus;
    public bool AmberWarningLampStatus;
    public bool RedStopLampStatus;
    public bool MalfunctionIndicatorLampStatus;    

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref SubType);
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      serializer(action, raw, ref bitPosition, 1, ref DataLinkType);
      filler(ref bitPosition, 7);

      BigEndianSerializer(action, raw, ref bitPosition, 2, ref ECMIdentifier);

      BigEndianSerializer(action, raw, ref bitPosition, 3, ref significantPartNumber);      
      serializer(action, raw, ref bitPosition, 8, ref FailureModeIdentifier);
      serializer(action, raw, ref bitPosition, 8, ref occurrenceCount);

      serializer(action, raw, ref bitPosition, 2, ref ProtectLampStatus);
      serializer(action, raw, ref bitPosition, 2, ref AmberWarningLampStatus);
      serializer(action, raw, ref bitPosition, 2, ref RedStopLampStatus);
      serializer(action, raw, ref bitPosition, 2, ref MalfunctionIndicatorLampStatus);      
      filler(ref bitPosition, 8);
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder("J1939DiagnosticReporting");
      builder.AppendFormat("\nDataLink Supported:             {0}", DataLinkType.ToString());
      builder.AppendFormat("\nECM identifier:                 {0}", ECMIdentifier.ToString());
      builder.AppendFormat("\nFailure Mode Identifier(FMI):   {0}", FailureModeIdentifier.ToString());
      builder.AppendFormat("\nOccurrence Count:               {0}", OccurrenceCount.ToString());
      builder.AppendFormat("\nSignificant Part Number (SPN):  {0}", SignificantPartNumber.ToString());
      builder.AppendFormat("\nProtectLampStatus:              {0}", ProtectLampStatus.ToString());
      builder.AppendFormat("\nAmberWarningLampStatus:         {0}", AmberWarningLampStatus.ToString());
      builder.AppendFormat("\nRedStopLampStatus:              {0}", RedStopLampStatus.ToString());
      builder.AppendFormat("\nMalfunctionIndicatorLampStatus: {0}", MalfunctionIndicatorLampStatus.ToString());

      return builder.ToString();
    }
  }

  public class ECMInformationMessage : MachineEventBlockGatewayPayload
  {
    public static new readonly int kPacketID = 0x51;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public string[] EngineSerialNumbers = null;
    public string[] TransmissionSerialNumbers = null;
    public DeviceIDData[] DeviceData = null;
    public byte TransactionVersion = 0x01;

    private byte SubType = 0x02;
    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      serializer(action, raw, ref bitPosition, 8, ref SubType);
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);

      byte engineCount = (EngineSerialNumbers == null) ? (byte)0 : (byte)EngineSerialNumbers.Length;
      byte transmissionCount = (TransmissionSerialNumbers == null) ? (byte)0 : (byte)TransmissionSerialNumbers.Length;

      serializer(action, raw, ref bitPosition, 4, ref engineCount);
      serializer(action, raw, ref bitPosition, 4, ref transmissionCount);

      if (action == SerializationAction.Hydrate)
      {
        EngineSerialNumbers = new string[engineCount];
        TransmissionSerialNumbers = new string[transmissionCount];
      }

      for (int i = 0; i < engineCount; i++)
      {
        serializeFixedLengthString(action, raw, ref bitPosition, 8, ref EngineSerialNumbers[i]);
      }
      for (int i = 0; i < transmissionCount; i++)
      {
        serializeFixedLengthString(action, raw, ref bitPosition, 8, ref TransmissionSerialNumbers[i]);
      }

      byte deviceIDDataCount = (DeviceData == null) ? (byte)0 : (byte)DeviceData.Length;

      serializer(action, raw, ref bitPosition, 8, ref deviceIDDataCount);

      if (action == SerializationAction.Hydrate)
        DeviceData = new DeviceIDData[deviceIDDataCount];

      DeviceData = (DeviceIDData[])serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 0, DeviceData, typeof(DeviceIDData));
    }
  }

  public class GatewayAdministrationMessage : MachineEventBlockGatewayPayload
  {
    public static new readonly int kPacketID = 0x53;
    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte SubType;
    public NestedMessage Message;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      serializer(action, raw, ref bitPosition, 8, ref SubType);
      if (SubType == 0x00 && Message == null)
      {
        DigitalInputsAdminInformation admin = new DigitalInputsAdminInformation();
        Message = admin;
      }
      else if (SubType == 0x01 && Message == null)
      {
        MaintenanceAdministrationInformation maint = new MaintenanceAdministrationInformation();
        Message = maint;
      }
      else if (SubType == 0xFF && Message == null)
      {
        AdministrationFailedDelivery failed = new AdministrationFailedDelivery();
        Message = failed;
      }
      else if (SubType == 0x02 && Message == null)
      {
        TamperSecurityAdministrationInformationMessage machine = new TamperSecurityAdministrationInformationMessage();
        Message = machine;
      }

      if (Message != null)
      {
        Message.Parent = this;
        Message.Serialize(action, raw, ref bitPosition);
      }
    }
  }

  public class SMHAdjustmentMessage : MachineEventBlockGatewayPayload
  {
    public static new readonly int kPacketID = 0x3A;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte SubType;
    public byte TransactionVersion;
    public TimeSpan SMUBeforeAdj
    {
      get { return TimeSpan.FromHours(smuBeforeAdjTenthHours / 10); }
      set { smuBeforeAdjTenthHours = (int)(value.TotalHours * 10); }
    }
    public TimeSpan SMUAfterAdj
    {
      get { return TimeSpan.FromHours(smuAfterAdjTenthHours / 10); }
      set { smuAfterAdjTenthHours = (int)(value.TotalHours * 10); }
    }

    private int smuBeforeAdjTenthHours;
    private int smuAfterAdjTenthHours;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      serializer(action, raw, ref bitPosition, 8, ref SubType);
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      BigEndianSerializer(action, raw, ref bitPosition, 3, ref smuBeforeAdjTenthHours);
      BigEndianSerializer(action, raw, ref bitPosition, 3, ref smuAfterAdjTenthHours);
    }
    #region "Unconverted value properties for OEM Data Feed"
    public int SMUBeforeAdjUnConverted
    {
      get { return smuBeforeAdjTenthHours; }
    }

    public int SMUAfterAdjUnConverted
    {
      get { return smuAfterAdjTenthHours; }
    }
    #endregion
  }

  public class TMSMessageBlock : MachineEventBlockGatewayPayload
  {
    public static new readonly int kPacketID = 0x44;

    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }

    public byte SubType;
    public byte packet;
    public NestedMessage Message;   
    

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      
      serializer(action, raw, ref bitPosition, 8, ref SubType);

      if (SubType == (byte)TMSMessageSubTypes.TMSInformationMessage && Message == null)
      {
        TMSInformationMessage tmsInformationMessage = new TMSInformationMessage();
        Message = tmsInformationMessage;
      }
      else if (SubType == (byte)TMSMessageSubTypes.TMSReportMessage && Message == null)
      {
        TMSReportMessage tmsReportMessage = new TMSReportMessage();
        Message = tmsReportMessage;
      }

      if (Message != null)
      {
        Message.Parent = this;
        Message.Serialize(action, raw, ref bitPosition);
      }
    }

    public enum TMSMessageSubTypes
    {
      TMSReportMessage = 0x01,
      TMSInformationMessage = 0x02
    }
  }

  public class UnkownMachineEventGatewayData : MachineEventBlockGatewayPayload
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
