using System;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class ReportingECM : NestedMessage // Version 1 and 2 
  { 
    public ushort ECMIdentifier;   
    
    public double FuelConsumption
    {
      get { return fuelConsumption/8.0; }
      set { fuelConsumption = (uint)(value*8.0); }
    }

    public byte FuelLevel;

    public TimeSpan TotalEngineIdleTime
    {
      get { return TimeSpan.FromHours((double) totalEngineIdleTime*0.05); }
      set { totalEngineIdleTime = (uint) (value.TotalHours/0.05); }
    }

    public double TotalMaximumFuelGallons
    {
      get { return totalMaximumFuel/8.0; }
      set { totalMaximumFuel = (uint)(value*8); }
    }

    public ushort NumberEngineStarts;

    public double TotalEngineRevolutions
    {
      get { return totalEngineRevolutions*4.0; }
      set { totalEngineRevolutions = (uint)(value/4.0); }
    }

    public double TotalIdleFuel
    {
      get { return totalIdleFuel/8.0; }
      set { totalIdleFuel = (uint)(value*8.0); }
    }

    public TimeSpan TotalMachineIdleTime
    {
      get { return TimeSpan.FromHours(totalMachineIdleTime*0.05); }
      set { totalMachineIdleTime = (uint) (value.TotalHours/0.05); }
    }

    public double TotalMachineIdleFuel
    {
      get { return totalMachineIdleFuel/8.0; }
      set { totalMachineIdleFuel = (uint)(value*8.0); }
    }
   
    private uint fuelConsumption;
    private uint totalEngineIdleTime;
    private uint totalMaximumFuel;
    private uint totalEngineRevolutions;
    private uint totalIdleFuel;
    private uint totalMachineIdleTime;
    private uint totalMachineIdleFuel;   

    public uint GetFuelConsumptionBeforeConversion()
    {
      return fuelConsumption;
    }

    public uint GetTotalEngineIdleTimeBeforeConversion()
    {
      return totalEngineIdleTime;
    }

    public uint GetTotalMaximumFuelBeforeConversion()
    {
      return totalMaximumFuel;
    }

    public uint GetTotalEngineRevolutionsBeforeConversion()
    {
      return totalEngineRevolutions;
    }

    public uint GetTotalIdleFuelBeforeConversion()
    {
      return totalIdleFuel;
    }

    public uint GetTotalMachineIdleTimeBeforeConversion()
    {
      return totalMachineIdleTime;
    }

    public uint GetTotalMachineIdleFuelBeforeConversion()
    {
      return totalMachineIdleFuel;
    }
    
    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref ECMIdentifier);     
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref fuelConsumption);
      serializer(action, raw, ref bitPosition, 8, ref FuelLevel);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalEngineIdleTime);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalMaximumFuel);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref NumberEngineStarts);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalEngineRevolutions);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalIdleFuel);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalMachineIdleTime);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalMachineIdleFuel);   
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

    public uint TotalMaximumFuelUnConverted
    {
      get { return totalMaximumFuel; }
    }

    public uint TotalEngineRevolutionsUnConverted
    {
      get { return totalEngineRevolutions; }
    }

    public uint TotalIdleFuelUnConverted
    {
      get { return totalIdleFuel; }
    }

    public uint TotalMachineIdleTimeUnConverted
    {
      get { return totalMachineIdleTime; }
    }

    public uint TotalMachineIdleFuelUnConverted
    {
      get { return totalMachineIdleFuel; }
    }
  #endregion
  }


  public class ReportingECMVersion3 : NestedMessage //Version3
  { 
    public ushort ECMIdentifier;

    public double? FuelConsumption
    {
      get 
      { 
        if (isFuelConsumptionAvailable)
        return fuelConsumption / 8.0;
      return null;
      }
      set 
      { 
        if (value.HasValue)
        fuelConsumption = (uint)(value.Value * 8.0); 
      }
    }

    public byte? FuelLevel
    {
      get
      {
        if (isFuelLevelAvailable)
          return fuelLevel;
        return null;
      }
      set
      {
        if (value.HasValue)
          fuelLevel = value.Value;
      }
    }

    public TimeSpan? TotalEngineIdleTime
    {
      get 
      {
        if (isTotalEngineIdleTimeAvailable)
          return TimeSpan.FromHours((double)totalEngineIdleTime * 0.05);
        return null;
      }
      set
      { 
        if (value.HasValue)
        totalEngineIdleTime = (uint)(value.Value.TotalHours/0.05);
      }
    }

    public double? TotalMaximumFuelGallons
    {
      get
      {
        if(isTotalMaximumFuelAvailable)
          return totalMaximumFuel / 8.0;
        return null;
      }
      set 
      { 
        if (value.HasValue)
        totalMaximumFuel = (uint)(value.Value * 8); 
      }
    }

    public ushort? NumberEngineStarts
    {
      get 
      {
        if (isNumberOfEngineStartsAvailable)
          return numberOfEngineStarts;
        return null;
      }
      set
      {
        if (value.HasValue)
          numberOfEngineStarts = value.Value;
      }
    }
      

    public double? TotalEngineRevolutions
    {
      get
      {
        if (isTotalEngineRevolutionsAvailable)
          return totalEngineRevolutions * 4.0;
        return null;

      }
      set 
      { 
        if (value.HasValue)
        totalEngineRevolutions = (uint)(value.Value / 4.0); 
      }
    }

    public double? TotalIdleFuel
    {
      get 
      { 
        if (isTotalIdleFuelAvailable)
          return totalIdleFuel / 8.0;
        return null;
      }
      set 
      {
        if (value.HasValue)
        totalIdleFuel = (uint)(value * 8.0);
      }
    }

    public TimeSpan? TotalMachineIdleTime
    {
      get 
      {
        if (isTotalMachineIdleTimeAvailable)
          return TimeSpan.FromHours(totalMachineIdleTime * 0.05);
        return null;
      }
      set 
      { 
        if (value.HasValue)
        totalMachineIdleTime = (uint)(value.Value.TotalHours / 0.05); 
      }
    }

    public double? TotalMachineIdleFuel
    {
      get 
      { 
        if (isTotalMachineIdleFuelAvailable)
          return totalMachineIdleFuel / 8.0;
        return null;
      }
      set
      { 
        if (value.HasValue)
        totalMachineIdleFuel = (uint)(value.Value * 8.0); 
      }
    }
    public double? AfterTreatmentHistoricalInfo
    {
      get 
      { 
        if (isAT1HIAvailable)
          return aT1HI / 8.0;
        return null;
      }
      set 
      {
        if (value.HasValue)
        aT1HI = (uint)(value.Value * 8.0); 
      }
    }
    public double? AfterTreatmentDieselExhaustInfo
    {
      get 
      {
        if (isAT1T1IAvailable)
          return aT1T1I / (double)0.004;
        return null;

      }
      set 
      {
        if (value.HasValue)
        aT1T1I = (byte)(value.Value * 0.004); 
      }
    }

    public bool isFuelConsumptionAvailable;
    private uint fuelConsumption;
    public bool isFuelLevelAvailable;
    private byte fuelLevel;
    public bool isTotalEngineIdleTimeAvailable;
    private uint totalEngineIdleTime;
    public bool isTotalMaximumFuelAvailable;
    private uint totalMaximumFuel;
    public bool isNumberOfEngineStartsAvailable;
    private ushort numberOfEngineStarts;
    public bool isTotalEngineRevolutionsAvailable;
    private uint totalEngineRevolutions;
    public bool isTotalIdleFuelAvailable;
    private uint totalIdleFuel;
    public bool isTotalMachineIdleTimeAvailable;
    private uint totalMachineIdleTime;
    public bool isTotalMachineIdleFuelAvailable;
    private uint totalMachineIdleFuel;
    public bool isAT1HIAvailable; // AT1HI: After treatment 1 Historical Information 
    private uint aT1HI;
    public bool isAT1T1IAvailable; //AT1T1I: After Treatment 1 Diesel Exhaust Fluid Tank 1 Information 
    private byte aT1T1I;

    public uint GetFuelConsumptionBeforeConversion()
    {
      return fuelConsumption;
    }

    public uint GetTotalEngineIdleTimeBeforeConversion()
    {
      return totalEngineIdleTime;
    }

    public uint GetTotalMaximumFuelBeforeConversion()
    {
      return totalMaximumFuel;
    }

    public uint GetTotalEngineRevolutionsBeforeConversion()
    {
      return totalEngineRevolutions;
    }

    public uint GetTotalIdleFuelBeforeConversion()
    {
      return totalIdleFuel;
    }

    public uint GetTotalMachineIdleTimeBeforeConversion()
    {
      return totalMachineIdleTime;
    }

    public uint GetTotalMachineIdleFuelBeforeConversion()
    {
      return totalMachineIdleFuel;
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref ECMIdentifier);         
        filler(ref bitPosition, 8); // Big-Endian- take last 11 bits value
        serializer(action, raw, ref bitPosition, 1, ref isTotalMachineIdleFuelAvailable);
        serializer(action, raw, ref bitPosition, 1, ref isAT1HIAvailable);
        serializer(action, raw, ref bitPosition, 1, ref isAT1T1IAvailable); 
        
        filler(ref bitPosition, 5);
        serializer(action, raw, ref bitPosition, 1, ref isFuelConsumptionAvailable);
        serializer(action, raw, ref bitPosition, 1, ref isFuelLevelAvailable);
        serializer(action, raw, ref bitPosition, 1, ref isTotalEngineIdleTimeAvailable);
        serializer(action, raw, ref bitPosition, 1, ref isTotalMaximumFuelAvailable);
        serializer(action, raw, ref bitPosition, 1, ref isNumberOfEngineStartsAvailable);
        serializer(action, raw, ref bitPosition, 1, ref isTotalEngineRevolutionsAvailable);        
        serializer(action, raw, ref bitPosition, 1, ref isTotalIdleFuelAvailable);
        serializer(action, raw, ref bitPosition, 1, ref isTotalMachineIdleTimeAvailable);        

        if (isFuelConsumptionAvailable)
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref fuelConsumption);
        if (isFuelLevelAvailable)
          serializer(action, raw, ref bitPosition, 8, ref fuelLevel);
        if (isTotalEngineIdleTimeAvailable)
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalEngineIdleTime);
        if (isTotalMaximumFuelAvailable)
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalMaximumFuel);
        if (isNumberOfEngineStartsAvailable)
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref numberOfEngineStarts);
        if (isTotalEngineRevolutionsAvailable)
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalEngineRevolutions);
        if (isTotalIdleFuelAvailable)
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalIdleFuel);
        if (isTotalMachineIdleTimeAvailable)
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalMachineIdleTime);
        if (isTotalMachineIdleFuelAvailable)
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalMachineIdleFuel);
        if (isAT1HIAvailable)
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref aT1HI);
        if (isAT1T1IAvailable)
          serializer(action, raw, ref bitPosition, 8, ref aT1T1I);       
    }

    #region "Unconverted value properties for OEM Data Feed"

    public uint? FuelConsumptionUnConverted
    {
      get 
      {
        if (isFuelConsumptionAvailable)
          return fuelConsumption;
        return null;
      }
    }

    public uint? TotalEngineIdleTimeUnConverted
    {
      get 
      {
        if (isTotalEngineIdleTimeAvailable)
          return totalEngineIdleTime;
        return null;
      }
    }

    public uint? TotalMaximumFuelUnConverted
    {
      get 
      {
        if (isTotalMaximumFuelAvailable)
          return totalMaximumFuel;
        return null;
      }
    }

    public uint? TotalEngineRevolutionsUnConverted
    {
      get 
      { 
        if (isTotalEngineRevolutionsAvailable)
          return totalEngineRevolutions;
        return null;
      }
    }

    public uint? TotalIdleFuelUnConverted
    {
      get 
      {
        if (isTotalIdleFuelAvailable)
          return totalIdleFuel;
        return null;
      }
    }

    public uint? TotalMachineIdleTimeUnConverted
    {
      get 
      { 
        if (isTotalMachineIdleTimeAvailable)
          return totalMachineIdleTime;
        return null;
      }
    }

    public uint? TotalMachineIdleFuelUnConverted
    {
      get 
      { 
        if (isTotalMachineIdleFuelAvailable)
          return totalMachineIdleFuel;
        return null;
      }
    }

    public uint? AfterTreatmentHistoricalInfoUnconverted
    {
      get 
      { 
        if (isAT1HIAvailable)
          return aT1HI;
        return null;
      }
    }

    public byte? AfterTreatmentDieselExhaustInfoUnconverted
    {
      get 
      { 
        if (isAT1T1IAvailable)
          return aT1T1I;
        return null;
      }
    }
    #endregion
  }

  public class PayloadCycleCountECM : NestedMessage
  {
    public ushort ECMIdentifier;

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
    
    private uint totalPayload;
    private uint totalCycles;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref ECMIdentifier);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalPayload);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalCycles);
    }

    #region "Unconverted value properties for OEM Data Feed"

    public uint TotalPayloadUnConverted
    {
      get { return totalPayload; }
    }

    public uint TotalCyclesUnConverted
    {
      get { return totalCycles; }
    }

    #endregion
  }

  public class DeviceIDData : NestedMessage
  {
    public DataLinkType ECMType
    {
      get { return (DataLinkType)ecmType; }
      set { ecmType = (byte)value; }
    }

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

    public bool ActingMasterECM;
    public bool SyncronizedSMUClockStrategySupported;
    public byte EventProtocolVersion;
    public byte DiagnosticProtocolVersion;
    public ushort ModuleID1;
    public ushort Module1ServiceToolSupportChangeLevel;
    public ushort Module1ApplicationLevel;
    public ushort ModuleID2;
    public ushort Module2ServiceToolSupportChangeLevel;
    public ushort Module2ApplicationLevel;
    public ushort ECMSourceAddress;
    public string ECMSoftwarePartNumber;
    public string ECMSerialNumber;
    public string ECMHardwarePartNumber;
    public bool ArbitraryAddressCapable;
    public byte IndustryGroup;
    public byte VehicleSystemInstance;
    public byte VehicleSystem;
    public byte Function;
    public byte FunctionInstance;
    public byte ECUInstance;
    public ushort ManufacturerCode;
    public int IdentityNumber;

    private byte ecmType;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      uint transactionVersionBitPosition = 272;
      byte transactionVersion = 0x01;
      serializer(action, raw, ref transactionVersionBitPosition, 8, ref transactionVersion);
      serializer(action, raw, ref bitPosition, 1, ref DiagnosticProtocolVersion);
      serializer(action, raw, ref bitPosition, 1, ref EventProtocolVersion);
      serializer(action, raw, ref bitPosition, 1, ref SyncronizedSMUClockStrategySupported);
      serializer(action, raw, ref bitPosition, 1, ref ActingMasterECM);

      if (transactionVersion == 1)
      {
        serializer(action, raw, ref bitPosition, 2, ref ecmType);
        filler(ref bitPosition, 2);
      }
      else
      {
        serializer(action, raw, ref bitPosition, 3, ref ecmType);
        filler(ref bitPosition, 1);
      }

      if ((transactionVersion == 2 && ECMType != DataLinkType.SAEJI939 && ECMType != DataLinkType.Unknown) || (transactionVersion == 1 && ECMType != DataLinkType.Unknown))
      { 
      serializer(action, raw, ref bitPosition, 16, ref ModuleID1);
      serializer(action, raw, ref bitPosition, 16, ref Module1ServiceToolSupportChangeLevel);
      serializer(action, raw, ref bitPosition, 16, ref Module1ApplicationLevel);
      }

      if (ECMType == DataLinkType.CDLAndJ1939 || ECMType == DataLinkType.All)
      {
        serializer(action, raw, ref bitPosition, 16, ref ModuleID2);
        serializer(action, raw, ref bitPosition, 16, ref Module2ServiceToolSupportChangeLevel);
        serializer(action, raw, ref bitPosition, 16, ref Module2ApplicationLevel);
      }

      if ((ECMType != DataLinkType.CDL && ECMType != DataLinkType.Unknown ) && transactionVersion == 2)
        serializer(action, raw, ref bitPosition, 8, ref ECMSourceAddress);

      if ((ECMType != DataLinkType.CDL && ECMType != DataLinkType.Unknown) && transactionVersion == 2)
      {
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

      serializeFixedLengthString(action, raw, ref bitPosition, 10, ref ECMSoftwarePartNumber);
      serializeFixedLengthString(action, raw, ref bitPosition, 10, ref ECMSerialNumber);

      if (transactionVersion == 2)
        serializeFixedLengthString(action, raw, ref bitPosition, 10, ref ECMHardwarePartNumber);
    }

    public enum DataLinkType
    {
      Unknown = 0,
      CDL = 1,
      J1939 = 2,
      CDLAndJ1939 = 3,
      SAEJI939 = 4,
      SAEJI939AndCDL = 5,
      SAEJI939AndJ1939 = 6,
      All = 7
    }
  }

  public class DigitalInputsAdminInformation : NestedMessage
  {
    public byte TransactionVersion;
    public InputConfig? inputConfig1 = null;
    public InputConfig? inputConfig2 = null;
    public InputConfig? inputConfig3 = null;
    public InputConfig? inputConfig4 = null;
    public DigitalInputMonitoringConditions? Input1MonitoringCondition = null;
    public DigitalInputMonitoringConditions? Input2MonitoringCondition = null;
    public DigitalInputMonitoringConditions? Input3MonitoringCondition = null;
    public DigitalInputMonitoringConditions? Input4MonitoringCondition = null;
    public string Input1Description = null;
    public string Input2Description = null;
    public string Input3Description = null;
    public string Input4Description = null;

    public bool IsDigitalInput1Configured
    {
      get { return ParseFlagFromByte(digitalInputCounter, 0); }
      set { GetByteFromFlag(0, value, ref digitalInputCounter); }
    }
    public bool IsDigitalInput2Configured
    {
      get { return ParseFlagFromByte(digitalInputCounter, 1); }
      set { GetByteFromFlag(1, value, ref digitalInputCounter); }
    }
    public bool IsDigitalInput3Configured
    {
      get { return ParseFlagFromByte(digitalInputCounter, 2); }
      set { GetByteFromFlag(2, value, ref digitalInputCounter); }
    }
    public bool IsDigitalInput4Configured
    {
      get { return ParseFlagFromByte(digitalInputCounter, 3); }
      set { GetByteFromFlag(3, value, ref digitalInputCounter); }
    }
    public TimeSpan? Input1DelayTime
    {
      get
      {
        if (input1DelayTime.HasValue)
          return TimeSpan.FromMilliseconds(input1DelayTime.Value * 100.0);

        return null;
      }
      set
      {
        if (value.HasValue)
          input1DelayTime = (ushort)(value.Value.TotalMilliseconds / 100.0);
        else
          input1DelayTime = null;
      }
    }
    public TimeSpan? Input2DelayTime
    {
      get
      {
        if (input2DelayTime.HasValue)
          return TimeSpan.FromMilliseconds(input2DelayTime.Value * 100.0);

        return null;
      }
      set
      {
        if (value.HasValue)
          input2DelayTime = (ushort)(value.Value.TotalMilliseconds / 100.0);
        else
          input2DelayTime = null;
      }
    }
    public TimeSpan? Input3DelayTime
    {
      get
      {
        if (input3DelayTime.HasValue)
          return TimeSpan.FromMilliseconds(input3DelayTime.Value * 100.0);

        return null;
      }
      set
      {
        if (value.HasValue)
          input3DelayTime = (ushort)(value.Value.TotalMilliseconds / 100.0);
        else
          input3DelayTime = null;
      }
    }

    public TimeSpan? Input4DelayTime
    {
      get
      {
        if (input4DelayTime.HasValue)
          return TimeSpan.FromMilliseconds(input4DelayTime.Value * 100.0);

        return null;
      }
      set
      {
        if (value.HasValue)
          input4DelayTime = (ushort)(value.Value.TotalMilliseconds / 100.0);
        else
          input4DelayTime = null;
      }
    }

    private byte digitalInputCounter;
    private ushort? input1DelayTime = null;
    private ushort? input2DelayTime = null;
    private ushort? input3DelayTime = null;
    private ushort? input4DelayTime = null;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      serializer(action, raw, ref bitPosition, 8, ref digitalInputCounter);
      if (IsDigitalInput1Configured)
      {
        byte inputConfig = 0;
        serializer(action, raw, ref bitPosition, 8, ref inputConfig);
        inputConfig1 = (InputConfig)inputConfig;
        ushort delayTime = 0;
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref delayTime);
        input1DelayTime = delayTime;
        ushort monitoringCondition = 0;
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref monitoringCondition);
        Input1MonitoringCondition = (DigitalInputMonitoringConditions)monitoringCondition;
        serializeFixedLengthString(action, raw, ref bitPosition, 24, ref Input1Description);
      }
      if (IsDigitalInput2Configured)
      {
        byte inputConfig = 0;
        serializer(action, raw, ref bitPosition, 8, ref inputConfig);
        inputConfig2 = (InputConfig)inputConfig;
        ushort delayTime = 0;
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref delayTime);
        input2DelayTime = delayTime;
        ushort monitoringCondition = 0;
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref monitoringCondition);
        Input2MonitoringCondition = (DigitalInputMonitoringConditions)monitoringCondition;
        serializeFixedLengthString(action, raw, ref bitPosition, 24, ref Input2Description);
      }
      if (IsDigitalInput3Configured)
      {
        byte inputConfig = 0;
        serializer(action, raw, ref bitPosition, 8, ref inputConfig);
        inputConfig3 = (InputConfig)inputConfig;
        ushort delayTime = 0;
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref delayTime);
        input3DelayTime = delayTime;
        ushort monitoringCondition = 0;
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref monitoringCondition);
        Input3MonitoringCondition = (DigitalInputMonitoringConditions)monitoringCondition;
        serializeFixedLengthString(action, raw, ref bitPosition, 24, ref Input3Description);
      }
      if (IsDigitalInput4Configured)
      {
        byte inputConfig = 0;
        serializer(action, raw, ref bitPosition, 8, ref inputConfig);
        inputConfig4 = (InputConfig)inputConfig;
        ushort delayTime = 0;
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref delayTime);
        input4DelayTime = delayTime;
        ushort monitoringCondition = 0;
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref monitoringCondition);
        Input4MonitoringCondition = (DigitalInputMonitoringConditions)monitoringCondition;
        serializeFixedLengthString(action, raw, ref bitPosition, 24, ref Input4Description);
      }
    }

    private bool ParseFlagFromByte(byte statusByte, int bitPosition)
    {
      return (((statusByte >> bitPosition) & 0x01) == 0x01) ? true : false;
    }
    private void GetByteFromFlag(int bitPosition, bool updateValue, ref byte value)
    {
      value |= (byte)((updateValue == true) ? (0x01 << bitPosition) : 0x00);
    }

    #region "Unconverted value properties for OEM Data Feed"
    public ushort? Input1DelayTimeUnConverted
    {
      get { return input1DelayTime; }
    }
    public ushort? Input2DelayTimeUnConverted
    {
      get { return input2DelayTime; }
    }
    public ushort? Input3DelayTimeUnConverted
    {
      get { return input3DelayTime; }
    }
    public ushort? Input4DelayTimeUnConverted
    {
      get { return input4DelayTime; }
    }
    #endregion
  }

  public class MaintenanceAdministrationInformation : NestedMessage
  {
    public byte TransactionVersion;
    public bool MaintenanceModeEnabled;
    public TimeSpan MaintenanceModeDuration
    {
      get { return TimeSpan.FromHours(maintenanceModeDurationHours); }
      set { maintenanceModeDurationHours = (byte)value.TotalHours; }
    }

    private byte maintenanceModeDurationHours;
    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      serializer(action, raw, ref bitPosition, 8, ref MaintenanceModeEnabled);
      serializer(action, raw, ref bitPosition, 8, ref maintenanceModeDurationHours);
    }

    public MTSConfigData.MaintenanceModeConfig GetMaintModeConfig(MessageStatusEnum status, long messageID, DateTime receivedUTC)
    {
      MTSConfigData.MaintenanceModeConfig maintModeConfig = new MTSConfigData.MaintenanceModeConfig();
      maintModeConfig.MessageSourceID = messageID;
      maintModeConfig.SentUTC = null;
      maintModeConfig.Status = status;
      maintModeConfig.IsEnabled = MaintenanceModeEnabled;
      maintModeConfig.Duration = MaintenanceModeDuration;
      maintModeConfig.SentUTC = receivedUTC;

      return maintModeConfig;
    }
  }

  public class AdministrationFailedDelivery : NestedMessage
  {
    public byte TransactionVersion;
    public byte MessageSequenceID;
    public FailureReason Reason
    {
      get { return (FailureReason)failureReason; }
      set { failureReason = (byte)value; }
    }

    private byte failureReason;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      serializer(action, raw, ref bitPosition, 8, ref MessageSequenceID);
      serializer(action, raw, ref bitPosition, 8, ref failureReason);
    }

    public enum FailureReason
    {
      UnrecognizableParameter = 0x01,
      IncorrectParameterValue = 0x02,
    }
  }

  public class TamperSecurityAdministrationInformationMessage : NestedMessage
  {
    public byte TransactionVersion;

    public MachineStartStatus StartStatus
    {
      get { return (MachineStartStatus)(sbyte)machineStartStatus; }
      set { machineStartStatus = (byte)value; }
    }

    private byte machineStartStatus;

    public MachineStartModeConfigurationSource MachineStartModeConfiguration
    {
      get { return (MachineStartModeConfigurationSource)machineStartModeConfigurationSource; }
      set { machineStartModeConfigurationSource = (byte)value; }
    }

    private byte machineStartModeConfigurationSource;
    

    public TamperResistanceStatus ResistanceStatus
    {
      get { return (TamperResistanceStatus)(sbyte)tamperResistanceStatus; }
        set { tamperResistanceStatus = (byte)value; }
    }

    private byte tamperResistanceStatus;

    public TamperResistanceModeConfigurationSource TamperResistanceModeConfiguration
    {
      get { return (TamperResistanceModeConfigurationSource)tamperResistanceModeConfigurationSource; }
      set { tamperResistanceModeConfigurationSource = (byte)value; }
    }

    private byte tamperResistanceModeConfigurationSource;

    public MachineSecurityMode MachineSecurityMode
    {
      get { return (MachineSecurityMode)machineSecurityMode; }
      set { machineSecurityMode = (byte)value; }
    }

    private byte machineSecurityMode;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
        serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
        serializer(action, raw, ref bitPosition, 8, ref machineStartStatus);
        serializer(action, raw, ref bitPosition, 8, ref machineStartModeConfigurationSource);
        serializer(action, raw, ref bitPosition, 8, ref tamperResistanceStatus);
        serializer(action, raw, ref bitPosition, 8, ref tamperResistanceModeConfigurationSource);
        serializer(action, raw, ref bitPosition, 8, ref machineSecurityMode);
    }
  }

  public class TamperSecurityStatusInformationMessage : NestedMessage
  {
    public byte TransactionVersion;

    public MachineStartStatus StartStatus
    {
      get { return (MachineStartStatus)(sbyte)machineStartStauts; }
      set { machineStartStauts = (byte)value; }
    }

    private byte machineStartStauts;

    public MachineStartStatusTrigger StartStatusTrigger
    {
      get { return (MachineStartStatusTrigger)machineStartStatusTrigger; }
      set { machineStartStatusTrigger = (byte)value; }
    }

    private byte machineStartStatusTrigger;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      serializer(action, raw, ref bitPosition, 8, ref machineStartStauts);
      serializer(action, raw, ref bitPosition, 8, ref machineStartStatusTrigger);
    }
  }

  public class IdleStartStopReport : NestedMessage
  {
    public byte TransactionVersion;
    public bool IdleStop;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      serializer(action, raw, ref bitPosition, 8, ref IdleStop);
    }
  }

  public class MSSKeyIDReport : NestedMessage
  {
    public byte TransactionVersion;
    public long MSSKeyID;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      BigEndianSerializer(action, raw, ref bitPosition, 8, ref MSSKeyID);
    }
  }

  public class FuelEngineReport : NestedMessage
  {
    public byte TransactionVersion;
    public ReportingECMVersion3[] ReportingECMsVersion3;
    public ReportingECM[] ReportingECMs; 

   

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);

      byte count;


        if (TransactionVersion == 3)
        {
          count = (ReportingECMsVersion3 == null) ? (byte)0 : (byte)ReportingECMsVersion3.Length;
          serializer(action, raw, ref bitPosition, 8, ref count);

          if (action == SerializationAction.Hydrate)          
            ReportingECMsVersion3 = new ReportingECMVersion3[count];
          
          ReportingECMsVersion3 = (ReportingECMVersion3[])serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 0, ReportingECMsVersion3, typeof(ReportingECMVersion3));
        }
        else
        {
          count = (ReportingECMs == null) ? (byte)0 : (byte)ReportingECMs.Length;
          serializer(action, raw, ref bitPosition, 8, ref count);

          if (action == SerializationAction.Hydrate)
            ReportingECMs = new ReportingECM[count];
          ReportingECMs = (ReportingECM[])serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 0, ReportingECMs, typeof(ReportingECM));
        }
    }
  }

  public class PayloadAndCycleCountReport : NestedMessage
  {
    public byte TransactionVersion;
    public PayloadCycleCountECM[] PayloadCycleCountECMs;

    public byte? NumberOfECMs
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

    public byte NumberOfECMsUnConverted
    {
      get { return numberOfECMs; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);

      serializer(action, raw, ref bitPosition, 8, ref numberOfECMs);

      if ((action == SerializationAction.Hydrate) && (NumberOfECMs.HasValue))
      {
        PayloadCycleCountECMs = new PayloadCycleCountECM[numberOfECMs];
      }

      if (NumberOfECMs.HasValue)
      {
        PayloadCycleCountECMs =
          (PayloadCycleCountECM[])
          serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 0, PayloadCycleCountECMs,
                                             typeof (PayloadCycleCountECM));
      }
    }
  }

  # region TMS Message
  public class TMSInformationMessage : NestedMessage
  {
    public byte TransactionVersion;
    public ushort InstallationStatus;
    public ushort MID;
    public ushort RecordsCount;
    public TMSInfo[] tmsInfo = null;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);      
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref InstallationStatus);     
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref MID);      
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref RecordsCount);
      
      if (action == SerializationAction.Hydrate) 
      tmsInfo = new TMSInfo[RecordsCount];
      

      tmsInfo = (TMSInfo[])serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 0, tmsInfo, typeof(TMSInfo));
    }
  }

    public class TMSInfo : NestedMessage
    {
      public byte AxlePosition;
      public byte TirePosition;
      public string SensorID;

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        base.Serialize(action, raw, ref bitPosition);
        serializer(action, raw, ref bitPosition, 8, ref AxlePosition);
        serializer(action, raw, ref bitPosition, 8, ref TirePosition);
        serializeFixedLengthString(action, raw, ref bitPosition, 25, ref SensorID);
      }
    }

    public class TMSReportMessage : NestedMessage
    {
      public byte TransactionVersion;
      public ushort RecordCount;
      public TMSReport[] tmsReport = null;

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {       

        base.Serialize(action, raw, ref bitPosition);
        serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);        
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref RecordCount);

        if (action == SerializationAction.Hydrate) 
        tmsReport = new TMSReport[RecordCount];

        tmsReport = (TMSReport[])serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 0, tmsReport, typeof(TMSReport));

      }
    }

    public class TMSReport : NestedMessage
    {
      public double Pressure
      {
        get { return (pressureRaw * 0.014504); }
        set { pressureRaw = (ushort)(value / 0.014504); }        
      }

      public short Temperature
      {
        get { return unchecked ((short) temperatureRaw); }
        set { temperatureRaw = unchecked((ushort) value);}
      }
      
      public byte AxlePosition;
      public byte TirePosition;
      public ushort pressureRaw;
      public ushort temperatureRaw;
      public ushort TemperatureIndicator;
      public ushort AlertStatus;
      public ushort SensorInstallationStatus;
   

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        base.Serialize(action, raw, ref bitPosition);
        serializer(action, raw, ref bitPosition, 8, ref AxlePosition);
        serializer(action, raw, ref bitPosition, 8, ref TirePosition);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref pressureRaw);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref TemperatureIndicator);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref temperatureRaw);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref AlertStatus);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref SensorInstallationStatus);
      }

      public double GetPressurevalueBeforeConversion()
      {
        return pressureRaw; 
      }

      public ushort PressureUnconverted
      {
        get { return pressureRaw; }
      }

    }
# endregion
}
