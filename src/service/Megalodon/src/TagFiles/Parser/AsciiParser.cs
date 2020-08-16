using System;
using System.Text;
using TagFiles.Types;
using TagFiles.Utils;
using TagFiles.Common;
using Microsoft.Extensions.Logging;

/// <summary>
/// Master class for parsing machine data packets and creating tagfiles for VL
/// </summary>
namespace TagFiles.Parser
{
  public class AsciiParser
  {

    private string TagName;
    private string TagValue;
    private bool HaveName = false;
    private bool HaveValue = false;
    private int HeaderRecordCount = 0;
    private bool tmpNR = false;
    private bool packetIsHeader = false;
    private DateTime prevEpochTime = DateTime.MinValue;
    private int updateCount = 0;
    public int EpochCount = 0;
    public bool HeaderRequired = true;
    public bool HeaderUpdated = false; // has there been a time epoch for header
    public bool DebugTraceToLog = false;

    public bool TrailerRequired = false;
    private EpochRecord _PrevTagFile_EpochRec; // previous epoch belonging to the last tagfile
    public EpochRecord Prev_EpochRec; // previous epoch record in current tagfile
    public EpochRecord EpochRec = new EpochRecord(); // current epoch record
    private EpochRecord LastStateEpochRecord = new EpochRecord(); // maintains the last known value for a tag. Used to prevent writing the same event or field value each time

    public TagContentList TagContent; // tagfile data content
    public ILogger Log;
    public bool NotSeenNewPosition = true;
    public byte TransmissionProtocolVersion = TagConstants.Version1;

    // hacks
    public bool ForceBOG = false;
    public double SeedLat = 0;
    public double SeedLon = 0;
    public string ForceSerial = String.Empty;

    /// <summary>
    /// Constructor
    /// </summary>
    public AsciiParser()
    {
      TagValue = String.Empty;
      TagName = String.Empty;
      TagContent = new TagContentList();
    }

    /// <summary>
    /// Validation for data packet
    /// </summary>
    /// <param name="ba"></param>
    /// <returns></returns>
    private bool ValidateText(ref byte[] ba)
    {
      if (ba.Length <= 4)
      {
        Log.LogWarning("Data packet less than 4 bytes");
        return false;
      }

      // Should start with record seperator
      if (ba[0] != TagConstants.RS)
      {
        Log.LogWarning("Data packet missing record seperator. 0x1E");
        return false;
      }

      // Should start with record seperator
      if (ba[ba.Length - 1] == TagConstants.ETX)
      {
        Log.LogWarning("Data packet to parse should not have ETX as last charartor");
        return false;
      }

      return true;

    }

