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

    public bool HeaderRequired = true;
    public bool HeaderUpdated = false; // has there been a time epoch for header

    public bool TrailerRequired = false;
    public EpochRecord EpochRec = new EpochRecord(); // current epoch record
    public TagContentList TagContent; // tagfile data content
    public ILogger Log;

    /// <summary>
    /// Constructor
    /// </summary>
    public AsciiParser()
    {
      TagValue = "";
      TagName = "";
      TagContent = new TagContentList();
      //   EpochDict = new EpochDictionary();
    }


    private bool ValidateText(ref byte[] ba)
    {
      if (ba.Length <= 4)
      {
        // todo log error
        return false;
      }

      // Should start with record seperator
      if (ba[0] != TagConstants.RS)
      {
        // todo log error
        return false;
      }

      return true;

    }


    /// <summary>
    /// Checks we have enough input data to create a tagfile epoch
    /// </summary>
    public void UpdateTagContentList()
    {
      // if (!EpochRec.HasTime)
      //  return;

      var timeAdded = false;

      if (!HeaderUpdated & !EpochRec.HasMTP) // dont process any epoch before recieving a header
      {
        EpochRec.ClearEpoch();
        return;
      }

      if (!HeaderUpdated)
      {
        if (EpochRec.HasTime)
        {
          TagContent.AddTimeEntry(new TagData_UnsignedInt() { Data = EpochRec.Time });
          TagContent.AddWeekEntry(new TagData_UnsignedInt() { Data = EpochRec.Week });
          EpochRec.HasTime = false; // reset
          HeaderRecordCount++;
          HeaderUpdated = true;
          timeAdded = true;

          // Some fields are defaulted by Trimble set them up here now
          // todo add these to appsettings as defaults
          EpochRec.CoordSys = 3; // coordinate system
          EpochRec.MappingMode = 1; // min elevation
          EpochRec.RadioType = "torch"; // torch
          EpochRec.AppVersion = "1"; // app version
          EpochRec.ValidPosition = 1; // has valid position

        }
      }

      if (EpochRec.HasCoordSys)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.CoordSys, DataType = TAGDataType.t4bitUInt, Data = EpochRec.CoordSys });
        EpochRec.HasCoordSys = false;
      }
      if (EpochRec.HasValidPosition)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.ValidPosition, DataType = TAGDataType.t4bitUInt, Data = EpochRec.ValidPosition });
        EpochRec.HasValidPosition = false;
      }
      if (EpochRec.HasMappingMode)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.MappingMode, DataType = TAGDataType.t8bitUInt, Data = EpochRec.MappingMode });
        EpochRec.HasMappingMode = false;
      }

      if (EpochRec.HasDES)
      {
        TagContent.AddEntry(new TagData_Unicode() { DictID = (short)DictionaryItem.Design, DataType = TAGDataType.tUnicodeString, Data = EpochRec.Design });
        EpochRec.HasDES = false;
      }
      if (EpochRec.HasLAT)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Latitude, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.LAT });
        EpochRec.HasLAT = false;
        HeaderRecordCount++;
      }
      if (EpochRec.HasLON)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Longtitude, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.LON });
        EpochRec.HasLON = false;
        HeaderRecordCount++;
      }
      if (EpochRec.HasHGT)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.height, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.HGT });
        EpochRec.HasHGT = false;
      }

      if (EpochRec.HasUTM)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.UTMZone, DataType = TAGDataType.t8bitUInt, Data = EpochRec.UTM });
        EpochRec.HasUTM = false;
      }

      HeaderRequired = HeaderRecordCount < 3; // do we have the key main header values

      // };

      if (EpochRec.HasTime & !timeAdded)
      {
        if (EpochRec.HasDeltaTime)
          TagContent.AddTimeDeltaEntry(new TagData_UnsignedInt() { Data = EpochRec.DeltaTime });
        else
          TagContent.AddTimeEntry(new TagData_UnsignedInt() { Data = EpochRec.Time });
        EpochRec.HasDeltaTime = false;
        EpochRec.HasTime = false; // reset
      }

      if (EpochRec.HasLEB || EpochRec.HasLNB || EpochRec.HasLHB)
      {
        TagContent.AddEntry(new TagData_Empty() { DictID = (short)DictionaryItem.Left, DataType = TAGDataType.tEmptyType });
        if (EpochRec.HasLEB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Easting, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.LEB });
          EpochRec.HasLEB = false;
        }
        if (EpochRec.HasLNB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Northing, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.LNB });
          EpochRec.HasLNB = false;
        }
        if (EpochRec.HasLHB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Elevation, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.LHB });
          EpochRec.HasLHB = false;
        }
      }

      if (EpochRec.HasREB || EpochRec.HasRNB || EpochRec.HasRHB)
      {
        TagContent.AddEntry(new TagData_Empty() { DictID = (short)DictionaryItem.Right, DataType = TAGDataType.tEmptyType });
        if (EpochRec.HasREB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Easting, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.REB });
          EpochRec.HasREB = false;
        }
        if (EpochRec.HasRNB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Northing, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.RNB });
          EpochRec.HasRNB = false;
        }
        if (EpochRec.HasRHB)
        {
          TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.Elevation, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.RHB });
          EpochRec.HasRHB = false;
        }
      }

      if (EpochRec.HasHDG)
      {
        // special field todo. Heading does not go to tagfile. will go somewhere else eventually 
        EpochRec.HasHDG = false;
      }

      if (EpochRec.HasMSD)
      {
        TagContent.AddEntry(new TagData_Double() { DictID = (short)DictionaryItem.MachineSpeed, DataType = TAGDataType.tIEEEDouble, Data = EpochRec.MSD });
        EpochRec.HasMSD = false;
      }
      if (EpochRec.HasGPM)
      {
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.GPSMode, DataType = TAGDataType.t4bitUInt, Data = EpochRec.GPM });
        EpochRec.HasGPM = false;
      }
      if (EpochRec.HasBOG)
      {
        // writes On_GROUND and BLADE_ON_GROUND together
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.OG, DataType = TAGDataType.t4bitUInt, Data = EpochRec.BOG });
        TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.BOG, DataType = TAGDataType.t4bitUInt, Data = EpochRec.BOG });
        EpochRec.HasBOG = false;
      }

      if (TrailerRequired)
      {
        if (EpochRec.HasRadioSerial)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.RadioSerial, DataType = TAGDataType.tANSIString, Data = EpochRec.RadioSerial });
          EpochRec.HasRadioSerial = false;
        }
        if (EpochRec.HasRadioType)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.RadioType, DataType = TAGDataType.tANSIString, Data = EpochRec.RadioType });
          EpochRec.HasRadioType = false;
        }
        if (EpochRec.HasSER)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.FileSerial, DataType = TAGDataType.tANSIString, Data = EpochRec.Serial });
          EpochRec.HasSER = false;
        }
        if (EpochRec.HasMID)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.MachineID, DataType = TAGDataType.tANSIString, Data = EpochRec.MID });
          EpochRec.HasMID = false;
        }
        if (EpochRec.HasAppVersion)
        {
          TagContent.AddEntry(new TagData_String() { DictID = (short)DictionaryItem.ApplicationVersion, DataType = TAGDataType.tANSIString, Data = EpochRec.AppVersion });
          EpochRec.HasAppVersion = false;
        }
        if (EpochRec.HasMTP)
        {
          TagContent.AddEntry(new TagData_UnsignedInt() { DictID = (short)DictionaryItem.MachineType, DataType = TAGDataType.t8bitUInt, Data = EpochRec.MTP });
          EpochRec.HasMTP = false;
        }

      }

    }

    /// <summary>
    /// Process field from datapacket
    /// </summary>
    private void ProcessField()
    {
      try
      {

        switch (TagName)
        {
          case TagConstants.TIME:
            {
              //  if (EpochRec.HasTime) // if we have an existing timestamp then save that epoch to list before continuing
              //  SaveEpochToList();

              // Starts new epoch
              uint gpsTime;
              uint gpsWeek;

              // Comes in as UTC unix timestamp
              //   var utcTime = TagUtils.UnixTimeStampToUTCDateTime(Convert.ToInt64(TagValue));
              var utcTime = TagUtils.DateTimeFromUnixTimestampMillis(Convert.ToInt64(TagValue));
              GPS.DateTimeToGPSOriginTime(utcTime, out gpsWeek, out gpsTime);
              EpochRec.Time = gpsTime;
              EpochRec.Week = gpsWeek;
              break;
            }
          case TagConstants.LEFT_EASTING_BLADE:
            {
              if (EpochRec.HasLEB)
                Log.LogWarning("Already have LEB value for epoch");
              EpochRec.LEB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.LEFT_NORTHING_BLADE:
            {
              if (EpochRec.HasLNB)
                Log.LogWarning("Already have LNB value for epoch");
              EpochRec.LNB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.LEFT_HEIGHT_BLADE:
            {
              if (EpochRec.HasLHB)
                Log.LogWarning("Already have LHB value for epoch");
              EpochRec.LHB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.RIGHT_EASTING_BLADE:
            {
              if (EpochRec.HasREB)
                Log.LogWarning("Already have REB value for epoch");
              EpochRec.REB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.RIGHT_NORTHING_BLADE:
            {
              if (EpochRec.HasRNB)
                Log.LogWarning("Already have RNB value for epoch");
              EpochRec.RNB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.RIGHT_HEIGHT_BLADE:
            {
              if (EpochRec.HasRHB)
                Log.LogWarning("Already have RHB value for epoch");
              EpochRec.RHB = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.GPS_MODE:
            {
              if (EpochRec.HasGPM)
                Log.LogWarning("Already have GPM value for epoch");
              EpochRec.GPM = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.BLADE_ON_GROUND:
            {
              ushort val = Convert.ToUInt16(TagValue);
              if (EpochRec.HasPrevBOG & val == EpochRec.PREV_BOG)
                break; // no change so dont record
              if (EpochRec.HasBOG)
                Log.LogWarning("Already have BOG value for epoch");
              EpochRec.BOG = Convert.ToUInt16(TagValue);
              break;
            }
          case TagConstants.DESIGN:
            {
              if (EpochRec.HasDES)
                Log.LogWarning("Already have DES value for epoch");
              EpochRec.Design = TagValue;
              break;
            }
          case TagConstants.LATITUDE:
            {
              if (EpochRec.HasLAT)
                Log.LogWarning("Already have LAT value for epoch");
              EpochRec.LAT = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.LONTITUDE:
            {
              if (EpochRec.HasLON)
                Log.LogWarning("Already have LON value for epoch");
              EpochRec.LON = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.HEIGHT:
            {
              if (EpochRec.HasHGT)
                Log.LogWarning("Already have HGT value for epoch");
              EpochRec.HGT = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.MESSAGE_ID:
            {
              if (EpochRec.HasMID)
                Log.LogWarning("Already have MID value for epoch");
              EpochRec.MID = TagValue;
              break;
            }
          case TagConstants.MACHINE_SPEED:
            {
              if (EpochRec.HasMSD)
                Log.LogWarning("Already have MSD value for epoch");
              EpochRec.MSD = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.MACHINE_TYPE:
            {
              if (EpochRec.HasMTP)
                Log.LogWarning("Already have MTP value for epoch");
              EpochRec.MTP = TagUtils.ConvertToMachineType(TagValue);
              break;
            }
          case TagConstants.HEADING:
            {
              if (EpochRec.HasHDG)
                Log.LogWarning("Already have HDG value for epoch");
              EpochRec.HDG = Convert.ToDouble(TagValue);
              break;
            }
          case TagConstants.SERIAL:
            {
              if (EpochRec.HasSER)
                Log.LogWarning("Already have SER value for epoch");
              EpochRec.Serial = TagValue;
              EpochRec.RadioSerial = TagValue; // Yes assigned two places
              break;
            }
          case TagConstants.UTM:
            {
              if (EpochRec.HasUTM)
                Log.LogWarning("Already have UTM value for epoch");
              EpochRec.UTM = Convert.ToByte(TagValue);
              EpochRec.RadioSerial = TagValue; // Yes assigned two places
              break;
            }
          default:
            Log.LogWarning($"ProcessField. Unknown TagName:{TagName} Value:{TagValue}");
            break;
        }

      }
      catch (Exception ex)
      {
        Log.LogError($"Unexpected error in ProcessField. TagName:{TagName}, Value:{TagValue}, Error:{ex.Message}");
      }
    }


    /// <summary>
    /// Clear all data to start new tagfile
    /// </summary>
    public void Reset()
    {
      //   EpochRec.ClearEpoch();
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
      // Note STX and ETX, ENQ, ACK, NAK should not enter here as a rule. Ignored if it does

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
            //  UpdateTagContentList(); // save current epoch check
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
                  ProcessField();
            }
            continue;
        }

      }

      UpdateTagContentList(); // finally process all fields picked out of the datapacket

      return true;
    }
  }
}
