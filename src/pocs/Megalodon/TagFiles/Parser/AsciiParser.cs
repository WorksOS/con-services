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
    private DateTime prevEpochTime = DateTime.MinValue;
    public bool HeaderRequired = true;
    public bool HeaderUpdated = false; // has there been a time epoch for header

    public bool TrailerRequired = false;
    private EpochRecord _PrevTagFile_EpochRec; // poch record beloging to last tagfile
    private EpochRecord _Prev_EpochRec; // previous epoch record
    public EpochRecord EpochRec = new EpochRecord(); // current epoch record
    public TagContentList TagContent; // tagfile data content
    public ILogger Log;

    // hacks
    public bool ForceBOG = false;
    public double SeedLat = 0;
    public double SeedLon = 0;
    public string ForceSerial = "";

    /// <summary>
    /// Constructor
    /// </summary>
    public AsciiParser()
    {
      TagValue = "";
      TagName = "";
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
    public void UpdateTagContentList(ref EpochRecord eRecord, ref bool newHeader)
    {
      newHeader = false;
      var timeAdded = false;

      if (!HeaderUpdated & !eRecord.HasMTP) // dont process any epoch before recieving a header
        return;

      if (!HeaderUpdated)
      {
        if (eRecord.HasTime)
        {
          TagContent.AddTimeEntry(new TagData_UnsignedInt() { Data = eRecord.Time });
          TagContent.AddWeekEntry(new TagData_UnsignedInt() { Data = eRecord.Week });
          eRecord.HasTime = false; // reset
          HeaderRecordCount++;
          HeaderUpdated = true;
          timeAdded = true;

          // Some fields are defaulted by Trimble set them up here now
          // todo add these to appsettings as defaults
          eRecord.CoordSys = TagConstants.DEFAULT_COORDSYS; // coordinate system
          eRecord.MappingMode = TagConstants.DEFAULT_MAPPINGMODE; // min elevation
          eRecord.RadioType = TagConstants.DEFAULT_RADIOTYPE; // torch
          eRecord.AppVersion = TagConstants.DEFAULT_APPVERSION; // app version
          eRecord.ValidPosition = TagConstants.DEFAULT_VALID_POSITION; // has valid position
          newHeader = true;
        }
      }

      if (eRecord.HasCoordSys)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.CoordSys, DataType = TAGDataType.t4bitUInt, Data = eRecord.CoordSys });
        eRecord.HasCoordSys = false;
      }
      if (eRecord.HasValidPosition)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.ValidPosition, DataType = TAGDataType.t4bitUInt, Data = eRecord.ValidPosition });
        eRecord.HasValidPosition = false;
      }
      if (eRecord.HasMappingMode)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.MappingMode, DataType = TAGDataType.t8bitUInt, Data = eRecord.MappingMode });
        eRecord.HasMappingMode = false;
      }

      if (eRecord.HasDES)
      {
        TagContent.AddEntry(new TagData_Unicode() { DictID = (short)DictionaryItem.Design, DataType = TAGDataType.tUnicodeString, Data = eRecord.Design });
        eRecord.HasDES = false;
      }
      if (eRecord.HasLAT)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Latitude, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LAT });
        eRecord.HasLAT = false;
        HeaderRecordCount++;
      }
      if (eRecord.HasLON)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Longtitude, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LON });
        eRecord.HasLON = false;
        HeaderRecordCount++;
      }
      if (eRecord.HasHGT)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.height, DataType = TAGDataType.tIEEEDouble, Data = eRecord.HGT });
        eRecord.HasHGT = false;
      }
      if (eRecord.HasUTM)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.UTMZone, DataType = TAGDataType.t8bitUInt, Data = eRecord.UTM });
        eRecord.HasUTM = false;
      }

      HeaderRequired = HeaderRecordCount < 3; // do we have the key main header values

      if (eRecord.HasTime & !timeAdded)
      {
        if (eRecord.HasDeltaTime)
          TagContent.AddTimeDeltaEntry(new TagData_UnsignedInt() { Data = eRecord.DeltaTime });
        else
          TagContent.AddTimeEntry(new TagData_UnsignedInt() { Data = eRecord.Time });
        eRecord.HasDeltaTime = false;
        eRecord.HasTime = false; // reset
      }

      if (eRecord.HasLEB || eRecord.HasLNB || eRecord.HasLHB)
      {
        TagContent.AddEntry(new TagData_Empty() { DictID = (short)DictionaryItem.Left, DataType = TAGDataType.tEmptyType });
        if (eRecord.HasLEB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Easting, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LEB });
          eRecord.HasLEB = false;
        }
        if (eRecord.HasLNB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Northing, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LNB });
          eRecord.HasLNB = false;
        }
        if (eRecord.HasLHB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Elevation, DataType = TAGDataType.tIEEEDouble, Data = eRecord.LHB });
          eRecord.HasLHB = false;
        }
      }

      if (eRecord.HasREB || eRecord.HasRNB || eRecord.HasRHB)
      {
        TagContent.AddEntry(new TagData_Empty() { DictID = (short)DictionaryItem.Right, DataType = TAGDataType.tEmptyType });
        if (eRecord.HasREB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Easting, DataType = TAGDataType.tIEEEDouble, Data = eRecord.REB });
          eRecord.HasREB = false;
        }
        if (eRecord.HasRNB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Northing, DataType = TAGDataType.tIEEEDouble, Data = eRecord.RNB });
          eRecord.HasRNB = false;
        }
        if (eRecord.HasRHB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Elevation, DataType = TAGDataType.tIEEEDouble, Data = eRecord.RHB });
          eRecord.HasRHB = false;
        }
      }

      if (eRecord.HasHDG)
      {
        // special field todo. Heading does not go to tagfile. will go somewhere else eventually 
        eRecord.HasHDG = false;
      }

      if (eRecord.HasMSD)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.MachineSpeed, DataType = TAGDataType.tIEEEDouble, Data = eRecord.MSD });
        eRecord.HasMSD = false;
      }
      if (eRecord.HasGPM)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.GPSMode, DataType = TAGDataType.t4bitUInt, Data = eRecord.GPM });
        eRecord.HasGPM = false;
      }
      if (eRecord.HasBOG)
      {
        // writes On_GROUND and BLADE_ON_GROUND together
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.OG, DataType = TAGDataType.t4bitUInt, Data = eRecord.BOG });
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.BOG, DataType = TAGDataType.t4bitUInt, Data = eRecord.BOG });
        eRecord.HasBOG = false;
      }

      if (TrailerRequired)
      {

        // hack
        if (ForceSerial != string.Empty)
        {
          eRecord.RadioSerial = ForceSerial;
          eRecord.Serial = ForceSerial;
        }

        if (eRecord.HasRadioSerial)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.RadioSerial, DataType = TAGDataType.tANSIString, Data = eRecord.RadioSerial });
          eRecord.HasRadioSerial = false;
        }
        if (eRecord.HasRadioType)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.RadioType, DataType = TAGDataType.tANSIString, Data = eRecord.RadioType });
          eRecord.HasRadioType = false;
        }
        if (eRecord.HasSER)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.FileSerial, DataType = TAGDataType.tANSIString, Data = eRecord.Serial });
          eRecord.HasSER = false;
        }
        if (eRecord.HasMID)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.MachineID, DataType = TAGDataType.tANSIString, Data = eRecord.MID });
          eRecord.HasMID = false;
        }
        if (eRecord.HasAppVersion)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.ApplicationVersion, DataType = TAGDataType.tANSIString, Data = eRecord.AppVersion });
          eRecord.HasAppVersion = false;
        }
        if (eRecord.HasMTP)
        {
          TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.MachineType, DataType = TAGDataType.t8bitUInt, Data = eRecord.MTP });
          eRecord.HasMTP = false;
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
              EpochRec.LEB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.LEFT_NORTHING_BLADE:
            {
              EpochRec.LNB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.LEFT_HEIGHT_BLADE:
            {
              EpochRec.LHB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.RIGHT_EASTING_BLADE:
            {
              EpochRec.REB = Convert.ToDouble(TagValue);
              break;
            }

          case TagConstants.RIGHT_NORTHING_BLADE:
            {
              EpochRec.RNB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.RIGHT_HEIGHT_BLADE:
            {
              EpochRec.RHB = Convert.ToDouble(TagValue);
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
              if (EpochRec.HasPrevBOG & val == EpochRec.PREV_BOG)
                break; // no change so dont record
              EpochRec.BOG = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.DESIGN:
            {
              EpochRec.Design = TagValue;
              break;
            }
          case TagConstants.LATITUDE:
            {
              if (SeedLat != 0)
                EpochRec.LAT = SeedLat;
              else
                EpochRec.LAT = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.LONTITUDE:
            {
              if (SeedLon != 0)
                EpochRec.LON = SeedLon;
              else
                EpochRec.LON = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.HEIGHT:
            {
              EpochRec.HGT = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.MACHINE_ID:
            {
              EpochRec.MID = TagValue;
              break;
            }
          case TagConstants.MACHINE_SPEED:
            {
              EpochRec.MSD = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.MACHINE_TYPE:
            {
              EpochRec.MTP = TagUtils.ConvertToMachineType(TagValue);
              break;
            }
          case TagConstants.HEADING:
            {
              EpochRec.HDG = Convert.ToDouble(TagValue);
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
      TagName = "";
      TagValue = "";
      HaveName = false;
      HaveValue = false;
      HeaderRecordCount = 0;
      HeaderRequired = true;
      HeaderUpdated = false;
      TrailerRequired = false;
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

      if (prevEpochTime != DateTime.MinValue)
      {
        // this check is here to prevent two epochs seprated by a min time interval being stitched together
        TimeSpan minSpanAllowed = TimeSpan.FromSeconds(TagConstants.MIN_EPOCH_INTERVAL_SECS);
        TimeSpan currentSpan = DateTime.Now - prevEpochTime;
        if (currentSpan > minSpanAllowed)
        {
          if (_Prev_EpochRec != null)
            _Prev_EpochRec = null; // stops the previous epoch being associated to this epoch
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
              if (HaveName && TagValue != "")
                ProcessField();
              HaveName = false;
              HaveValue = false;
              TagValue = "";
              TagName = "";
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
                  if (HaveName && TagValue != "")
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

        if (_Prev_EpochRec != null)
        {
          // Check if its the same position and elevation
          if (EpochRec.HasLAT) // if header
            UpdateTagContentList(ref EpochRec, ref newHeader);
          else if (EpochRec.NotEpochSamePosition(ref _Prev_EpochRec))
            UpdateTagContentList(ref EpochRec, ref newHeader);
        }
        else
          UpdateTagContentList(ref EpochRec, ref newHeader);

        if (newHeader)
        {
          if (_PrevTagFile_EpochRec != null) // epoch from last tagfile
          {
            if (_PrevTagFile_EpochRec.IsFullPositionEpoch()) // if it is a new tagfile we use last known epoch to start new tagfile
              UpdateTagContentList(ref _PrevTagFile_EpochRec, ref tmpNR);
            _PrevTagFile_EpochRec = null;
          }
          if (_Prev_EpochRec != null) // epoch missed to SOH request
            if (_Prev_EpochRec.IsFullPositionEpoch()) // if it is a new tagfile we use last known epoch to start new tagfile
              UpdateTagContentList(ref _Prev_EpochRec, ref tmpNR);
        }

        _Prev_EpochRec = new EpochRecord();
        _Prev_EpochRec.EpochCopy(ref EpochRec);

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