    /// <summary>
    /// Checks we have enough input data to create a tagfile epoch
    /// </summary>
    public void UpdateTagContentList(ref EpochRecord eRecord, ref bool newHeader, TagConstants.UpdateReason reason)
    {
      if (DebugTraceToLog)
        Log.LogInformation($"* UpdateTagContentList * {reason}"); 

      newHeader = false;
      var timeAdded = false;

      if (!HeaderUpdated & !eRecord.HasMTP) // dont process any epoch before recieving a header
      {
        Log.LogWarning("Epoch ignored before recieving header record"); // should not happen 
        return;
      }

      if (!eRecord.HasUpdateData() && !TrailerRequired)
        return;

      if (reason == TagConstants.UpdateReason.ChangeRecord)
        updateCount++;

      if (!HeaderUpdated)
      {
        if (eRecord.HasTime)
        {
          TagContent.AddTimeEntry(new TagData_UnsignedInt() { Data = eRecord.Time });
          TagContent.AddWeekEntry(new TagData_UnsignedInt() { Data = eRecord.Week });
          eRecord.Time = uint.MaxValue; // reset
          HeaderRecordCount++;
          HeaderUpdated = true;
          timeAdded = true;

          // Some fields are defaulted by Trimble set them up here now
          // todo add these to appsettings as defaults
          eRecord.CoordSys = TagConstants.DEFAULT_COORDSYS; // coordinate system
          if  (eRecord.MappingMode == ushort.MaxValue)
             eRecord.MappingMode = TagConstants.DEFAULT_MAPPINGMODE; // min elevation defaulted if not supplied to suit marine
          eRecord.RadioType = TagConstants.DEFAULT_RADIOTYPE; // torch
          eRecord.AppVersion = TagConstants.DEFAULT_APPVERSION; // app version
          eRecord.ValidPosition = TagConstants.DEFAULT_VALID_POSITION; // has valid position
          newHeader = true;
        }
      }

      if (eRecord.HasCST)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.CST, DataType = TAGDataType.t8bitUInt, Data = eRecord.CST });
        eRecord.CST = uint.MaxValue;
      }
      if (eRecord.HasFlags)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.Flags, DataType = TAGDataType.t4bitUInt, Data = eRecord.Flags });
        eRecord.Flags = uint.MaxValue; ;
      }
      if (eRecord.HasTargetCCV)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.TargetCCV, DataType = TAGDataType.t12bitUInt, Data = eRecord.TargetCCV });
        eRecord.TargetCCV = uint.MaxValue; 
      }
      if (eRecord.HasTargetMDP)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.TargetMDP, DataType = TAGDataType.t12bitUInt, Data = eRecord.TargetMDP });
        eRecord.TargetMDP = uint.MaxValue;
      }
      if (eRecord.HasDirection)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.Direction, DataType = TAGDataType.t4bitUInt, Data = eRecord.Direction});
        eRecord.Direction = uint.MaxValue; 
      }
      if (eRecord.HasTemperature)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.Temperature, DataType = TAGDataType.t12bitUInt, Data = eRecord.Temperature});
        eRecord.Temperature = uint.MaxValue; 
      }
      if (eRecord.HasCoordSys)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.CoordSys, DataType = TAGDataType.t4bitUInt, Data = eRecord.CoordSys });
        eRecord.CoordSys = ushort.MaxValue; ;
      }
      if (eRecord.HasValidPosition)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.ValidPosition, DataType = TAGDataType.t4bitUInt, Data = eRecord.ValidPosition });
        eRecord.ValidPosition = ushort.MaxValue; 
      }
      if (eRecord.HasMappingMode)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.MappingMode, DataType = TAGDataType.t8bitUInt, Data = eRecord.MappingMode });
        eRecord.MappingMode = ushort.MaxValue; ;
      }
      if (eRecord.HasDesign)
      {
        TagContent.AddEntry(new TagData_Unicode() { DictID = (short)DictionaryItem.Design, DataType = TAGDataType.tUnicodeString, Data = eRecord.Design });
        eRecord.Design = String.Empty;
      }
      if (eRecord.HasLAT)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Latitude, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LAT });
        eRecord.LAT = double.MaxValue; 
        HeaderRecordCount++;
      }
      if (eRecord.HasLON)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Longtitude, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LON });
        eRecord.LON = double.MaxValue; 
        HeaderRecordCount++;
      }
      if (eRecord.HasHGT)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.height, DataType = TAGDataType.tIEEEDouble, Data = eRecord.HGT });
        eRecord.HGT = double.MaxValue;
      }
      if (eRecord.HasUTM)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.UTMZone, DataType = TAGDataType.t8bitUInt, Data = eRecord.UTM });
        eRecord.UTM = byte.MaxValue;
      }
      if (eRecord.HasTargetThickness)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.TargetThickness, DataType = TAGDataType.t16bitUInt, Data = eRecord.TargetThickness });
        eRecord.TargetThickness = uint.MaxValue;
      }
      if (eRecord.HasTargetPasses)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.TargetPasses, DataType = TAGDataType.t12bitUInt, Data = eRecord.TargetPasses });
        eRecord.TargetPasses = uint.MaxValue;
      }
      if (eRecord.HasTempMin)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.TempMin, DataType = TAGDataType.t12bitUInt, Data = eRecord.TempMin });
        eRecord.TempMin = uint.MaxValue; 
      }
      if (eRecord.HasTempMax)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.TempMax, DataType = TAGDataType.t12bitUInt, Data = eRecord.TempMax });
        eRecord.TempMax = uint.MaxValue; 
      }
      if (TransmissionProtocolVersion < TagConstants.Version1) 
        HeaderRequired = HeaderRecordCount < 3; // do we have the key main header values
      else 
        HeaderRequired = !eRecord.HasHeader;

      if (eRecord.HasTime & !timeAdded)
      {
        if (eRecord.HasDeltaTime)
          TagContent.AddTimeDeltaEntry(new TagData_UnsignedInt() { Data = eRecord.DeltaTime });
        else
          TagContent.AddTimeEntry(new TagData_UnsignedInt() { Data = eRecord.Time });
        eRecord.DeltaTime = uint.MaxValue; 
        eRecord.Time = uint.MaxValue;  // reset
        EpochCount++;
      }

      if (eRecord.HasLEB || eRecord.HasLNB || eRecord.HasLHB)
      {
        TagContent.AddEntry(new TagData_Empty() { DictID = (short)DictionaryItem.Left, DataType = TAGDataType.tEmptyType });
        if (eRecord.HasLEB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Easting, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LEB });
          eRecord.LEB = double.MaxValue;
        }
        if (eRecord.HasLNB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Northing, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LNB });
          eRecord.LNB = double.MaxValue;
        }
        if (eRecord.HasLHB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Elevation, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LHB });
          eRecord.LHB = double.MaxValue;
        }
      }
      if (eRecord.HasREB || eRecord.HasRNB || eRecord.HasRHB)
      {
        TagContent.AddEntry(new TagData_Empty() { DictID = (short)DictionaryItem.Right, DataType = TAGDataType.tEmptyType });
        if (eRecord.HasREB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Easting, DataType = TAGDataType.tIEEEDouble, Data = eRecord.REB });
          eRecord.REB = double.MaxValue;
        }
        if (eRecord.HasRNB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Northing, DataType = TAGDataType.tIEEEDouble, Data = eRecord.RNB });
          eRecord.RNB = double.MaxValue;
        }
        if (eRecord.HasRHB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Elevation, DataType = TAGDataType.tIEEEDouble, Data = eRecord.RHB });
          eRecord.RHB = double.MaxValue;
        }
      }

      if (eRecord.HasHDG)
      {
        // special field todo. Heading does not go to tagfile. will go somewhere else eventually 
        eRecord.HDG = double.MaxValue;
      }

      if (eRecord.HasMSD)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.MachineSpeed, DataType = TAGDataType.tIEEEDouble, Data = eRecord.MSD });
        eRecord.MSD = double.MaxValue;
      }
      if (eRecord.HasGPM)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.GPSMode, DataType = TAGDataType.t4bitUInt, Data = eRecord.GPM });
        eRecord.GPM = ushort.MaxValue;
      }
      if (eRecord.HasBOG)
      {
        // writes On_GROUND and BLADE_ON_GROUND together
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.OG, DataType = TAGDataType.t4bitUInt, Data = eRecord.BOG });
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.BOG, DataType = TAGDataType.t4bitUInt, Data = eRecord.BOG });
        eRecord.BOG = ushort.MaxValue;
      }
      if (eRecord.HasCCV)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.CCV, DataType = TAGDataType.t12bitUInt, Data = eRecord.CCV });
        eRecord.CCV = uint.MaxValue;
      }
      if (eRecord.HasMDP)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.MDP, DataType = TAGDataType.t12bitUInt, Data = eRecord.MDP });
        eRecord.MDP = uint.MaxValue;
      }

      if (TrailerRequired)
      {
        
        if (ForceSerial != string.Empty)
        {
          eRecord.RadioSerial = ForceSerial;
          eRecord.Serial = ForceSerial;
        }

        if (eRecord.HasRadioSerial)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.RadioSerial, DataType = TAGDataType.tANSIString, Data = eRecord.RadioSerial });
          eRecord.RadioSerial = String.Empty;
        }
        if (eRecord.HasRadioType)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.RadioType, DataType = TAGDataType.tANSIString, Data = eRecord.RadioType });
          eRecord.RadioType = String.Empty;
        }
        if (eRecord.HasSerial)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.FileSerial, DataType = TAGDataType.tANSIString, Data = eRecord.Serial });
          eRecord.Serial = String.Empty;
        }
        if (eRecord.HasMID)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.MachineID, DataType = TAGDataType.tANSIString, Data = eRecord.MID });
          eRecord.MID = String.Empty;
        }
        if (eRecord.HasAppVersion)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.ApplicationVersion, DataType = TAGDataType.tANSIString, Data = eRecord.AppVersion });
          eRecord.AppVersion = String.Empty;
        }
        if (eRecord.HasMTP)
        {
          TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.MachineType, DataType = TAGDataType.t8bitUInt, Data = eRecord.MTP });
          eRecord.MTP = byte.MaxValue;
        }

      }
    }

    /// <summary>
    /// Process field from datapacket
    /// </summary>
    private bool ProcessField()
    {
      try
      {

        switch (TagName)
        {
          case TagConstants.TIME:
            {
              // Starts new epoch
              uint gpsTime;
              uint gpsWeek;
              var utcTime = TagUtils.DateTimeFromUnixTimestampMillis(Convert.ToInt64(TagValue));
              GPS.DateTimeToGPSOriginTime(utcTime, out gpsWeek, out gpsTime);
              EpochRec.Time = gpsTime;
              EpochRec.Week = gpsWeek;
              break;
            }
          case TagConstants.LEFT_EASTING_BLADE:
            {
              var leb = Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasLEB & leb == Prev_EpochRec.LEB)
                  break;
              }
              EpochRec.LEB = leb;
              break;
            }
          case TagConstants.LEFT_NORTHING_BLADE:
            {
              var lnb = Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasLNB & lnb == Prev_EpochRec.LNB)
                  break;
              }
              EpochRec.LNB = lnb;
              break;
            }
          case TagConstants.LEFT_HEIGHT_BLADE:
            {
              var lhb = Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasLHB & lhb == Prev_EpochRec.LHB)
                  break;
              }
              EpochRec.LHB = lhb;
              break;
            }
          case TagConstants.RIGHT_EASTING_BLADE:
            {
              var reb = Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasREB & reb == Prev_EpochRec.REB)
                  break;
              }
              EpochRec.REB = reb;
              break;
            }
          case TagConstants.RIGHT_NORTHING_BLADE:
            {
              var rnb = Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasRNB & rnb == Prev_EpochRec.RNB)
                  break;
              }
              EpochRec.RNB = rnb;
              break;
            }
          case TagConstants.RIGHT_HEIGHT_BLADE:
            {
              var rhb = Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasRHB & rhb == Prev_EpochRec.RHB)
                  break;
              }
              EpochRec.RHB = rhb;
              break;
            }
          case TagConstants.GPS_MODE:
            {
              EpochRec.GPM = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.BLADE_ON_GROUND:
            {
              ushort val = Convert.ToUInt16(TagValue);
              if (Prev_EpochRec != null) 
              {
                if (Prev_EpochRec.HasBOG & val == Prev_EpochRec.BOG)
                  break; 
              }
              EpochRec.BOG = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.DESIGN:
            {
              var design = TagValue;
              if (Prev_EpochRec != null) 
              {
                if (Prev_EpochRec.HasDesign & design == Prev_EpochRec.Design)
                  break; 
              }
              EpochRec.Design = design;
              break;
            }
          case TagConstants.LATITUDE:
            {
              var latt = (SeedLat != 0) ? SeedLat : Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null) 
              {
                if (Prev_EpochRec.HasLAT & latt == Prev_EpochRec.LAT)
                  break; 
              }
              EpochRec.LAT = latt;
              break;
            }
          case TagConstants.LONTITUDE:
            {
              var lng = (SeedLon != 0) ? SeedLon : Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasLON & lng == Prev_EpochRec.LON)
                  break;
              }
              EpochRec.LON = lng;
              break;
            }
          case TagConstants.HEIGHT:
            {
              var hgt = Convert.ToDouble(TagValue); 
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasHGT & hgt == Prev_EpochRec.HGT)
                  break;
              }
              EpochRec.HGT = hgt;
              break;
            }
          case TagConstants.MACHINE_ID:
            {
              EpochRec.MID = TagValue;
              break;
            }
          case TagConstants.MACHINE_SPEED:
            {
              var spd = Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasMSD & spd == Prev_EpochRec.MSD)
                  break;
              }
              EpochRec.MSD = spd;
              break;
            }
          case TagConstants.MACHINE_TYPE:
            {
              EpochRec.MTP = TagUtils.ConvertToMachineType(TagValue);
              break;
            }
          case TagConstants.HEADING:
            {
              var hdg = Convert.ToDouble(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasHDG & hdg == Prev_EpochRec.HDG)
                  break;
              }
              EpochRec.HDG = hdg;
              break;
            }
          case TagConstants.SERIAL:
            {
              EpochRec.Serial = TagValue;
              EpochRec.RadioSerial = TagValue; // Yes assigned two places
              break;
            }
          case TagConstants.UTM:
            {
              EpochRec.UTM = Convert.ToByte(TagValue);
              break;
            }
          case TagConstants.HDR:
            {
              if (Convert.ToByte(TagValue) == TagConstants.HEADER_RECORD)
              {
                EpochRec.HasHeader = true;
                packetIsHeader = true;
              }
              break;
            }
          case TagConstants.CCV:
            {
              var ccv = Convert.ToUInt16(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasCCV & ccv == Prev_EpochRec.CCV)
                  break;
              }
              EpochRec.CCV = ccv;
              break;
            }
          case TagConstants.MAPPING_MODE:
            {
              EpochRec.MappingMode = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.TARGET_CCV:
            {
              EpochRec.TargetCCV = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.TEMPERATURE:
            {
              var tmp = Convert.ToUInt16(TagValue);
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasTemperature & tmp == Prev_EpochRec.Temperature)
                  break;
              }
              EpochRec.Temperature = tmp;
              break;
            }
          case TagConstants.COMPACT_SENSOR_TYPE:
            {
              EpochRec.CST = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.FLAGS:
            {
              EpochRec.Flags = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.DIRECTION:
            {
              EpochRec.Direction = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.MDP:
            {
              var mdp = Convert.ToUInt16(TagValue); 
              if (Prev_EpochRec != null)
              {
                if (Prev_EpochRec.HasMDP & mdp == Prev_EpochRec.MDP)
                  break;
              }
              EpochRec.MDP = mdp;
              break;
            }
          case TagConstants.TARGET_MDP:
            {
              EpochRec.TargetMDP = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.TARGET_PASSCOUNT:
            {
              EpochRec.TargetPasses = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.TARGET_THICKNESS:
            {
              EpochRec.TargetThickness = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.TEMP_MIN:
            {
              EpochRec.TempMin = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.TEMP_MAX:
            {
              EpochRec.TempMax = Convert.ToUInt16(TagValue);
              break;
            }


          default:
            Log.LogWarning($"ProcessField. Unknown TagName:{TagName} Value:{TagValue}");
            break;
        }

      }
      catch (Exception ex)
      {
        Log.LogError($"Unexpected error in ProcessField. TagName:{TagName}, Value:{TagValue}, Error:{ex.Message.ToString()} Trace:{ex.StackTrace.ToString()}");
        return false;
      }

      return true;
    }


    /// <summary>
    /// Clear all data to start new tagfile
    /// </summary>
    public void Reset()
    {
      EpochRec = new EpochRecord();
      TagName = String.Empty;
      TagValue = String.Empty;
      HaveName = false;
      HaveValue = false;
      HeaderRecordCount = 0;
      HeaderRequired = true;
      HeaderUpdated = false;
      TrailerRequired = false;
      updateCount = 0;
      EpochCount = 0;
      TagContent = new TagContentList(); // zero list
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="txt">Each field from data packet should be in the order RS+Name+Value</param>
    /// <returns></returns>
    public bool ParseText(string txt)
    {
      // Note STX and ETX, ENQ, ACK, NAK should not enter here as a rule. 

      packetIsHeader = false;

      // If debugging (causing a ide delay) this can be commented out
     
      if (prevEpochTime != DateTime.MinValue)
      {
        // this check is here to prevent two epochs seprated by a min time interval being stitched together
        TimeSpan minSpanAllowed = TimeSpan.FromSeconds(TagConstants.MIN_EPOCH_INTERVAL_SECS);
        TimeSpan currentSpan = DateTime.Now - prevEpochTime;
        if (currentSpan > minSpanAllowed)
        {
          if (Prev_EpochRec != null)
            Prev_EpochRec = null; // stops the previous epoch being associated to this epoch
          if (_PrevTagFile_EpochRec != null)
            _PrevTagFile_EpochRec = null;
        }
      }
      

      prevEpochTime = DateTime.Now;

      try
      {
        byte[] bArray = Encoding.UTF8.GetBytes(txt);
        if (!ValidateText(ref bArray))
        {
          return false;
        }

        HaveName = false;
        HaveValue = false;

        // construct records(tagfile fields) byte by byte
        for (int i = 0; i < bArray.Length; i++)
        {
          switch (bArray[i])
          {
            case TagConstants.STX:
              continue;
            case TagConstants.ETX:
              continue;
            case TagConstants.ENQ:
              continue;
            case TagConstants.ACK:
              continue;
            case TagConstants.NAK:
              continue;
            case TagConstants.RS:
              if (HaveName && TagValue != String.Empty)
                ProcessField();
              HaveName = false;
              HaveValue = false;
              TagValue = String.Empty;
              TagName = String.Empty;
              continue;
            default:
              if (!HaveName)
              {
                TagName = TagName + (char)bArray[i];
                HaveName = TagName.Length == TagConstants.TAG_NAME_LENGHT;
              }
              else
              {
                TagValue = TagValue + (char)bArray[i];
                if (!HaveValue)
                  HaveValue = true;
                if (HaveName && i == bArray.Length - 1) // endoftext
                  if (HaveName && TagValue != String.Empty)
                  {
                    var res = ProcessField();
                    if (res == false)
                      return false;
                  }
              }
              continue;
          }

        }

        // finally process all fields picked out of the datapacket
        var newHeader = false;

        LastStateEpochRecord.EpochCopyLatestValues(ref EpochRec); // Remeber last valid values for each tag

        if (Prev_EpochRec != null) // test needed
        {
          // Check if its the same position and elevation
          if (packetIsHeader) // if header record
          {
            UpdateTagContentList(ref EpochRec, ref newHeader, TagConstants.UpdateReason.NewHeader);
          }
          else if (EpochRec.MachineStateDifferent(ref Prev_EpochRec))
          {
            UpdateTagContentList(ref EpochRec, ref newHeader, TagConstants.UpdateReason.ChangeRecord);
            if (NotSeenNewPosition && updateCount > 1)
              NotSeenNewPosition = false;
          }
          else Log.LogError($"** Same Position ***");

        }
        else
          if (packetIsHeader)
            UpdateTagContentList(ref EpochRec, ref newHeader, TagConstants.UpdateReason.NewHeader);
          else
            UpdateTagContentList(ref EpochRec, ref newHeader, TagConstants.UpdateReason.ChangeRecord);

        if (newHeader) // Is this the start of a new tagfile
        {
          if (_PrevTagFile_EpochRec != null) // epoch from last tagfile
          {
            if (_PrevTagFile_EpochRec.IsFullPositionEpoch()) // if it is a new tagfile we use last known epoch to start new tagfile
              UpdateTagContentList(ref _PrevTagFile_EpochRec, ref tmpNR, TagConstants.UpdateReason.LastTagFileEpoch);
            _PrevTagFile_EpochRec = null;
            LastStateEpochRecord.ClearEpoch();
          }
        }
        else
        {
          if (Prev_EpochRec == null)
            Prev_EpochRec = new EpochRecord();
          Prev_EpochRec.EpochCopy(ref LastStateEpochRecord);
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError($"Unexpected error occured in ParseText. Error:{e.Message}, {e.StackTrace}");
        return false;
      }
    }

    /// <summary>
    /// Make copy of last epoch to be reused in new tagfile
    /// </summary>
    public void CloneLastEpoch()
    {
      _PrevTagFile_EpochRec = new EpochRecord();
      _PrevTagFile_EpochRec.EpochCopy(ref EpochRec);

    }

  }
}
