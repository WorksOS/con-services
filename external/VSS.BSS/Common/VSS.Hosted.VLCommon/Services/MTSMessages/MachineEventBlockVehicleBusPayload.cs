using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VSS.Hosted.VLCommon.MTSMessages
{
  public abstract class MachineEventBlockVehicleBusPayload : MachineEventBlockPayload
  {
    public override PlatformMessage.MessageCategory Category
    {
      get
      {
        return MessageCategory.MachineEventBlockVehicleBusPayload;
      }
    }
  }

  public class VehicleBusAddressClaimMessage : MachineEventBlockVehicleBusPayload
  {
    public static new readonly int kPacketID = 0x01;
    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }

    public VehicleBusECMAddressClaim[] DeviceECMs;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      byte count = (DeviceECMs == null) ? (byte)0 : (byte)DeviceECMs.Length;

      serializer(action, raw, ref bitPosition, 8, ref count);

      if (action == SerializationAction.Hydrate)
        DeviceECMs = new VehicleBusECMAddressClaim[count];

      for (int i = 0; i < count; i++)
      {
        if (action == SerializationAction.Hydrate)
          DeviceECMs[i] = new VehicleBusECMAddressClaim();

        DeviceECMs[i].Serialize(action, raw, ref bitPosition);
      }
    }
  }

  public class VehicleBusECMAddressClaim : NestedMessage
  {
    public byte SourceAddress;
    public byte CANBusInstance;
    public bool ArbitraryAddressCapable;
    public byte IndustryGroup;
    public byte VehicleSystemInstance;
    public byte VehicleSystem;
    public byte Function;
    public byte FunctionInstance;
    public byte ECUInstance;
    public ushort ManufacturerCode;
    public int IdentityNumber;

    public string GetECMIDFromJ1939Name()
    {
      byte[] raw = new byte[8];
      uint bitPosition = 0;
      const SerializationAction action = SerializationAction.Serialize;
      serializer(action, raw, ref bitPosition, 21, ref IdentityNumber);
      serializer(action, raw, ref bitPosition, 11, ref ManufacturerCode);
      serializer(action, raw, ref bitPosition, 3, ref ECUInstance);
      serializer(action, raw, ref bitPosition, 5, ref FunctionInstance);
      serializer(action, raw, ref bitPosition, 8, ref Function);
      filler(ref bitPosition, 1);
      serializer(action, raw, ref bitPosition, 7, ref VehicleSystem);
      serializer(action, raw, ref bitPosition, 4, ref VehicleSystemInstance);
      serializer(action, raw, ref bitPosition, 3, ref IndustryGroup);
      serializer(action, raw, ref bitPosition, 1, ref ArbitraryAddressCapable);
      
      return BitConverter.ToUInt64(raw, 0).ToString();
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref CANBusInstance);
      serializer(action, raw, ref bitPosition, 8, ref SourceAddress);
      serializer(action, raw, ref bitPosition, 21, ref IdentityNumber);
      serializer(action, raw, ref bitPosition, 11, ref ManufacturerCode);
      serializer(action, raw, ref bitPosition, 3, ref ECUInstance);
      serializer(action, raw, ref bitPosition, 5, ref FunctionInstance);
      serializer(action, raw, ref bitPosition, 8, ref Function);
      filler(ref bitPosition, 1);
      serializer(action, raw, ref bitPosition, 7, ref VehicleSystem);
      serializer(action, raw, ref bitPosition, 4, ref VehicleSystemInstance);
      serializer(action, raw, ref bitPosition, 3, ref IndustryGroup);
      serializer(action, raw, ref bitPosition, 1, ref ArbitraryAddressCapable);
    }
  }

  public class VehicleBusECMInformation : MachineEventBlockVehicleBusPayload
  {
    public static new readonly int kPacketID = 0x02;
    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }

    public byte EngineCount;
    public byte TransmissionCount;

    public VehicleBusECMInfoData[] DeviceECMs;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 4, ref EngineCount);
      serializer(action, raw, ref bitPosition, 4, ref TransmissionCount);

      byte count = (DeviceECMs == null) ? (byte)0 : (byte)DeviceECMs.Length;

      serializer(action, raw, ref bitPosition, 8, ref count);

      if (action == SerializationAction.Hydrate)
        DeviceECMs = new VehicleBusECMInfoData[count];

      for (int i = 0; i < count; i++)
      {
        if (action == SerializationAction.Hydrate)
          DeviceECMs[i] = new VehicleBusECMInfoData();

        DeviceECMs[i].Serialize(action, raw, ref bitPosition);
      }
    }
  }

  public class VehicleBusECMInfoData : NestedMessage
  {
    public byte CANBusInstance;
    public byte ECMSourceAddress;
    public string SoftwarePartNumber;
    public string SoftwareDescription;
    public string SoftwareReleaseDate;
    public string PartNumber;
    public string SerialNumber;

    public string ECMIDFromAddressClaimCache(string serialNumber, Dictionary<byte, string> cacheOverrideAddressClaims)
    {
      if ((null != cacheOverrideAddressClaims) && (cacheOverrideAddressClaims.ContainsKey(ECMSourceAddress)))
      {
        return cacheOverrideAddressClaims[ECMSourceAddress];
      }
      return ECMAddressClaims.GetECMIDFromSourceAddress(serialNumber, ECMSourceAddress);
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref CANBusInstance);
      serializer(action, raw, ref bitPosition, 8, ref ECMSourceAddress);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref SoftwarePartNumber);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref SoftwareDescription);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref SoftwareReleaseDate);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref PartNumber);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref SerialNumber);
    }
  }

  public class VehicleBusFuelEngineReport : MachineEventBlockVehicleBusPayload
  {
    public static new readonly int kPacketID = 0x03;
    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }

    public VehicleBusFuelECMData[] reportingECMs;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      byte count = (reportingECMs == null) ? (byte)0 : (byte)reportingECMs.Length;

      serializer(action, raw, ref bitPosition, 8, ref count);

      if (action == SerializationAction.Hydrate)
        reportingECMs = new VehicleBusFuelECMData[count];

      for (int i = 0; i < count; i++)
      {
        if (action == SerializationAction.Hydrate)
          reportingECMs[i] = new VehicleBusFuelECMData();

        reportingECMs[i].Serialize(action, raw, ref bitPosition);
      }
    }
  }

  public class VehicleBusFuelECMData : NestedMessage
  {
    public byte CANBusInstance;
    public byte ECMSourceAddress;
    public double FuelConsumption
    {
      get { return fuelConsumption / 8.0; }
      set { fuelConsumption = (uint)(value * 8); }
    }
    public byte FuelLevel;
    public TimeSpan TotalEngineIdleTime
    {
      get { return TimeSpan.FromHours((double)totalEngineIdleTime * 0.05); }
      set { totalEngineIdleTime = (uint)(value.TotalHours / 0.05); }
    }
    
    public ushort NumberEngineStarts;
    public double TotalEngineRevolutions
    {
      get { return totalEngineRevolutions * 4.0; }
      set { totalEngineRevolutions = (uint)(value / 4); }
    }
    public double TotalIdleFuel
    {
      get { return totalIdleFuel / 8.0; }
      set { totalIdleFuel = (uint)(value * 8); }
    }

    private uint fuelConsumption;
    private uint totalEngineIdleTime;
    private uint totalEngineRevolutions;
    private uint totalIdleFuel;

    public string ECMIDFromAddressClaimCache(string serialNumber, Dictionary<byte, string> cacheOverrideAddressClaims)
    {
      if ((null != cacheOverrideAddressClaims) && (cacheOverrideAddressClaims.ContainsKey(ECMSourceAddress)))
      {
        return cacheOverrideAddressClaims[ECMSourceAddress];
      }
      return ECMAddressClaims.GetECMIDFromSourceAddress(serialNumber, ECMSourceAddress);
    }

    public uint GetFuelConsumptionBeforeConversion()
    {
      return fuelConsumption;
    }
    public uint GetTotalEngineIdleTimeBeforeConversion()
    {
      return totalEngineIdleTime;
    }
    public uint GetTotalEngineRevolutionsBeforeConversion()
    {
      return totalEngineRevolutions;
    }
    public uint GetTotalIdleFuelBeforeConversion()
    {
      return totalIdleFuel;
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref CANBusInstance);
      serializer(action, raw, ref bitPosition, 8, ref ECMSourceAddress);
      serializer(action, raw, ref bitPosition, 32, ref fuelConsumption);
      serializer(action, raw, ref bitPosition, 8, ref FuelLevel);
      serializer(action, raw, ref bitPosition, 32, ref totalEngineIdleTime);
      serializer(action, raw, ref bitPosition, 16, ref NumberEngineStarts);
      serializer(action, raw, ref bitPosition, 32, ref totalEngineRevolutions);
      serializer(action, raw, ref bitPosition, 32, ref totalIdleFuel);
    }

    #region "Unconverted value properties for OEM Data Feed"
    public uint FuelConsumptionUnConverted
    {
      get { return fuelConsumption; }
    }
    public uint TotalEngineIdleTimeUnConverted
    {
      get { return totalEngineIdleTime; }
    }
    public uint TotalEngineRevolutionsUnConverted
    {
      get { return totalEngineRevolutions; }
    }
    public uint TotalIdleFuelUnConverted
    {
      get { return totalIdleFuel; }
    }
    #endregion

  }

  public class VehicleBusDiagnosticMessage : MachineEventBlockVehicleBusPayload
  {
    public static new readonly int kPacketID = 0x05;
    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }

    public VehicleBusECMDiagnostic[] Diagnostic;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      byte count = (Diagnostic == null) ? (byte)0 : (byte)Diagnostic.Length;

      serializer(action, raw, ref bitPosition, 8, ref count);

      if (action == SerializationAction.Hydrate)
        Diagnostic = new VehicleBusECMDiagnostic[count];

      for (int i = 0; i < count; i++)
      {
        if (action == SerializationAction.Hydrate)
          Diagnostic[i] = new VehicleBusECMDiagnostic();

        Diagnostic[i].Serialize(action, raw, ref bitPosition);
      }
    }
  }

  public class VehicleBusECMDiagnostic : NestedMessage
  {
    public byte CANBusInstance;
    public byte ECMSourceAddress;

    public bool ProtectLampStatus;
    public bool AmberWarningLampStatus;
    public bool RedStopLampStatus;
    public bool MalfunctionIndicatorLampStatus;
    public int SuspectParameterNumber;

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

    public string ECMIDFromAddressClaimCache(string serialNumber, Dictionary<byte, string> cacheOverrideAddressClaims)
    {
      if ((null != cacheOverrideAddressClaims) && (cacheOverrideAddressClaims.ContainsKey(ECMSourceAddress)))
      {
        return cacheOverrideAddressClaims[ECMSourceAddress];
      }
      return ECMAddressClaims.GetECMIDFromSourceAddress(serialNumber, ECMSourceAddress);
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref CANBusInstance);
      serializer(action, raw, ref bitPosition, 8, ref ECMSourceAddress);
      serializer(action, raw, ref bitPosition, 2, ref ProtectLampStatus);
      serializer(action, raw, ref bitPosition, 2, ref AmberWarningLampStatus);
      serializer(action, raw, ref bitPosition, 2, ref RedStopLampStatus);
      serializer(action, raw, ref bitPosition, 2, ref MalfunctionIndicatorLampStatus);
      serializer(action, raw, ref bitPosition, 24, ref SuspectParameterNumber);
      serializer(action, raw, ref bitPosition, 8, ref FailureModeIdentifier);
      serializer(action, raw, ref bitPosition, 8, ref occurrenceCount);

    }
  }

  // For New TPMS Reporting on SNM940
  public class VehicleBusTPMSReport : MachineEventBlockVehicleBusPayload
  {
    public static new readonly int kPacketID = 0x08;
    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }

    public byte ECMCount;
    public VehicleBusTPMSMessage[] TPMSMessages;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      ECMCount = (TPMSMessages == null) ? (byte)0 : (byte)TPMSMessages.Length;

      serializer(action, raw, ref bitPosition, 8, ref ECMCount);

      if (action == SerializationAction.Hydrate)
        TPMSMessages = new VehicleBusTPMSMessage[ECMCount];

      for (int i = 0; i < ECMCount; i++)
      {
        if (action == SerializationAction.Hydrate)
          TPMSMessages[i] = new VehicleBusTPMSMessage();

        TPMSMessages[i].Serialize(action, raw, ref bitPosition);
      }
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder("VehicleBusTPMSReport");
      builder.AppendFormat("\nPacketID:  {0}", PacketID.ToString());
      builder.AppendFormat("\nECMCount:  {0}", ECMCount.ToString());

      if (TPMSMessages != null)
      {
        builder.Append("\nTPMSMessages:");
        foreach(VehicleBusTPMSMessage TPMSMessage in TPMSMessages)
        {
          builder.AppendFormat("\n{0}", TPMSMessage.ToString());
        }
      }
      return builder.ToString();
    }
  }

  public class VehicleBusTPMSMessage : NestedMessage
  {
    public byte CANBusInstance;
    public byte ECMSourceAddress;
    public byte ECMDescriptionLength;
    public string ECMDescription;
    public byte TireCount;

    public TireReport[] TireDetails;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref CANBusInstance);
      serializer(action, raw, ref bitPosition, 8, ref ECMSourceAddress);
      serializer(action, raw, ref bitPosition, 8, ref ECMDescriptionLength);
      serializeASCIIFixedLengthString(action, raw, ref bitPosition, ECMDescriptionLength, ref ECMDescription);

      TireCount = (TireDetails == null) ? (byte)0 : (byte)TireDetails.Length;
      serializer(action, raw, ref bitPosition, 8, ref TireCount);

      if (action == SerializationAction.Hydrate)
        TireDetails = new TireReport[TireCount];

      for (int i = 0; i < TireCount; i++)
      {
        if (action == SerializationAction.Hydrate)
          TireDetails[i] = new TireReport();

        TireDetails[i].Serialize(action, raw, ref bitPosition);
      }
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder("VehicleBusTPMSMessage");
      builder.AppendFormat("\nCANBusInstance:  {0}", CANBusInstance.ToString());
      builder.AppendFormat("\nECMSourceAddress:  {0}", ECMSourceAddress.ToString());
      builder.AppendFormat("\nECMDescriptionLength:  {0}", ECMDescriptionLength.ToString());
      builder.AppendFormat("\nECMDescription:  {0}", ECMDescription.ToString());
      builder.AppendFormat("\nTireCount:  {0}", TireCount.ToString());

      if (TireDetails != null)
      {
        builder.Append("\nTireDetails:");
        foreach (TireReport tr in TireDetails)
        {
          builder.AppendFormat("\n{0}", tr.ToString());
        }
      }
      return builder.ToString();
    }
  }

  public class TireReport : NestedMessage
  {     
    private uint TirePressureRaw;
    private uint TireTemperatureRaw; 
        
    public ushort AlertStatus;   

    public int AxlePosition;
    public int TirePosition;    
    
    public double TirePressure
    {
      get { return TirePressureRaw * Constants.TirePressureMultiplier; }
      set { TirePressureRaw = (uint)(value / Constants.TirePressureMultiplier); }
    }

    public double TireTemperature
    {
      get { return (TireTemperatureRaw * Constants.TireTemperatureMultiplier) + Constants.TireTemperatureOffSet; }
      set { TireTemperatureRaw = (uint)((value - Constants.TireTemperatureOffSet )/ Constants.TireTemperatureMultiplier); }
    }       

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 4, ref TirePosition);
      serializer(action, raw, ref bitPosition, 4, ref AxlePosition);
      serializer(action, raw, ref bitPosition, 8, ref TirePressureRaw);
      serializer(action, raw, ref bitPosition, 16, ref TireTemperatureRaw);
      serializer(action, raw, ref bitPosition, 16, ref AlertStatus);     
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder("TireDetails");
      builder.AppendFormat("\nAxlePosition:  {0}", AxlePosition.ToString());
      builder.AppendFormat("\nTirePosition:  {0}", TirePosition.ToString());
      builder.AppendFormat("\nTirePressure:  {0}", TirePressure.ToString());
      builder.AppendFormat("\nTireTemperature:  {0}", TireTemperature.ToString());
      builder.AppendFormat("\nAlertStatus:  {0}", AlertStatus.ToString());
      
      return builder.ToString();
    }

    #region "Unconverted value properties for OEM Data Feed"
    public uint TirePressureUnconverted
    {
      get { return TirePressureRaw; }
    }

    public uint TireTemperatureUnconverted
    {
      get { return TireTemperatureRaw; }
    }
    #endregion
  }

  public class UnkownMachineEventVehicleBusData : MachineEventBlockVehicleBusPayload
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

  // For SNM940
  public class VehicleBusPayloadReport : MachineEventBlockVehicleBusPayload
  {
    public static new readonly int kPacketID = 0x09;
    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }

    public VehicleBusPayloadCycleCountECM[] PayloadCycleCountECMs;

    public byte? ECMCount
    {
      get
      {
        if (numberOfECMs == byte.MaxValue)
          return null;
        return numberOfECMs;
      }
      set
      {
        if (value.HasValue)
          numberOfECMs = (byte)value;
      }
    }

    private byte numberOfECMs;

    public byte ECMCountUnConverted
    {
      get { return numberOfECMs; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
  
      serializer(action, raw, ref bitPosition, 8, ref numberOfECMs);

      if (action == SerializationAction.Hydrate && (ECMCount.HasValue))
        PayloadCycleCountECMs = new VehicleBusPayloadCycleCountECM[numberOfECMs];

      if (ECMCount.HasValue)
      {
        for (int i = 0; i < numberOfECMs; i++)
        {
          if (action == SerializationAction.Hydrate)
            PayloadCycleCountECMs[i] = new VehicleBusPayloadCycleCountECM();

          PayloadCycleCountECMs[i].Serialize(action, raw, ref bitPosition);
        }
      }
    }
  }

  public class VehicleBusPayloadCycleCountECM : NestedMessage
  {
    public byte? CANBusInstance
    {
      get
      {
        if (canBusInstance == byte.MaxValue)
          return null;
        return canBusInstance;
      }
      set
      {
        if (value.HasValue)
          canBusInstance = (byte)value;
      }
    }

    public byte? ECMSourceAddress
    {
      get
      {
        if (ecmSourceAddress == byte.MaxValue)
          return null;
        return ecmSourceAddress;
      }
      set
      {
        if (value.HasValue)
          ecmSourceAddress = (byte)value;
      }
    }

    public uint? TotalPayload
    {
      get
      {
        if (totalPayload == uint.MaxValue)
          return null;
        return totalPayload;
      }
      set
      {
        if (value.HasValue)
          totalPayload = (uint)value;
      }
    }

    public uint? TotalCycles
    {
      get
      {
        if (totalCycles == uint.MaxValue)
          return null;
        return totalCycles;
      }
      set
      {
        if (value.HasValue)
          totalCycles = (uint)value;
      }
    }

    private byte canBusInstance;
    private byte ecmSourceAddress;
    private uint totalPayload;
    private uint totalCycles;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref canBusInstance);
      serializer(action, raw, ref bitPosition, 8, ref ecmSourceAddress);
      serializer(action, raw, ref bitPosition, 32,ref totalPayload);
      serializer(action, raw, ref bitPosition, 32, ref totalCycles);
    }

    #region "Unconverted value properties for OEM Data Feed"
    public byte CANBusInstanceUnConverted
    {
      get { return canBusInstance; }
    }

    public byte ECMSourceAddressUnConverted
    {
      get { return ecmSourceAddress; }
    }

    public uint TotalPayloadUnConverted
    {
      get { return totalPayload; }
    }

    public uint TotalCyclesUnConverted
    {
      get { return totalCycles; }
    }
    # endregion
  }
}
