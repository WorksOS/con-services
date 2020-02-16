using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using VSS.Hosted.VLCommon;
using Microsoft.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class BitConfigurationTrackerMessage : TrackerMessage,
                                                TrackerMessage.IDeviceSequenceID,
                                                INeedsAcknowledgement
  {
    public static new readonly int kPacketID = 0x06;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref DevicePacketSequenceIDRaw);
      serializer(action, raw, ref bitPosition, 24, ref EventTimeUtcSeconds.Seconds);

      if (action == SerializationAction.Hydrate)
      {
        // Here's a wonderful hack to work around the fact that forever-plus-a-day CDPD devices have sent
        // daily BIT messages claiming 5 nested blocks when they indeed have 4.  This has been a known bug
        // since forever, but nobody fixes it not wanting to incur the airtime to send out the correction
        // (clue: fix it, leave it in the sources, and let it go out the next time you send out the firmware
        // for other reasons).  Other problem, some versions of firmware incorrectly report 4 nested blocks
        // when there are 5, so we miss one.

        // The hack is to parse nested BIT blocks until we've consumed the entire payload.

        ArrayList bitBlocks = new ArrayList();

        int reportedBlockCount = 0;  // We don't believe it, but use it to report hydration problems.

        serializer(action, raw, ref bitPosition, 4, ref reportedBlockCount);

        uint endingBitPosition = bitPosition + (8 * bytesLeftInMessage(bitPosition));
        bool isUnknown = false;
        try
        {
          while (bitPosition < endingBitPosition)
          {
            PlatformMessage bitBlock = hydratePlatformMessage(raw, ref bitPosition, false,
               MessageCategory.BitConfigurationBlockPayload, bytesLeftInMessage(bitPosition));

            if (bitBlock is UnknownPlatformMessage)
            {
              UnknownBITConfiguration unkn = new UnknownBITConfiguration();
              unkn.Data = ((UnknownPlatformMessage)bitBlock).UnknownMessageData;
              isUnknown = true;
              bitBlocks.Add(unkn);
            }
            else
              bitBlocks.Add(bitBlock as BitConfigurationMessage);

          }
        }
        catch (Exception)
        {

          hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;
        }

        BitConfigurationBlocks = (BitConfigurationMessage[])bitBlocks.ToArray(typeof(BitConfigurationMessage));

        if (bitBlocks.Count != reportedBlockCount && !isUnknown)
        {
          hydrationErrors |= MessageHydrationErrors.EmbeddedBitCountIncorrect;
        }

        rebuildRemapper();
      }
      else
      {
        BitConfigurationBlocks = (BitConfigurationMessage[])
           serializeHeterogeneousRunLengthArray(action, raw, ref bitPosition, 4,
              typeof(BitConfigurationMessage), BitConfigurationBlocks,
              MessageCategory.BitConfigurationBlockPayload);
      }
    }

    private byte DevicePacketSequenceIDRaw;
    public UtcSeconds EventTimeUtcSeconds;

    public BitConfigurationMessage[] BitConfigurationBlocks;

    protected int[] blockRemapper;

    public BitConfigurationTrackerMessage()
    {
      initRemapper();
    }

    public enum BitConfigurationID
    {
      NetworkConfiguration = 0x00,
      EnvironmentConfiguration = 0x01,
      NavigationConfiguration = 0x02,
      SoftwareConfiguration = 0x03,
      ReadyMixConfiguration = 0x04,
      Configuration = 0x05,
      NetworkInterfaceConfiguration = 0x06,
      NetworkHardwareConfiguration = 0x07, // 0x07 Reserved (See the CDPD Wireless Interface ICD[4])
      ReadyMixConfiguration2 = 0x08, // 0x8	Reserved (Ready Mix Data II TBD)
      SecurityApplicationConfiguration = 0x09,
      Configuration2 = 0x0A,
      DriverIDConfiguration = 0x0B,
      DriverTimeMgrConfiguration = 0x0C,
      NetworkHardware2Configuration = 0x0D,
      ApplicationLifetimeCounter = 0x0E,
      SbcConfiguration = 0x0F,
      JBusConfiguration = 0x10,
      JBusValueMonitoring = 0x11,
      IPFilteringConfiguration = 0x13,
      DevicePortConfiguration = 0x14,
      SlumperHealthReport = 0x15,
      FirmwareUpdateStatus = 0x1B,
      MTSNetwork = 0x20,
      MTSEnvironment = 0x21,
      MTSNavigation = 0x22,
      MTSConfiguration = 0x23,
      DataUsage = 0x24,
      UnknownBIT = 0x81,
      BitConfigurationBlockCount                // Represents the number of block types (assumes the enumeration is in order!)
    }

    protected void initRemapper()
    {
      blockRemapper = new int[(int)BitConfigurationID.BitConfigurationBlockCount];

      // Set all remappings to -1 to indicate that there is no destination block yet.

      for (int i = blockRemapper.Length - 1; i >= 0; i--)
      {
        blockRemapper[i] = -1;
      }
    }

    protected void rebuildRemapper()
    {
      initRemapper();

      for (int i = 0; i < BitConfigurationBlocks.Length; i++)
      {
        if (BitConfigurationBlocks[i] == null)
        {
          // This BIT block wasn't hydrated properly; odds are it is that persistent bug in the CDPD
          // firmware that misreports the number of blocks it contains.  Actually, it exists in a
          // different way in the GPRS firmware as well.

          hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;
        }
        else
        {
          blockRemapper[BitConfigurationBlocks[i].PacketID] = i;
        }
      }
    }

    protected BitConfigurationMessage getConfigurationBlock(BitConfigurationID ID, Type targetType)
    {
      if (blockRemapper[(int)ID] == -1)
      {
        int newLength = (BitConfigurationBlocks == null) ? 1 : BitConfigurationBlocks.Length + 1;

        BitConfigurationMessage[] newArray = new BitConfigurationMessage[newLength];

        if (BitConfigurationBlocks != null)
        {
          Array.Copy(BitConfigurationBlocks, newArray, newLength - 1);
        }

        blockRemapper[(int)ID] = newLength - 1;
      }

      return BitConfigurationBlocks[blockRemapper[(int)ID]];
    }

    #region Accessors for specific block types

    public bool NetworkConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.NetworkConfiguration] != -1; }
    }

    public NetworkConfigurationBlock GetOrCreateNetworkConfiguration()
    {
      return (NetworkConfigurationBlock)getConfigurationBlock(BitConfigurationID.NetworkConfiguration,
                                                               typeof(NetworkConfigurationBlock));
    }


    public bool EnvironmentConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.EnvironmentConfiguration] != -1; }
    }

    public EnvironmentConfigurationBlock GetOrCreateEnvironmentConfiguration()
    {
      return (EnvironmentConfigurationBlock)getConfigurationBlock(BitConfigurationID.EnvironmentConfiguration,
                                                                   typeof(EnvironmentConfigurationBlock));
    }

    public bool NavigationConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.NavigationConfiguration] != -1; }
    }

    public NavigationConfigurationBlock GetOrCreateNavigationConfiguration()
    {
      return (NavigationConfigurationBlock)getConfigurationBlock(BitConfigurationID.NavigationConfiguration,
                                                                  typeof(NavigationConfigurationBlock));
    }

    public bool SoftwareConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.SoftwareConfiguration] != -1; }
    }

    public SoftwareConfigurationBlock GetOrCreateSoftwareConfiguration()
    {
      return (SoftwareConfigurationBlock)getConfigurationBlock(BitConfigurationID.SoftwareConfiguration,
                                                                typeof(SoftwareConfigurationBlock));
    }

    public bool ReadyMixConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.ReadyMixConfiguration] != -1; }
    }

    public ReadyMixConfigurationBlock GetOrCreateReadyMixConfiguration()
    {
      return (ReadyMixConfigurationBlock)getConfigurationBlock(BitConfigurationID.ReadyMixConfiguration,
                                                                typeof(ReadyMixConfigurationBlock));
    }

    public bool ConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.Configuration] != -1; }
    }

    public ConfigurationBlock GetOrCreateConfiguration()
    {
      return (ConfigurationBlock)getConfigurationBlock(BitConfigurationID.Configuration,
                                                        typeof(ConfigurationBlock));
    }

    public bool NetworkInterfaceConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.NetworkInterfaceConfiguration] != -1; }
    }

    public NetworkInterfaceConfigurationBlock GetOrCreateNetworkInterfaceConfiguration()
    {
      return (NetworkInterfaceConfigurationBlock)getConfigurationBlock(BitConfigurationID.NetworkInterfaceConfiguration,
                                                                        typeof(NetworkInterfaceConfigurationBlock));
    }

    //      public bool NetworkHardwareConfigurationExists
    //      {
    //         get { return blockRemapper[(int) BitConfigurationID.NetworkHardwareConfiguration] != -1; }
    //      }
    //
    //      public NetworkHardwareConfigurationBlock GetOrCreateNetworkHardwareConfiguration()
    //      {
    //         return (NetworkHardwareConfigurationBlock) getConfigurationBlock(BitConfigurationID.NetworkHardwareConfiguration,
    //                                                                          typeof(NetworkHardwareConfigurationBlock));
    //      }

    public bool ReadyMixConfiguration2Exists
    {
      get { return blockRemapper[(int)BitConfigurationID.ReadyMixConfiguration2] != -1; }
    }

    public ReadyMixConfiguration2Block GetOrCreateReadyMixConfiguration2()
    {
      return (ReadyMixConfiguration2Block)getConfigurationBlock(BitConfigurationID.ReadyMixConfiguration2,
                                                                 typeof(ReadyMixConfiguration2Block));
    }

    public bool SecurityApplicationConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.SecurityApplicationConfiguration] != -1; }
    }

    public SecurityApplicationConfigurationBlock GetOrCreateSecurityApplicationConfiguration()
    {
      return (SecurityApplicationConfigurationBlock)getConfigurationBlock(BitConfigurationID.SecurityApplicationConfiguration,
                                                                           typeof(SecurityApplicationConfigurationBlock));
    }

    public bool Configuration2Exists
    {
      get { return blockRemapper[(int)BitConfigurationID.Configuration2] != -1; }
    }

    public Configuration2Block GetOrCreateConfiguration2()
    {
      return (Configuration2Block)getConfigurationBlock(BitConfigurationID.Configuration2,
         typeof(Configuration2Block));
    }

    public bool DriverIDConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.DriverIDConfiguration] != -1; }
    }

    public DriverIDConfigurationBlock GetOrCreateDriverIDConfiguration()
    {
      return (DriverIDConfigurationBlock)getConfigurationBlock(BitConfigurationID.DriverIDConfiguration,
                                                                typeof(DriverIDConfigurationBlock));
    }

    public bool DriverTimeMgrConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.DriverTimeMgrConfiguration] != -1; }
    }

    public DriverTimeMgrConfigurationBlock GetOrCreateDriverTimeMgrConfiguration()
    {
      return (DriverTimeMgrConfigurationBlock)getConfigurationBlock(BitConfigurationID.DriverTimeMgrConfiguration,
                                                                     typeof(DriverTimeMgrConfigurationBlock));
    }

    public IPFilterConfigurationBlock GetOrCreateIPFilterConfigurationBlock()
    {
      return (IPFilterConfigurationBlock)getConfigurationBlock(BitConfigurationID.IPFilteringConfiguration,
         typeof(IPFilterConfigurationBlock));
    }

    public bool DevicePortConfigurationExists
    {
      get { return blockRemapper[(int)BitConfigurationID.DevicePortConfiguration] != -1; }
    }

    public DevicePortConfigurationBlock GetOrCreateDevicePortConfiguration()
    {
      return (DevicePortConfigurationBlock)getConfigurationBlock(BitConfigurationID.DevicePortConfiguration,
         typeof(DevicePortConfigurationBlock));
    }

    public bool SlumperHealthReportExists
    {
      get { return blockRemapper[(int)BitConfigurationID.SlumperHealthReport] != -1; }
    }

    public SlumperHealthReportBlock GetOrCreateSlumperHealthReport()
    {
      return (SlumperHealthReportBlock)getConfigurationBlock(BitConfigurationID.SlumperHealthReport,
         typeof(SlumperHealthReportBlock));
    }

    #endregion

    #region Implementation of IDeviceSequenceID

    public byte DevicePacketSequenceID
    {
      get { return DevicePacketSequenceIDRaw; }
      set { DevicePacketSequenceIDRaw = value; }
    }

    #endregion

    #region Implementation of INeedsAcknowledgement

    public Type AcknowledgementType
    {
      get { return typeof(BitBlockAckBaseMessage); }
    }

    public ProtocolMessage GenerateAcknowledgement()
    {
      BitBlockAckBaseMessage ack = new BitBlockAckBaseMessage();

      ack.DevicePacketSequenceID = DevicePacketSequenceID;

      return ack;
    }

    public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
    {
      BitBlockAckBaseMessage ack = ackMessage as BitBlockAckBaseMessage;

      return ack != null && ack.DevicePacketSequenceID == DevicePacketSequenceID;
    }

    #endregion
  }

  public abstract class BitConfigurationMessage : NestedMessage
  {
    public override MessageCategory Category
    {
      get { return MessageCategory.BitConfigurationBlockPayload; }
    }

    public BitConfigurationTrackerMessage.BitConfigurationID ConfigurationBlockID
    {
      get { return (BitConfigurationTrackerMessage.BitConfigurationID)PacketID; }
    }

    public uint MessageSize;

    public abstract string Abbreviation { get; }
  }

  public class NetworkConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.NetworkConfiguration; // 0x00

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref RssiMin);
      serializer(action, raw, ref bitPosition, 8, ref RssiMax);
      serializer(action, raw, ref bitPosition, 16, ref InboundCrcErrorCount);
      serializer(action, raw, ref bitPosition, 8, ref NetEntryPercentage);
      serializer(action, raw, ref bitPosition, 8, ref PositionPercentage);
      serializer(action, raw, ref bitPosition, 8, ref ArchivedPositionPercentage);
      serializer(action, raw, ref bitPosition, 8, ref AuxiliaryEventPercentage);
      serializer(action, raw, ref bitPosition, 8, ref BitPacketPercentage);
      filler(ref bitPosition, 8);
      filler(ref bitPosition, 8);
    }

    public override string Abbreviation { get { return "Ntwk"; } }

    public Int16 RssiMin;
    public Int16 RssiMax;
    public UInt16 InboundCrcErrorCount;
    public byte NetEntryPercentage;
    public byte PositionPercentage;
    public byte ArchivedPositionPercentage;
    public byte AuxiliaryEventPercentage;
    public byte BitPacketPercentage;
  }

  public class EnvironmentConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.EnvironmentConfiguration; // 0x01

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref HighestBatteryVoltageRaw);
      serializer(action, raw, ref bitPosition, 8, ref LowestBatteryVoltageRaw);
      serializer(action, raw, ref bitPosition, 8, ref IgnitionOffCount);
      serializer(action, raw, ref bitPosition, 12, ref ShortestOffDurationMinutes);
      serializer(action, raw, ref bitPosition, 12, ref LongestOffDurationMinutes);
      serializer(action, raw, ref bitPosition, 8, ref HightestTemperatureC);
      serializer(action, raw, ref bitPosition, 8, ref LowestTemperatureC);
      serializer(action, raw, ref bitPosition, 24, ref OdometerCalibrationValue);   // ticks/mile
      serializer(action, raw, ref bitPosition, 16, ref OdometerCalibrationConfidence);
      serializer(action, raw, ref bitPosition, 8, ref FlashFilePercentUsed);
    }

    public override string Abbreviation { get { return "Env"; } }

    private byte HighestBatteryVoltageRaw;
    private byte LowestBatteryVoltageRaw;
    public byte IgnitionOffCount;
    public UInt16 ShortestOffDurationMinutes;
    public UInt16 LongestOffDurationMinutes;
    public sbyte HightestTemperatureC;
    public sbyte LowestTemperatureC;
    public UInt32 OdometerCalibrationValue;   // ticks/mile
    public UInt16 OdometerCalibrationConfidence;
    public byte FlashFilePercentUsed;


    public double HighestBatteryVoltage
    {
      get { return HighestBatteryVoltageRaw * Constants.BatteryVoltageConversionMultiplier; }
      set { HighestBatteryVoltageRaw = (byte)(value / Constants.BatteryVoltageConversionMultiplier); }
    }

    public double LowestestBatteryVoltage
    {
      get { return LowestBatteryVoltageRaw * Constants.BatteryVoltageConversionMultiplier; }
      set { LowestBatteryVoltageRaw = (byte)(value / Constants.BatteryVoltageConversionMultiplier); }
    }
  }

  public class NavigationConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.NavigationConfiguration; // 0x02

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref NavInvalidCount);
      serializer(action, raw, ref bitPosition, 10, ref MaxDurationNavInvalidMinutes);
      serializer(action, raw, ref bitPosition, 4, ref AvgSVTrackedCount);
      serializer(action, raw, ref bitPosition, 2, ref LastBootTypeRaw); // enum
      serializer(action, raw, ref bitPosition, 7, ref FiveOrFewerTrackedPercentage);
      filler(ref bitPosition, 1);
      serializer(action, raw, ref bitPosition, 7, ref ValidPercentage);
      serializer(action, raw, ref bitPosition, 1, ref BitResultPassed);
      serializer(action, raw, ref bitPosition, 12, ref MinHdopRaw);
      serializer(action, raw, ref bitPosition, 12, ref MaxHdopRaw);
      serializer(action, raw, ref bitPosition, 8, ref MinSnr);
      serializer(action, raw, ref bitPosition, 8, ref MaxSnr);
      serializer(action, raw, ref bitPosition, 8, ref AvgR);
    }

    public override string Abbreviation { get { return "Nav"; } }

    public byte NavInvalidCount;
    public UInt16 MaxDurationNavInvalidMinutes;
    public byte AvgSVTrackedCount;
    public byte LastBootTypeRaw; // enum
    public byte FiveOrFewerTrackedPercentage;
    public byte ValidPercentage;
    public bool BitResultPassed;
    private UInt16 MinHdopRaw;
    private UInt16 MaxHdopRaw;
    public byte MinSnr;
    public byte MaxSnr;
    public byte AvgR;

    public enum BootType
    {
      Cold,
      Warm,
      Hot
    }

    public BootType LastBootType
    {
      get { return (BootType)LastBootTypeRaw; }
      set { LastBootTypeRaw = (byte)value; }
    }

    public double MinHdop
    {
      get { return MinHdopRaw * Constants.HdopConversionMultiplier; }
      set { MinHdopRaw = (UInt16)(value / Constants.HdopConversionMultiplier); }
    }

    public double MaxHdop
    {
      get { return MaxHdopRaw * Constants.HdopConversionMultiplier; }
      set { MaxHdopRaw = (UInt16)(value / Constants.HdopConversionMultiplier); }
    }
  }

  public class SoftwareConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.SoftwareConfiguration; // 0x03

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref DeviceSoftwareMajorRelease);
      serializer(action, raw, ref bitPosition, 8, ref DeviceSoftwareMinorRelease);
      serializer(action, raw, ref bitPosition, 8, ref DeviceSoftwareBuild);
      serializer(action, raw, ref bitPosition, 8, ref DeviceHardwareMajorRelease);
      serializer(action, raw, ref bitPosition, 8, ref DeviceHardwareMinorRelease);
      serializer(action, raw, ref bitPosition, 8, ref MDTSoftwareMajorRelease);
      serializer(action, raw, ref bitPosition, 8, ref MDTSoftwareMinorRelease);
      serializer(action, raw, ref bitPosition, 8, ref MDTSoftwareBuild);
      serializer(action, raw, ref bitPosition, 8, ref MDTHardwareMajorRelease);
      serializer(action, raw, ref bitPosition, 8, ref MDTHardwareMinorRelease);
    }

    public override string Abbreviation { get { return "Soft"; } }

    public byte DeviceSoftwareMajorRelease;
    public byte DeviceSoftwareMinorRelease;
    public byte DeviceSoftwareBuild;
    public byte DeviceHardwareMajorRelease;
    public byte DeviceHardwareMinorRelease;
    public byte MDTSoftwareMajorRelease;
    public byte MDTSoftwareMinorRelease;
    public byte MDTSoftwareBuild;
    public byte MDTHardwareMajorRelease;
    public byte MDTHardwareMinorRelease;
  }

  public class ReadyMixConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.ReadyMixConfiguration; // 0x04

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 16, ref WashOutCount);
      serializer(action, raw, ref bitPosition, 16, ref WaterOnCount);
      serializer(action, raw, ref bitPosition, 16, ref DoorOpenedCount);
      serializer(action, raw, ref bitPosition, 16, ref DrumChargedCount);
      serializer(action, raw, ref bitPosition, 16, ref DrumDischargedCount);
    }

    public override string Abbreviation { get { return "RMC1"; } }

    public UInt16 WashOutCount;
    public UInt16 WaterOnCount;
    public UInt16 DoorOpenedCount;
    public UInt16 DrumChargedCount;
    public UInt16 DrumDischargedCount;
  }

  public class ConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.Configuration;  // 0x05

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      serializer(action, raw, ref bitPosition, 32, ref ActiveDimListID);
      serializer(action, raw, ref bitPosition, 32, ref InProcessDimListID);
      serializer(action, raw, ref bitPosition, 16, ref DeviceLogicType);
      serializer(action, raw, ref bitPosition, 16, ref DeviceShutdownDelay);
      serializer(action, raw, ref bitPosition, 16, ref MdtShutdownDelay);
      serializer(action, raw, ref bitPosition, 8, ref AlwaysOnDevice);

      // The less than pretty notation is necessary since array covariance doesn't extend to ref parameters.

      ActiveWorkSites = (ActiveSite[])
         serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, ActiveWorkSites, typeof(ActiveSite));
      ActiveHomeSites = (ActiveSite[])
         serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, ActiveHomeSites, typeof(ActiveSite));

      serializer(action, raw, ref bitPosition, 8, ref WorkSiteEntrySpeed);
      serializer(action, raw, ref bitPosition, 8, ref HomeSiteEntrySpeed);
      if (this.PacketID == (int)BitConfigurationTrackerMessage.BitConfigurationID.Configuration2)
      {
        serializer(action, raw, ref bitPosition, 8, ref ((Configuration2Block)this).HomeZoneEntrySpeed);
      }

      serializer(action, raw, ref bitPosition, 8, ref WorkSiteExitSpeed);
      serializer(action, raw, ref bitPosition, 8, ref HomeSiteExitSpeed);
      if (this.PacketID == (int)BitConfigurationTrackerMessage.BitConfigurationID.Configuration2)
      {
        serializer(action, raw, ref bitPosition, 8, ref ((Configuration2Block)this).HomeZoneExitSpeed);
      }

      serializer(action, raw, ref bitPosition, 8, ref WorkSiteHysteresis);
      serializer(action, raw, ref bitPosition, 8, ref HomeSiteHysteresis);
      if (this.PacketID == (int)BitConfigurationTrackerMessage.BitConfigurationID.Configuration2)
      {
        serializer(action, raw, ref bitPosition, 8, ref ((Configuration2Block)this).HomeZoneHysteresis);
      }

      serializer(action, raw, ref bitPosition, 8, ref IgnitionReportingEnable);

      serializer(action, raw, ref bitPosition, 8, ref DiscreteInputEnableRaw);
      if (DiscreteInputEnableRaw != 0x00)
      {
        serializer(action, raw, ref bitPosition, 8, ref DiscreteInputPolarityRaw);
        serializer(action, raw, ref bitPosition, 8, ref DiscreteIgnitionQualificationEnableRaw);
        filler(ref bitPosition, 16);

        if ((DiscreteInputEnableRaw & 0x01) == 0x01)
          serializer(action, raw, ref bitPosition, 16, ref Discrete1HysteresisRaw);
        if ((DiscreteInputEnableRaw & 0x02) == 0x02)
          serializer(action, raw, ref bitPosition, 16, ref Discrete2HysteresisRaw);
        if ((DiscreteInputEnableRaw & 0x04) == 0x04)
          serializer(action, raw, ref bitPosition, 16, ref Discrete3HysteresisRaw);
        if ((DiscreteInputEnableRaw & 0x08) == 0x08)
          serializer(action, raw, ref bitPosition, 16, ref Discrete4HysteresisRaw);
        if ((DiscreteInputEnableRaw & 0x10) == 0x10)
          serializer(action, raw, ref bitPosition, 16, ref Discrete5HysteresisRaw);
        if ((DiscreteInputEnableRaw & 0x20) == 0x20)
          serializer(action, raw, ref bitPosition, 16, ref Discrete6HysteresisRaw);
        if ((DiscreteInputEnableRaw & 0x40) == 0x40)
          serializer(action, raw, ref bitPosition, 16, ref Discrete7HysteresisRaw);
        if ((DiscreteInputEnableRaw & 0x80) == 0x80)
          serializer(action, raw, ref bitPosition, 16, ref Discrete8HysteresisRaw);
      }

      serializer(action, raw, ref bitPosition, 8, ref DiscreteTachometerEnableRaw);
      if (DiscreteTachometerEnableRaw != 0x00)
      {
        serializer(action, raw, ref bitPosition, 8, ref DiscreteTachSpeedQualificationEnableRaw);
        filler(ref bitPosition, 24);

        if ((DiscreteTachometerEnableRaw & 0x01) == 0x01)
          serializer(action, raw, ref bitPosition, 16, ref Discrete1TachHysteresisRaw);
        if ((DiscreteTachometerEnableRaw & 0x02) == 0x02)
          serializer(action, raw, ref bitPosition, 16, ref Discrete2TachHysteresisRaw);
        if ((DiscreteTachometerEnableRaw & 0x04) == 0x04)
          serializer(action, raw, ref bitPosition, 16, ref Discrete3TachHysteresisRaw);
        if ((DiscreteTachometerEnableRaw & 0x08) == 0x08)
          serializer(action, raw, ref bitPosition, 8, ref Discrete1TachFrequencyThresholdRaw);
        if ((DiscreteTachometerEnableRaw & 0x10) == 0x10)
          serializer(action, raw, ref bitPosition, 8, ref Discrete2TachFrequencyThresholdRaw);
        if ((DiscreteTachometerEnableRaw & 0x20) == 0x20)
          serializer(action, raw, ref bitPosition, 8, ref Discrete3TachFrequencyThresholdRaw);
      }

      serializer(action, raw, ref bitPosition, 8, ref DiscreteOverrevEnableRaw);
      if ((DiscreteOverrevEnableRaw & 0x01) == 0x01)
      {
        serializer(action, raw, ref bitPosition, 8, ref DiscreteOverrevFrequencyRaw);
        filler(ref bitPosition, 16);
      }

      serializer(action, raw, ref bitPosition, 8, ref StoreAndForwardEnableRaw);
      if ((StoreAndForwardEnableRaw & 0x01) == 0x01)
        serializer(action, raw, ref bitPosition, 8, ref StoreAndForwardUpdatePeriodRaw);

      serializer(action, raw, ref bitPosition, 8, ref SpeedingReportEnableRaw);
      if ((SpeedingReportEnableRaw & 0x01) == 0x01)
      {
        serializer(action, raw, ref bitPosition, 8, ref SpeedingThresholdRaw);
        serializer(action, raw, ref bitPosition, 16, ref SpeedingDurationRaw);
      }

      serializer(action, raw, ref bitPosition, 8, ref StopNotificationEnableRaw);
      if ((StopNotificationEnableRaw & 0x01) == 0x01)
      {
        serializer(action, raw, ref bitPosition, 8, ref StopThresholdRaw);
        serializer(action, raw, ref bitPosition, 16, ref StopDurationRaw);
      }

      serializer(action, raw, ref bitPosition, 32, ref PrimaryIPAddress);
      serializer(action, raw, ref bitPosition, 16, ref PrimaryPortNumber);
      serializer(action, raw, ref bitPosition, 32, ref SecondaryIPAddress);
      serializer(action, raw, ref bitPosition, 16, ref SecondaryPortNumber);

      // The following are not in the documentation, but are still transmitted
      // in the message.
      filler(ref bitPosition, 16); // Max Transmission Unit
      filler(ref bitPosition, 16); // Init Retrans delay
      filler(ref bitPosition, 8); // Max retrans
      filler(ref bitPosition, 16); // Appl. Levle Retrans delay

      serializer(action, raw, ref bitPosition, 8, ref SensorType);
      serializer(action, raw, ref bitPosition, 8, ref ReverseSignalPolarity);
      serializer(action, raw, ref bitPosition, 16, ref PulsesPerDrumRevolution);
      filler(ref bitPosition, 48);

      serializer(action, raw, ref bitPosition, 8, ref DefaultFileSystemMissing);

      if (this.PacketID == (int)BitConfigurationTrackerMessage.BitConfigurationID.Configuration2)
      {
        serializer(action, raw, ref bitPosition, 8, ref ((Configuration2Block)this).PushButtonStatusing);

        serializer(action, raw, ref bitPosition, 8, ref ((Configuration2Block)this).WashOutSensor);

        // These come as a unit so we don't need to check for each byte

        serializer(action, raw, ref bitPosition, 16, ref ((Configuration2Block)this).WaterMeterPulsesPerGallon);

        ((Configuration2Block)this).LoadingZoneSiteIDs = (ActiveZone[])
           serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, ((Configuration2Block)this).LoadingZoneSiteIDs, typeof(ActiveZone));

        ((Configuration2Block)this).MuteZoneSiteIDs = (ActiveZone[])
           serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, ((Configuration2Block)this).MuteZoneSiteIDs, typeof(ActiveZone));

        ((Configuration2Block)this).RelocatableJobSiteIDs = (ActiveZone[])
           serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, ((Configuration2Block)this).RelocatableJobSiteIDs, typeof(ActiveZone));
      }
      if (this.PacketID == (int)BitConfigurationTrackerMessage.BitConfigurationID.Configuration2
        && (raw != null && raw.Length - 1 > (bitPosition / 8) || (((Configuration2Block)this).GPSEventConfigEnabled.HasValue)))
      {
        ((Configuration2Block)this).includeGPSConfig = true;
        serializer(action, raw, ref bitPosition, 8, ref ((Configuration2Block)this).gpsEventConfigEnabled);


        // The following settings apply to crosscheck only. The MTS is supposed to pad these bytes with zeros, but 
        // is not doing that predictably. This code allows for the bytes following the "GPS Event Configuration" field
        // of the Config II message to be optional.
        if ((raw != null && raw.Length - 1 > (bitPosition / 8)) || ((Configuration2Block)this).includeEngineStartStopEnable)
        {
          ((Configuration2Block)this).includeEngineStartStopEnable = true;
          serializer(action, raw, ref bitPosition, 8, ref ((Configuration2Block)this).engineStartStopEnable);

          serializer(action, raw, ref bitPosition, 32, ref ((Configuration2Block)this).engineRuntimeIntervalsSec);
          serializer(action, raw, ref bitPosition, 16, ref ((Configuration2Block)this).firstMixInterval);
          serializer(action, raw, ref bitPosition, 16, ref ((Configuration2Block)this).firstMixDuration);
          serializer(action, raw, ref bitPosition, 16, ref ((Configuration2Block)this).lastMixInterval);
          serializer(action, raw, ref bitPosition, 16, ref ((Configuration2Block)this).lastMixDuration);
          serializer(action, raw, ref bitPosition, 16, ref ((Configuration2Block)this).minRotationSpeedForMeasurement);
          serializer(action, raw, ref bitPosition, 16, ref ((Configuration2Block)this).pressureScaleFactor);
        }
      }

      configBlockLength.Backfill(bitPosition);
      MessageSize = configBlockLength.messageSize;
    }

    public override string Abbreviation { get { return "Cnfg"; } }

    public Int32 ActiveDimListID;
    public Int32 InProcessDimListID;
    public UInt16 DeviceLogicType;         
    public UInt16 DeviceShutdownDelay;
    public UInt16 MdtShutdownDelay;
    public bool AlwaysOnDevice;
    public ActiveSite[] ActiveWorkSites;
    public ActiveSite[] ActiveHomeSites;
    public byte WorkSiteEntrySpeed;
    public byte HomeSiteEntrySpeed;
    public byte WorkSiteExitSpeed;
    public byte HomeSiteExitSpeed;
    public byte WorkSiteHysteresis;
    public byte HomeSiteHysteresis;

    public bool IgnitionReportingEnable;

    public byte DiscreteInputEnableRaw;
    public byte DiscreteInputPolarityRaw;
    public byte DiscreteIgnitionQualificationEnableRaw;
    public UInt16 Discrete1HysteresisRaw;
    public UInt16 Discrete2HysteresisRaw;
    public UInt16 Discrete3HysteresisRaw;
    public UInt16 Discrete4HysteresisRaw;
    public UInt16 Discrete5HysteresisRaw;
    public UInt16 Discrete6HysteresisRaw;
    public UInt16 Discrete7HysteresisRaw;
    public UInt16 Discrete8HysteresisRaw;

    public byte DiscreteTachometerEnableRaw;
    public byte DiscreteTachSpeedQualificationEnableRaw;
    public UInt16 Discrete1TachHysteresisRaw;
    public UInt16 Discrete2TachHysteresisRaw;
    public UInt16 Discrete3TachHysteresisRaw;
    public byte Discrete1TachFrequencyThresholdRaw;
    public byte Discrete2TachFrequencyThresholdRaw;
    public byte Discrete3TachFrequencyThresholdRaw;

    public byte DiscreteOverrevEnableRaw;
    public byte DiscreteOverrevFrequencyRaw;

    public byte StoreAndForwardEnableRaw;
    public byte StoreAndForwardUpdatePeriodRaw;

    public byte SpeedingReportEnableRaw;
    public byte SpeedingThresholdRaw;
    public UInt16 SpeedingDurationRaw;

    public byte StopNotificationEnableRaw;
    public byte StopThresholdRaw;
    public UInt16 StopDurationRaw;

    public UInt32 PrimaryIPAddress;
    public UInt16 PrimaryPortNumber;
    public UInt32 SecondaryIPAddress;
    public UInt16 SecondaryPortNumber;

    public byte SensorType;
    public byte ReverseSignalPolarity;
    public UInt16 PulsesPerDrumRevolution;

    public byte DefaultFileSystemMissing;

    public class ActiveSite : NestedMessage
    {
      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        serializer(action, raw, ref bitPosition, 32, ref SiteID);
        serializer(action, raw, ref bitPosition, 32, ref MessageSequenceID);
      }

      public Int64 SiteID;
      public Int64 MessageSequenceID;
    }

    public class ActiveZone : NestedMessage
    {
      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        serializer(action, raw, ref bitPosition, 32, ref SiteID);
      }

      public Int64 SiteID;
    }
  }

  public class NetworkInterfaceConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.NetworkInterfaceConfiguration;  // 0x06

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      serializeNulTerminatedString(action, raw, ref bitPosition, ref StackConfigString1);
      serializeNulTerminatedString(action, raw, ref bitPosition, ref StackConfigString2);
      serializeNulTerminatedString(action, raw, ref bitPosition, ref StackConfigString3);
      serializeNulTerminatedString(action, raw, ref bitPosition, ref StackConfigString4);
      serializeNulTerminatedString(action, raw, ref bitPosition, ref ApplicationConfigString);

      configBlockLength.Backfill(bitPosition);
    }

    public override string Abbreviation { get { return "Itfc"; } }

    public string StackConfigString1;
    public string StackConfigString2;
    public string StackConfigString3;
    public string StackConfigString4;
    public string ApplicationConfigString;
  }

  // Network Hardware BIT Block
  // Note: This packet is not yet implemented in the device firmware
  //@@@@   need 0x07  CDPD
  //   2	Block Length
  //   1	Modem Module Type
  //   1	Diagnostic Major Version
  //   1	Diagnostic Minor Version
  //   2	Diagnostic Build
  //   1	Module FW version length
  //   Variable	Module FW version (string)
  //   1	Module HW version length
  //   Variable	Module HW version (string)

  public class ReadyMixConfiguration2Block : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.ReadyMixConfiguration2;  // 0x08

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      serializer(action, raw, ref bitPosition, 16, ref Version);

      if (Version == 1)
      {
        serializer(action, raw, ref bitPosition, 16, ref MaxDrumChargeSpeedRph);
        serializer(action, raw, ref bitPosition, 16, ref MaxDrumDischargeSpeedRph);
        serializer(action, raw, ref bitPosition, 32, ref WashLifetimeCounterSeconds);
        serializer(action, raw, ref bitPosition, 32, ref WaterAddedLifetimeCounterGallons);
        serializer(action, raw, ref bitPosition, 16, ref WaterAddedGallons);
        serializer(action, raw, ref bitPosition, 32, ref WaterAddedDurationSeconds);
        serializer(action, raw, ref bitPosition, 16, ref WaterAddedMaxFlowRateGpm);
        serializer(action, raw, ref bitPosition, 16, ref MagnetGapDetections);
        serializer(action, raw, ref bitPosition, 32, ref MaxWeightLbs);
        serializer(action, raw, ref bitPosition, 8, ref WeightValid);
      }

      configBlockLength.Backfill(bitPosition);
    }

    public override string Abbreviation { get { return "RMC2"; } }

    public UInt16 Version;
    public UInt16 MaxDrumChargeSpeedRph;
    public UInt16 MaxDrumDischargeSpeedRph;
    public UInt32 WashLifetimeCounterSeconds;
    public UInt32 WaterAddedLifetimeCounterGallons;
    public UInt16 WaterAddedGallons;
    public UInt32 WaterAddedDurationSeconds;
    public UInt16 WaterAddedMaxFlowRateGpm;
    public UInt16 MagnetGapDetections;
    public UInt32 MaxWeightLbs;
    public bool WeightValid;
  }

  public class SecurityApplicationConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.SecurityApplicationConfiguration;  // 0x09

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      serializer(action, raw, ref bitPosition, 8, ref AlarmConditionEnableRaw);
      serializer(action, raw, ref bitPosition, 32, ref AlarmSessionTimeoutSeconds);
      serializer(action, raw, ref bitPosition, 16, ref SirenTimeoutSeconds);
      serializer(action, raw, ref bitPosition, 32, ref PowerManagementShutdownTimeoutSeconds);
      serializer(action, raw, ref bitPosition, 16, ref PowerManagementMinTimeoutSeconds);
      serializer(action, raw, ref bitPosition, 16, ref PowerManagementMaxTimeoutSeconds);
      serializer(action, raw, ref bitPosition, 16, ref PowerManagementGpsTimeoutSeconds);
      serializer(action, raw, ref bitPosition, 16, ref PowerManagementCommsTimeoutSeconds);
      serializer(action, raw, ref bitPosition, 16, ref TimeDistanceReportingMinTimeoutSeconds);
      serializer(action, raw, ref bitPosition, 16, ref TimeDistanceReportingMaxTimeoutSeconds);
      serializer(action, raw, ref bitPosition, 16, ref TimeDistanceReportingMaxDistanceMeters);

      // The less than pretty notation is necessary since array covariance doesn't extend to ref parameters.

      InputConfigurations = (IOConfiguration[])
         serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, InputConfigurations, typeof(IOConfiguration));
      OutputConfigurations = (IOConfiguration[])
         serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, OutputConfigurations, typeof(IOConfiguration));

      configBlockLength.Backfill(bitPosition);
    }

    public override string Abbreviation { get { return "Sec"; } }

    private byte AlarmConditionEnableRaw;
    public UInt32 AlarmSessionTimeoutSeconds;
    public UInt16 SirenTimeoutSeconds;
    public UInt32 PowerManagementShutdownTimeoutSeconds;
    public UInt16 PowerManagementMinTimeoutSeconds;
    public UInt16 PowerManagementMaxTimeoutSeconds;
    public UInt16 PowerManagementGpsTimeoutSeconds;
    public UInt16 PowerManagementCommsTimeoutSeconds;
    public UInt16 TimeDistanceReportingMinTimeoutSeconds;
    public UInt16 TimeDistanceReportingMaxTimeoutSeconds;
    public UInt16 TimeDistanceReportingMaxDistanceMeters;

    public IOConfiguration[] InputConfigurations;
    public IOConfiguration[] OutputConfigurations;

    public enum AlarmCondition
    {
      Panic,
      Pursuit
    }

    public AlarmCondition AlarmConditionEnable
    {
      get { return (AlarmConditionEnableRaw == 0x00) ? AlarmCondition.Panic : AlarmCondition.Pursuit; }
      set { AlarmConditionEnableRaw = (value == AlarmCondition.Panic) ? (byte)0x00 : (byte)0x02; }
    }

    public class IOConfiguration : NestedMessage
    {
      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        serializer(action, raw, ref bitPosition, 8, ref ioConfiguration);
      }

      public byte ioConfiguration;
    }
  }

  public class Configuration2Block : ConfigurationBlock
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.Configuration2;  // 0x0a

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      // We derive off ConfigurationBlock since all but 3 properties are the same however the
      // serialization order is different.  All the serialization logic is in the base Configuration
      // block's serializer that conditionally skips or includes the new fields based on the
      // block ID.

      base.Serialize(action, raw, ref bitPosition);
    }

    public override string Abbreviation { get { return "Conf2"; } }

    public byte HomeZoneEntrySpeed;
    public byte HomeZoneExitSpeed;
    public byte HomeZoneHysteresis;

    //skyline 

    public bool? GPSEventConfigEnabled
    {
      get 
      { 
        if (includeGPSConfig) 
          return gpsEventConfigEnabled; 
        else return null; 
      }
      set
      {
        if (!value.HasValue)
        {
          includeGPSConfig = false;
          gpsEventConfigEnabled = false;
        }
        else
        {
          includeGPSConfig = true;
          gpsEventConfigEnabled = value.Value;
        }
      }
    }
    internal bool gpsEventConfigEnabled;
    internal bool includeGPSConfig = false;
    public byte? EngineStartStopEnable
    {
     get
     {
       if (includeEngineStartStopEnable)
         return engineStartStopEnable;

       return null;
     }
      set
      {
        if (value.HasValue)
        {
          includeEngineStartStopEnable = true;
          engineStartStopEnable = value.Value;
        }
        else
        {
          includeEngineStartStopEnable = false;
          engineStartStopEnable = 0;
        }
      }
    }
    internal bool includeEngineStartStopEnable = false;
    internal byte engineStartStopEnable = 0;
    public uint engineRuntimeIntervalsSec;

    //Hydrolic pressure config
    public ushort firstMixInterval;
    public ushort firstMixDuration;
    public ushort lastMixInterval;
    public ushort lastMixDuration;
    public ushort minRotationSpeedForMeasurement;
    public ushort pressureScaleFactor;

    public byte PushButtonStatusing;

    public byte WashOutSensor;
    public UInt16 WaterMeterPulsesPerGallon;
    public ActiveZone[] LoadingZoneSiteIDs;
    public ActiveZone[] MuteZoneSiteIDs;
    public ActiveZone[] RelocatableJobSiteIDs;
  }

  public class DriverIDConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.DriverIDConfiguration;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      serializer(action, raw, ref bitPosition, 8, ref Enabled);
      serializer(action, raw, ref bitPosition, 8, ref MdtDataEntryEnabled);
      serializer(action, raw, ref bitPosition, 8, ref ForceDriverLogin);
      serializer(action, raw, ref bitPosition, 8, ref CharactersetRaw);
      serializer(action, raw, ref bitPosition, 8, ref MdtIDMaxLength);
      serializer(action, raw, ref bitPosition, 8, ref MdtIDMinLength);
      serializer(action, raw, ref bitPosition, 8, ref MdtDisplayedDriverListSize);
      serializer(action, raw, ref bitPosition, 8, ref MemorizedDriverListSize);
      serializer(action, raw, ref bitPosition, 8, ref AutologoutEnabled);
      serializer(action, raw, ref bitPosition, 32, ref AutologoutTimerMinutes);
      serializer(action, raw, ref bitPosition, 8, ref ExpireAnyMruEntryEnabled);
      serializer(action, raw, ref bitPosition, 32, ref MruEntryExpirationSeconds);
      serializer(action, raw, ref bitPosition, 8, ref ExpireUnvalidatedMruEntriesEnabled);
      serializer(action, raw, ref bitPosition, 32, ref UnvalidatedMruEntryExpirationSeconds);
      serializer(action, raw, ref bitPosition, 8, ref DisplayMechanicID);

      //	12	Mechanic ID. Unused bytes are 0. NOT null-terminated

      serializeFixedLengthString(action, raw, ref bitPosition, 12, ref MechanicID);

      //	16	Mechanic Display Name. Unused bytes are 0. NOT null-terminated

      serializeFixedLengthString(action, raw, ref bitPosition, 16, ref MechanicDisplayName);

      configBlockLength.Backfill(bitPosition);
    }

    public override string Abbreviation { get { return "DIDC"; } }

    public bool Enabled;
    public bool MdtDataEntryEnabled;
    public bool ForceDriverLogin;
    private UInt16 CharactersetRaw;
    public UInt16 MdtIDMaxLength;
    public UInt16 MdtIDMinLength;
    public UInt16 MdtDisplayedDriverListSize;
    public UInt16 MemorizedDriverListSize;
    public bool AutologoutEnabled;
    public UInt32 AutologoutTimerMinutes;
    public bool ExpireAnyMruEntryEnabled;
    public UInt32 MruEntryExpirationSeconds;
    public bool ExpireUnvalidatedMruEntriesEnabled;
    public UInt32 UnvalidatedMruEntryExpirationSeconds;
    public bool DisplayMechanicID;
    public string MechanicID;
    public string MechanicDisplayName;

    public enum CharactersetType
    {
      Numeric,
      Alphabetic,
      Alphanumeric
    }

    public CharactersetType Characterset
    {
      get { return (CharactersetType)CharactersetRaw; }
      set { CharactersetRaw = (UInt16)value; }
    }
  }

  public class DriverTimeMgrConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.DriverTimeMgrConfiguration;  // 0x0c

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      serializer(action, raw, ref bitPosition, 8, ref BreakReminder1Enabled);
      serializer(action, raw, ref bitPosition, 32, ref BreakReminder1TimeoutMinutes);
      serializer(action, raw, ref bitPosition, 8, ref BreakReminder2Enabled);
      serializer(action, raw, ref bitPosition, 32, ref BreakReminder2TimeoutMinutes);
      serializer(action, raw, ref bitPosition, 32, ref ReminderSnoozeTimeoutMinutes);
      serializer(action, raw, ref bitPosition, 8, ref MovingOnBreakRaw);  // enum

      configBlockLength.Backfill(bitPosition);
    }

    public override string Abbreviation { get { return "DTMC"; } }

    public bool BreakReminder1Enabled;
    public UInt32 BreakReminder1TimeoutMinutes;
    public bool BreakReminder2Enabled;
    public UInt32 BreakReminder2TimeoutMinutes;
    public UInt32 ReminderSnoozeTimeoutMinutes;
    private byte MovingOnBreakRaw;  // enum

    public enum MovingOnBreakAction
    {
      Ignore,
      Prompt,
      FinishBreak
    }

    public MovingOnBreakAction MovingOnBreak
    {
      get { return (MovingOnBreakAction)MovingOnBreakRaw; }
      set { MovingOnBreakRaw = (byte)value; }
    }
  }

    public class IPFilterConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.IPFilteringConfiguration; // 0x13

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      AllowedAddresses = (AllowedAddress[])
         serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, AllowedAddresses, typeof(AllowedAddress));

      configBlockLength.Backfill(bitPosition);
    }

    public class AllowedAddress : NestedMessage
    {
      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        // JBusValue packets do not call the base serializer.  There is no 'packet ID' to
        // pick up since the type is implied by context.

        serializer(action, raw, ref bitPosition, 32, ref AllowedIPAddress);
        serializer(action, raw, ref bitPosition, 32, ref NetMask);
        serializer(action, raw, ref bitPosition, 16, ref MinPort);
        serializer(action, raw, ref bitPosition, 16, ref MaxPort);
        serializer(action, raw, ref bitPosition, 8, ref ProtocolRaw);
      }

      public UInt32 AllowedIPAddress;
      public UInt32 NetMask;
      public UInt16 MinPort;
      public UInt16 MaxPort;
      private byte ProtocolRaw;

      public enum ProtocolType
      {
        Icmp = 1,
        Igmp = 2,
        Tcp = 6,
        Udp = 17
      }

      public ProtocolType Protocol
      {
        get { return (ProtocolType)ProtocolRaw; }
        set { ProtocolRaw = (byte)value; }
      }
    }

    public AllowedAddress[] AllowedAddresses;

    public override string Abbreviation { get { return "PornFlt"; } }
  }

  public class DevicePortConfigurationBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.DevicePortConfiguration; // 0x14

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      PortConfigurations = (PortConfiguration[])
         serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, PortConfigurations, typeof(PortConfiguration));

      configBlockLength.Backfill(bitPosition);
    }

    public class PortConfiguration : NestedMessage
    {
      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        // JBusValue packets do not call the base serializer.  There is no 'packet ID' to
        // pick up since the type is implied by context.

        serializer(action, raw, ref bitPosition, 8, ref PortNumberRaw);
        serializer(action, raw, ref bitPosition, 8, ref ServiceTypeRaw);
      }

      private byte PortNumberRaw;
      private byte ServiceTypeRaw;

      public enum Port
      {
        SIO1 = 1, // (DB9)
        SIO2 = 2, // (RJ45)
        JBX1 = 3, // (JBX – Serial Port 1)
        JBX2 = 4  // (JBX – Serial Port 2)
      }

      public enum Service
      {
        Diagnostic = 0, // (CONIO)
        MDT = 1,
        UserDataService = 2,
        SensorPNP4800 = 3,
        SensorPNP9600 = 4,
        NMEA = 5,
        JbxModule = 6,
        WanAccess = 7
      }

      public Port PortNumber
      {
        get { return (Port)PortNumberRaw; }
        set { PortNumberRaw = (byte)value; }
      }

      public Service ServiceType
      {
        get { return (Service)ServiceTypeRaw; }
        set { ServiceTypeRaw = (byte)value; }
      }
    }

    public PortConfiguration[] PortConfigurations;

    public override string Abbreviation { get { return "PortCfg"; } }
  }

  public class SlumperHealthReportBlock : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.SlumperHealthReport; // 0x15

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      serializer(action, raw, ref bitPosition, 8, ref Version);

      if (Version == 1)
      {
        serializer(action, raw, ref bitPosition, 16, ref SlumperFirmwareVersion);
        serializer(action, raw, ref bitPosition, 16, ref UnavailableSeconds);
        serializer(action, raw, ref bitPosition, 16, ref ModbusDesyncsCount);
        serializer(action, raw, ref bitPosition, 16, ref ModbusTimeoutCount);
        serializer(action, raw, ref bitPosition, 16, ref ModbusExceptionCount);
        serializer(action, raw, ref bitPosition, 16, ref HardResets);
        serializer(action, raw, ref bitPosition, 16, ref SoftResets);
        serializer(action, raw, ref bitPosition, 8, ref AlarmsRaw);
      }

      configBlockLength.Backfill(bitPosition);
    }

    public override string Abbreviation { get { return "Slumper"; } }

    public byte Version;
    public Int16 SlumperFirmwareVersion;
    public Int16 UnavailableSeconds;
    public Int16 ModbusDesyncsCount;
    public Int16 ModbusTimeoutCount;
    public Int16 ModbusExceptionCount;
    public Int16 HardResets;
    public Int16 SoftResets;
    private byte AlarmsRaw;

    [Flags]
    public enum AlarmFlags
    {
      None = 0,
      ComponentAlarm = 0x01,
      H2ONoFlow = 0x02,
      H2ONoStop = 0x04,
      SPNoFlow = 0x08,
      SPNoStop = 0x10
    }

    public AlarmFlags Alarms
    {
      get { return (AlarmFlags)AlarmsRaw; }
      set { AlarmsRaw = (byte)value; }
    }
  }

  public class UnknownBITConfiguration : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.UnknownBIT;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override string Abbreviation
    {
      get { return "UnknownBIT"; }
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