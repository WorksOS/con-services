using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public enum LocationAgeUnitEnum
  {
    Seconds = 0,
    Minutes = 1,
    Hours = 2,
    Days = 3,
    Months = 4
  }

  public enum LocationUncertaintyUnitEnum
  {
    Centimeters = 0,
    Meters = 1,
  }

  public enum MachineEventSourceEnum
  {
    Gateway = 0,
    LiteVIMS = 1,
    Radio = 2,
    VehicleBus = 3,
  }

  public class MachineEventMessage : TrackerMessage, INeedsAcknowledgement, TrackerMessage.IDeviceSequenceID
  {
    private readonly DateTime beginTimeUTC = new DateTime(2009, 01, 01);

    public static new readonly int kPacketID = 0x07;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public DateTime UtcDateTime
    {
      get
      {
        return beginTimeUTC.AddSeconds(SecondsElapsed);
      }
      set
      {
        SecondsElapsed = (uint)value.Subtract(beginTimeUTC).TotalSeconds;
      }
    }

    public double ServiceMeterHours
    {
      get 
      {
        if (ServiceMeterHoursRaw == uint.MaxValue)
          return double.NaN;
        return (double)(ServiceMeterHoursRaw / 10.0); 
      }
      set { ServiceMeterHoursRaw = (uint)(value * 10.0); }
    }

    public double? SMHAfterCalibration { get; set; }

    public double Latitude
    {
      get { return (double)LatitudeRaw * Constants.LatLongConversionMultiplier; }
      set { LatitudeRaw = (int)(value / Constants.LatLongConversionMultiplier); }
    }
    public double Longitude
    {
      get { return (double)LongitudeRaw * Constants.LatLongConversionMultiplier; }
      set { LongitudeRaw = (int)(value / Constants.LatLongConversionMultiplier); }
    }

    #region Implementation of ISpeedHeading

    public Double SpeedMPH
    {
      get { return (double)SpeedMPHRaw * Constants.SpeedConversionMultiplier; }
      set { SpeedMPHRaw = (byte)(value / Constants.SpeedConversionMultiplier); }
    }

    public Double Track
    {
      get { return (double)TrackRaw * Constants.HeadingConversionMultiplier; }
      set { TrackRaw = (sbyte)(value / Constants.HeadingConversionMultiplier); }
    }


    #endregion

    public LocationAgeUnitEnum LocationAgeUnit
    {
      get
      {
        return (LocationAgeUnitEnum)LocationAgeUnitRaw;
      }
      set
      {
        LocationAgeUnitRaw = (byte)value;
      }
    }

    public TimeSpan? LocationAge
    {
      get
      {
        if (LocationAgeRaw == byte.MaxValue)
          return null;
        if (LocationAgeUnit == LocationAgeUnitEnum.Days)
        {
          return TimeSpan.FromDays(LocationAgeRaw);
        }
        else if (LocationAgeUnit == LocationAgeUnitEnum.Hours)
        {
          return TimeSpan.FromHours(LocationAgeRaw);
        }
        else if (LocationAgeUnit == LocationAgeUnitEnum.Minutes)
        {
          return TimeSpan.FromMinutes(LocationAgeRaw);
        }
        else
        {
          return TimeSpan.FromSeconds(LocationAgeRaw);
        }
      }
      set
      {
        if (value == null)
        {
          LocationAgeRaw = byte.MaxValue;
          return;
        }
        if (LocationAgeUnit == LocationAgeUnitEnum.Days)
        {
          LocationAgeRaw = (byte)value.Value.TotalDays;
        }
        else if (LocationAgeUnit == LocationAgeUnitEnum.Hours)
        {
          LocationAgeRaw = (byte)value.Value.TotalHours;
        }
        else if (LocationAgeUnit == LocationAgeUnitEnum.Minutes)
        {
          LocationAgeRaw = (byte)value.Value.TotalMinutes;
        }
        else
        {
          LocationAgeRaw = (byte)value.Value.TotalSeconds;
        }
      }
    }

    public LocationUncertaintyUnitEnum LocationUncertaintyUnit
    {
      get
      {
        return (LocationUncertaintyUnitEnum)LocationUncertaintyUnitRaw;
      }
      set
      {
        LocationUncertaintyUnitRaw = (byte)value;
      }
    }
    public byte LocationUncertainty 
    { 
      get 
      { 
        return LocationUncertaintyRaw; 
      } 
      set 
      { 
        LocationUncertaintyRaw = value; 
      } 
    }

    public double? MilesTraveled
    {
      get 
      {
        if (MilesTraveledRaw == 0xFFFFFF)
          return null;

        return (double)MilesTraveledRaw * Constants.EUDDistanceTraveledConversionMultiplier; 
      }
      set 
      { 
        if(value.HasValue)
          MilesTraveledRaw = (UInt32)(value / Constants.EUDDistanceTraveledConversionMultiplier); 
        else
          MilesTraveledRaw = 0xFFFFFF; 
      }
    }

    public MachineEventBlock[] Blocks;

    private byte DevicePacketSequenceIDRaw;
    private uint SecondsElapsed;
    private uint ServiceMeterHoursRaw;
    private int LatitudeRaw;
    private int LongitudeRaw;
    private byte SpeedMPHRaw;
    private sbyte TrackRaw;
    private byte LocationAgeUnitRaw;
    private byte LocationAgeRaw;
    private byte LocationUncertaintyUnitRaw;
    private byte LocationUncertaintyRaw;
    private UInt32 MilesTraveledRaw;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref DevicePacketSequenceIDRaw);
      serializer(action, raw, ref bitPosition, 32, ref SecondsElapsed);
      serializer(action, raw, ref bitPosition, 32, ref ServiceMeterHoursRaw);
      serializer(action, raw, ref bitPosition, 24, ref LatitudeRaw);
      serializer(action, raw, ref bitPosition, 25, ref LongitudeRaw);
      serializer(action, raw, ref bitPosition, 24, ref MilesTraveledRaw);
      serializer(action, raw, ref bitPosition, 8, ref SpeedMPHRaw);
      serializer(action, raw, ref bitPosition, 7, ref TrackRaw);
      serializer(action, raw, ref bitPosition, 2, ref LocationAgeUnitRaw);
      serializer(action, raw, ref bitPosition, 8, ref LocationAgeRaw);
      serializer(action, raw, ref bitPosition, 2, ref LocationUncertaintyUnitRaw);
      serializer(action, raw, ref bitPosition, 8, ref LocationUncertaintyRaw);
      
      try
      {
        byte count = (Blocks == null) ? (byte)0 : (byte)Blocks.Length;
        
        serializer(action, raw, ref bitPosition, 8, ref count);

        if (action == SerializationAction.Hydrate)
          Blocks = new MachineEventBlock[count];

        Blocks = (MachineEventBlock[])serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 0, Blocks, typeof(MachineEventBlock));

      }
      catch (Exception)
      {

        hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;
      }
    }

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
      get { return typeof(MachineEventAck); }
    }

    public ProtocolMessage GenerateAcknowledgement()
    {
      MachineEventAck ack = new MachineEventAck();

      ack.DevicePacketSequenceID = DevicePacketSequenceID;

      return ack;
    }

    public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
    {
      MachineEventAck ack = ackMessage as MachineEventAck;

      return ack != null && ack.DevicePacketSequenceID == DevicePacketSequenceID;
    }

    #endregion

    public List<MachineEventMessage> GetSingleMachineEventMessages()
    {
      List<MachineEventMessage> singleMachineEventBlocks = new List<MachineEventMessage>();
      if (this.Blocks.Count() == 0)
      {
        return new List<MachineEventMessage>() { this };
      }
      foreach (MachineEventBlock block in this.Blocks)
      {
        MachineEventMessage newMsg = new MachineEventMessage();

        newMsg.DevicePacketSequenceIDRaw = this.DevicePacketSequenceIDRaw;
        newMsg.SecondsElapsed = this.SecondsElapsed;
        newMsg.ServiceMeterHoursRaw = this.ServiceMeterHoursRaw;
        newMsg.LatitudeRaw = this.LatitudeRaw;
        newMsg.LongitudeRaw = this.LongitudeRaw;
        newMsg.MilesTraveledRaw = this.MilesTraveledRaw;
        newMsg.SpeedMPHRaw = this.SpeedMPHRaw;
        newMsg.TrackRaw = this.TrackRaw;
        newMsg.LocationAgeUnitRaw = this.LocationAgeUnitRaw;
        newMsg.LocationAgeRaw = this.LocationAgeRaw;
        newMsg.LocationUncertaintyUnitRaw = this.LocationUncertaintyUnitRaw;
        newMsg.LocationUncertaintyRaw = this.LocationUncertaintyRaw;
        newMsg.Blocks = new MachineEventBlock[1] { block };

        singleMachineEventBlocks.Add(newMsg);
      }

      return singleMachineEventBlocks;
    }

    #region "Unconverted value properties for OEM Data Feed"
    public uint MilesTravelledUnConverted
    {
      get { return MilesTraveledRaw; }
    }
    
    public sbyte TrackUnConverted
    {
      get { return TrackRaw; }
    }

    public uint SpeedMPHUnConverted
    {
      get { return SpeedMPHRaw; }
    }

    public int LatitudeUnConverted
    {
      get { return LatitudeRaw; }
    }

    public int LongitudeUnConverted
    {
      get { return LongitudeRaw; }
    }

    public uint ServiceMeterHoursUnConverted
    {
      get { return ServiceMeterHoursRaw; }
    }

    public uint DateTimeUtcUnConverted
    {
      get { return SecondsElapsed; }
    }

    public byte LocactionAgeUnConverted
    {
      get { return LocationAgeRaw; }
    }

    #endregion

  }

  public class MachineEventAck : BaseMessage, TrackerMessage.IDeviceSequenceID
  {
    public static new readonly int kPacketID = 0x16;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      // Now for my members

      serializer(action, raw, ref bitPosition, 8, ref MachineEventSequenceIDRaw);
    }

    public byte MachineEventSequenceIDRaw;

    #region Implementation of IDeviceSequenceID

    public byte DevicePacketSequenceID
    {
      get { return MachineEventSequenceIDRaw; }
      set { MachineEventSequenceIDRaw = value; }
    }

    #endregion
  }
}
