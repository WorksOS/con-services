using System;
using System.IO;
using System.Timers;
using TagFiles.Common;
using TagFiles.Parser;
using TagFiles.Types;
using TagFiles.Utils;
using TAGFiles.Common;

namespace TagFiles
{
  public class TagFile
  {

    private readonly TAGDictionary Dictionary = new TAGDictionary();
    private Timer TagTimer = new System.Timers.Timer();

    public TagHeader Header = new TagHeader();
    // Create the state and sink
    public TAGDictionary TagFileDictionary;
    public AsciiParser Parser = new AsciiParser();
    public string TagOutputFolder;
    private UserSettings userSettings;
    
    public TagFile()
    {
      userSettings = new UserSettings();
    }

    /// <summary>
    /// Timer Event for closing tagfile
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
      if (Parser.ReadyToWriteEpoch())
        WriteTagFileToDisk();
      MegalodonLogger.LogInfo("Closing off tagfile"); 
    }

    private void StartTimer()
    {
      TagTimer.Enabled = false;
      TagTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
      TagTimer.Interval = TagConstants.TAG_FILE_INTERVAL;
      TagTimer.Start();
    }

    private void StartNewTagFile()
    {

    }

    /// <summary>
    /// Outputs current tagfile in memory to disk
    /// </summary>
    public void WriteTagFileToDisk()
    {

      // Close off tagfile content list

      if (Parser.HeaderRequired) // could check further for more epoch data 
      {
        MegalodonLogger.LogInfo($"WriteTagFileToDisk. No data to create tagfile.");
        return;
      }

      Parser.TrailerRequired = true;
      Parser.UpdateTagContentList();
      Header.UpdateTagfileName(Parser.EpochRec.Serial, Parser.EpochRec.MID);
      Directory.CreateDirectory(userSettings.TagfileFolder); 
      var newFilename = System.IO.Path.Combine(userSettings.TagfileFolder, Header.TagfileName);
      var outStream = new NybbleFileStream(newFilename, FileMode.Create);
      try
      {
        if (Write(outStream))
        {
          Parser.Reset();
          MegalodonLogger.LogInfo($"{newFilename} written to disk.");
        }
        else
          MegalodonLogger.LogInfo($"{newFilename} failed to write to disk.");
      }
      finally
      {
        outStream.Dispose();
      }

    }

    public void ParseText(string txt)
    {
      Parser.ParseText(txt);
  //    if (Parser.ReadyToWriteEpoch())
    //    WriteTagFileToDisk();
    }

    /// <summary>
    /// Create tagfile dictionary for all supported types
    /// </summary>
    public void CreateTagfileDictionary()
    {
      short idx = 1; // start idx from 1
      TagFileDictionary = new TAGDictionary();
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileTimeTag, TAGDataType.t32bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileTimeTag, TAGDataType.t4bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileWeekTag, TAGDataType.t16bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagCoordSysType, TAGDataType.t4bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagUTMZone, TAGDataType.t8bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileElevationMappingModeTag, TAGDataType.t8bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileLeftTag, TAGDataType.tEmptyType, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileRightTag, TAGDataType.tEmptyType, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileEastingTag, TAGDataType.tIEEEDouble, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileNorthingTag, TAGDataType.tIEEEDouble, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileElevationTag, TAGDataType.tIEEEDouble, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileGPSModeTag, TAGDataType.t4bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileBladeOnGroundTag, TAGDataType.t4bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileDesignTag, TAGDataType.tUnicodeString, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagPositionLatitude, TAGDataType.tIEEEDouble, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagPositionLongitude, TAGDataType.tIEEEDouble, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagPositionHeight, TAGDataType.tIEEEDouble, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileSerialTag, TAGDataType.tANSIString, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagRadioSerial, TAGDataType.tANSIString, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagRadioType, TAGDataType.tANSIString, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileMachineIDTag, TAGDataType.tANSIString, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileApplicationVersion, TAGDataType.tANSIString, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagMachineSpeed, TAGDataType.tIEEEDouble, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagMachineType, TAGDataType.t8bitUInt, idx++);
    }

    public void CreateTestData()
    {

      CreateTagfileDictionary();

      // todo Setup header defaults for test
      Parser.EpochRec.Week = 1;
      Parser.EpochRec.CoordSys = 3;
      Parser.EpochRec.UTM = 0; // not needed so default
      Parser.EpochRec.MappingMode = 1;
      // vss supplied
      Parser.EpochRec.RadioType = "torch";
      //Parser.EpochRec.RadioSerial = "123456";
   //   Parser.EpochRec.Serial = "e6cd374b-22d5-4512-b60e-fd8152a0899b";

      //    StartTimer();

      // handy code
      
      Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

      // from local time to unix time
      var dateTime = new DateTime(2015, 05, 24, 10, 2, 0, DateTimeKind.Local);
      var dateTimeOffset = new DateTimeOffset(dateTime);
      var unixDateTime = dateTimeOffset.ToUnixTimeSeconds();

      // going back again 
      var localDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixDateTime)
        .DateTime.ToLocalTime();

      var timeStamp = "TME" + unixTimestamp.ToString();

      // Record Seperator
      var rs = Convert.ToChar(TagConstants.RS).ToString();
      // this would normally be read from socket
      ParseText(rs + timeStamp +
        rs + "GPM3" +
        rs +  "DESPlanB" +
        rs + "LAT-0.759971" +
        rs + "LON3.012268" +
        rs + "HGT-37.600" +
        rs + "MIDVessel1" +
        rs + "BOG0" +
        //        rs + "UTM0" +
        rs + "HDG92" +
        rs + "HDG92" +
        rs + "HDG92" +
        rs + "SERe6cd374b - 22d5 - 4512 - b60e - fd8152a0899b" +
        rs + "MTPHEX"
        );


      Header.TagfileName = TagUtils.MakeTagfileName("", Parser.EpochRec.MID);

      unixTimestamp += 100;
      timeStamp = "TME" + unixTimestamp.ToString();

      ParseText(rs + timeStamp +
        rs + "LEB504383.841" + rs + "LNB7043871.371" + rs + "LHB-20.882" +
        rs + "LEB504383.841" + rs + "REB504384.745" + rs + "RNB7043869.853" +
        rs + "RHB-20.899" +  rs + "BOG1" + rs + "MSD0.2" + rs + "HDG93");


      unixTimestamp += 100;
      timeStamp = "TME" + unixTimestamp.ToString();

      ParseText(rs + timeStamp +
        rs + "LEB504383.851" + rs + "LNB7043871.381" + rs + "LHB-20.992" +
        rs + "LEB504383.851" + rs + "REB504384.755" + rs + "RNB7043869.863" +
        rs + "RHB-20.999" + rs + "MSD0.2" + rs + "HDG94");

    }

    /// <summary>
    /// Write out contents of tagfile
    /// </summary>
    /// <param name="nStream">File stream to write to</param>
    /// <returns></returns>
    public bool Write(NybbleStream nStream)
    {
      nStream.NybblePosition = 0;
      Header.TypeTableOffset = 0; // overriden later
      Header.Write(nStream);

      // write contents

      Parser.TagContent.Write(nStream);
      nStream.Pad(); // have to pad the dictionary
      var pos = nStream.stream.Position;

      TagFileDictionary.Write(nStream);

      Header.TypeTableOffset = pos;
      nStream.NybblePosition = 0;
      nStream.HighNybble = false;
      Header.Write(nStream);

      return true;
    }


  }
}