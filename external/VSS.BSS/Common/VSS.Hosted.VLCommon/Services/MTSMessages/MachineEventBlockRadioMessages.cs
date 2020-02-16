using System;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public abstract class MachineEventBlockRadioPayload : MachineEventBlockPayload
  {
    public override MessageCategory Category
    {
      get
      {
        return MessageCategory.MachineEventBlockRadioPayload;
      }
    }
  }

  public class PositionMachineEventBlock : MachineEventBlockRadioPayload
  {
    public static new readonly int kPacketID = 0x00;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public double DeltaLatitude
    {
      get { return (double)LatitudeRaw * Constants.LatLongConversionMultiplier; }
      set { LatitudeRaw = (int)(value / Constants.LatLongConversionMultiplier); }
    }
    public double DeltaLongitude
    {
      get { return (double)LongitudeRaw * Constants.LatLongConversionMultiplier; }
      set { LongitudeRaw = (int)(value / Constants.LatLongConversionMultiplier); }
    }

    #region Implementation of ISpeedHeading

    public Double Speed
    {
      get { return (double)SpeedRaw * Constants.SpeedConversionMultiplier; }
      set { SpeedRaw = (byte)(value / Constants.SpeedConversionMultiplier); }
    }

    public Double Track
    {
      get { return (double)TrackRaw * Constants.HeadingConversionMultiplier; }
      set { TrackRaw = (sbyte)(value / Constants.HeadingConversionMultiplier); }
    }

    public Double MileageDelta
    {
      get { return (double)DistanceDeltaRaw * Constants.EUDDistanceTraveledConversionMultiplier; }
      set { DistanceDeltaRaw = (UInt16)(value / Constants.EUDDistanceTraveledConversionMultiplier); }
    }

    public LocationUncertaintyUnitEnum LocationUncertaintyUnit
    {
      get { return (LocationUncertaintyUnitEnum)LocationUncertaintyUnitRaw; }
      set { LocationUncertaintyUnitRaw = (byte)(value); }
    }

    public byte DeltaLocationUncertainty;
    public bool invalidLocation;

    private byte LocationUncertaintyUnitRaw;

    internal int LatitudeRaw;
    internal int LongitudeRaw;
    internal byte SpeedRaw;
    internal sbyte TrackRaw;
    internal uint DistanceDeltaRaw;


    #endregion

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 17, ref LatitudeRaw);
      serializer(action, raw, ref bitPosition, 17, ref LongitudeRaw);
      serializer(action, raw, ref bitPosition, 10, ref DistanceDeltaRaw);
      serializer(action, raw, ref bitPosition, 8, ref SpeedRaw);
      serializer(action, raw, ref bitPosition, 7, ref TrackRaw);
      serializer(action, raw, ref bitPosition, 2, ref LocationUncertaintyUnitRaw);
      serializer(action, raw, ref bitPosition, 8, ref DeltaLocationUncertainty);
      serializer(action, raw, ref bitPosition, 1, ref invalidLocation);
      filler(ref bitPosition, 2);
    }

    #region "Unconverted value properties for OEM Data Feed"
    public int DeltaLatitudeUnConverted
    {
      get { return LatitudeRaw; }
    }

    public int DeltaLongitudeUnConverted
    {
      get { return LongitudeRaw; }
    }

    public uint SpeedUnConverted
    {
      get { return SpeedRaw; }
    }

    public int TrackUnConverted
    {
      get { return TrackRaw; }
    }

    public uint MileageDeltaUnConverted
    {
      get { return DistanceDeltaRaw; }
    }
    #endregion
  }

  public class EngineStartStopEventBlock : MachineEventBlockRadioPayload
  {
    public static new readonly int kPacketID = 0x01;
    public override int PacketID
    {
      get { return kPacketID; }
    }

    public int EngineStart;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref EngineStart);
    }
  }

  public class GensetStatusBlock : MachineEventBlockRadioPayload
  {
    public static new readonly int kPacketID = 0x09;
    public override int PacketID
    {
      get { return kPacketID; }
    }

    public int EngineStatus;
    public int OperatingState;
    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      serializer(action, raw, ref bitPosition, 8, ref EngineStatus);
      serializer(action, raw, ref bitPosition, 8, ref OperatingState);
    }
  }

    public class IgnitionOnOffEventBlock : MachineEventBlockRadioPayload
  {
    public static new readonly int kPacketID = 0x04;
    public override int PacketID
    {
      get
      { return kPacketID; }
    }

    public bool IsOn;
    public ushort RunTimeCounterHours;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 15, ref RunTimeCounterHours);
      serializer(action, raw, ref bitPosition, 1, ref IsOn);
    }
  }

  public class DiscreteInputEventBlock : MachineEventBlockRadioPayload
  {
    public static new readonly int kPacketID = 0x05;
    public override int PacketID
    {
      get
      { return kPacketID; }
    }

    private byte discreteSelectorRaw;
    public bool DiscreteOn;

    public byte DiscreteSelector // A 1-based number
    {
      get
      {
        return (byte)(LowestBitIndexSet(discreteSelectorRaw) + 1);  // It is a 1-based number.
      }
      set { discreteSelectorRaw = (byte)(1 << (value - 1)); }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 3, ref discreteSelectorRaw);
      filler(ref bitPosition, 5);
      serializer(action, raw, ref bitPosition, 1, ref DiscreteOn);
      filler(ref bitPosition, 7);
    }
  }

  public class SpeedingIndicationEventBlock : MachineEventBlockRadioPayload
  {
    public static new readonly int kPacketID = 0x06;
    public override int PacketID
    {
      get
      { return kPacketID; }
    }

    private ushort durationSecondsRaw;
    private ushort DistanceTraveledMilesRaw;
    public bool BeginSpeeding;
    public byte MaximumSpeedMph;

    public TimeSpan Duration
    {
      get { return TimeSpan.FromSeconds(durationSecondsRaw); }
      set { durationSecondsRaw = (ushort)value.TotalSeconds; }
    }

    public double DistanceTraveledMiles
    {
      get { return DistanceTraveledMilesRaw * Constants.DistanceTraveledInTenthsConversionMultiplier; }
      set { DistanceTraveledMilesRaw = (ushort)(value / Constants.DistanceTraveledInTenthsConversionMultiplier); }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 16, ref durationSecondsRaw);
      serializer(action, raw, ref bitPosition, 16, ref DistanceTraveledMilesRaw);
      serializer(action, raw, ref bitPosition, 8, ref BeginSpeeding);
      serializer(action, raw, ref bitPosition, 8, ref MaximumSpeedMph);
    }

    #region "Unconverted value properties for OEM Data Feed"
    public ushort DurationUnConverted
    {
      get { return durationSecondsRaw; }
    }

    public ushort DistanceTraveledMilesUnConverted
    {
      get { return DistanceTraveledMilesRaw; }
    }
    #endregion

  }

  public class StoppedNotificationEventBlock : MachineEventBlockRadioPayload
  {
    public static new readonly int kPacketID = 0x07;
    public override int PacketID
    {
      get
      { return kPacketID; }
    }

    public bool StoppedMoving;
    public bool SuspiciousMove;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 1, ref StoppedMoving);
      serializer(action, raw, ref bitPosition, 1, ref SuspiciousMove);
      filler(ref bitPosition, 6);
    }
  }

  public class SiteEntryExitEventBlock : MachineEventBlockRadioPayload
  {
    public static new readonly int kPacketID = 0x08;
    public override int PacketID
    {
      get
      { return kPacketID; }
    }

    private byte SiteTypeRaw;

    public DeviceSiteType SiteType
    {
      get { return (DeviceSiteType)SiteTypeRaw; }
      set { SiteTypeRaw = (byte)value; }
    }

    public bool IsDeparture;
    public bool AutomaticSource;
    public bool UserSource;
    private long SiteIDRaw;
    
    public long SiteID
    {
      get { return SiteIDRaw; }
      set { SiteIDRaw = value; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 2, ref SiteTypeRaw);
      serializer(action, raw, ref bitPosition, 1, ref IsDeparture);
      serializer(action, raw, ref bitPosition, 1, ref AutomaticSource);
      serializer(action, raw, ref bitPosition, 1, ref UserSource);
      filler(ref bitPosition, 3);
      serializer(action, raw, ref bitPosition, 32, ref SiteIDRaw);
    }
  }

    public class DeviceMachineSecurityReportingStatusMessage : MachineEventBlockRadioPayload
  {
      public static new readonly int kPacketID = 0x17;

      public override int PacketID
      {
          get { return kPacketID; }
      }

      public byte byteCount;

      public MachineSecurityModeSetting LatestMachineSecurityModeconfiguration
      {
          get { return (MachineSecurityModeSetting)(sbyte)latestMachineSecurityModeconfiguration; }
          set { latestMachineSecurityModeconfiguration = (byte)value; }
      }

      private byte latestMachineSecurityModeconfiguration;

      public MachineSecurityModeSetting CurrentMachineSecurityModeconfiguration
      {
          get { return (MachineSecurityModeSetting)(sbyte)currentMachineSecurityModeconfiguration; }
          set { currentMachineSecurityModeconfiguration = (byte)value; }
      }

      private byte currentMachineSecurityModeconfiguration;

      public TamperResistanceStatus TamperResistanceMode
      {
          get { return (TamperResistanceStatus)(sbyte)tamperResistanceMode; }
          set { tamperResistanceMode = (byte)value; }
      }

      private byte tamperResistanceMode;

      public DeviceSecurityModeReceivingStatus DeviceSecurityModeReceivingStatus
      {
          get { return (DeviceSecurityModeReceivingStatus)deviceSecurityModeReceivingStatus; }
          set { deviceSecurityModeReceivingStatus = (byte)value; }
      }

      private byte deviceSecurityModeReceivingStatus;

      public SourceSecurityModeConfiguration SourceSecurityModeConfiguration
      {
          get { return (SourceSecurityModeConfiguration)sourceSecurityModeConfiguration; }
          set { sourceSecurityModeConfiguration = (byte)value; }
      }

      private byte sourceSecurityModeConfiguration;

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
          base.Serialize(action, raw, ref bitPosition);
          serializer(action, raw, ref bitPosition, 8, ref byteCount);
          serializer(action, raw, ref bitPosition, 8, ref latestMachineSecurityModeconfiguration);
          serializer(action, raw, ref bitPosition, 8, ref currentMachineSecurityModeconfiguration);
          serializer(action, raw, ref bitPosition, 8, ref tamperResistanceMode);
          serializer(action, raw, ref bitPosition, 8, ref deviceSecurityModeReceivingStatus);
          serializer(action, raw, ref bitPosition, 8, ref sourceSecurityModeConfiguration);
          
      }
  }

  //public class OccupiedSitesEventBlock : MachineEventBlockRadioPayload
  //{
  //  public static new readonly int kPacketID = 0x09;
  //  public override int PacketID
  //  {
  //    get
  //    { return kPacketID; }
  //  }

  //  public List<long> SiteIDs;

  //  public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
  //  {
  //    base.Serialize(action, raw, ref bitPosition);

  //    byte numSites = action == SerializationAction.Hydrate ? (byte)0 : (byte)SiteIDs.Count();
  //    serializer(action, raw, ref bitPosition, 8, ref numSites);
  //    if(action == SerializationAction.Hydrate)
  //      SiteIDs = new List<long>();
  //    for (int i = 0; i < numSites; i++)
  //    {
  //      long siteID = action == SerializationAction.Hydrate ? 0 : SiteIDs[i];

  //      serializer(action, raw, ref bitPosition, 32, ref siteID);
        
  //      if(action == SerializationAction.Hydrate)
  //        SiteIDs.Add(siteID);
  //    }
  //  }
  //}

  public class UnkownMachineEventRadioData : MachineEventBlockRadioPayload
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
