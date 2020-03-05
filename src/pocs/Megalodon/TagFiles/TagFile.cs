using System;
using System.IO;
using System.Timers;
using Microsoft.Extensions.Logging;
using TagFiles.Common;
using TagFiles.Parser;
using TagFiles.Types;
using TagFiles.Utils;
using TAGFiles.Common;

namespace TagFiles
{
  /// <summary>
  /// TagFile library is a stand alone class for creating tagfiles
  /// Input is via the method ParseText
  /// </summary>
  public class TagFile
  {
    private readonly object updateLock = new object();
    private readonly TAGDictionary Dictionary = new TAGDictionary();
    private Timer TagTimer = new System.Timers.Timer();
    private bool readyToWrite = false;
    private ulong tagFileCount = 0;
    private ulong epochCount = 0;
    private bool tmpNR = false;
    public TagHeader Header = new TagHeader();
    public TAGDictionary TagFileDictionary;
    public AsciiParser Parser = new AsciiParser();
    public string MachineSerial = "UnknownSerial";
    public string MachineID = "UnknownID";
    public bool SendTagFilesDirect = false;
    public string TagFileFolder = "c:\\Trimble\\tagfiles";
    public double SeedLat = 0;
    public double SeedLon = 0;
    public double TagFileIntervalMilliSecs = 60000; // default 60 seconds
    public ILogger Log;

    /// <summary>
    /// Tagfile cutoff controlled by timer
    /// </summary>
    private bool _EnableTagFileCreationTimer = false;
    public bool EnableTagFileCreationTimer  // read-write instance property
    {
      get => _EnableTagFileCreationTimer;
      set {
            _EnableTagFileCreationTimer = value;
            TagTimer.Enabled = value;
            if (value)
            {
              TagTimer.Interval = TagFileIntervalMilliSecs; //TagConstants.NEW_TAG_FILE_INTERVAL_MSECS;
              TagTimer.Start();
            }
          }
    }

    public TagFile()
    {
      // Setup Tagfile Directory
      CreateTagfileDictionary();
      TagTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
    }


    public void SetupLog(ILogger log)
    {
      Log = log;
      Parser.Log = log;
    }

    /// <summary>
    /// Timer event for closing off tagfile
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {

      if (!readyToWrite)
      {
        var hertz = 0.0;
        if (epochCount > 0)
          hertz =  epochCount / (TagTimer.Interval / 1000);
        Log.LogInformation($"TimerInterval:{(TagTimer.Interval / 1000)}s, Epochs:{epochCount}, Hertz:{hertz.ToString("#.#")}");
        epochCount = 0;
        readyToWrite = true;
        lock (updateLock)
        {
          WriteTagFileToDisk();
          readyToWrite = false;
        }
      }
    }

    /// <summary>
    /// Shut down tagfile and write to disk
    /// </summary>
    public void ShutDown()
    {
      if (epochCount > 0)
      { 
        lock (updateLock)
        {
          WriteTagFileToDisk();
          // allow time for tagfile to be sent to VL
          System.Threading.Thread.Sleep(1000);
        }
      }
    }


    /// <summary>
    /// Outputs current tagfile in memory to disk
    /// </summary>
    public void WriteTagFileToDisk()
    {
     // string newFilename;

      if (Parser.HeaderRequired)
      {
        Log.LogDebug($"WriteTagFileToDisk. No header record recieved.");
        return;
      }

      if (Parser.NotSeenNewPosition)
      {
        Log.LogInformation($"WriteTagFileToDisk. No new positions reported.");
        return;
      }

      Parser.TrailerRequired = true;
      Parser.CloneLastEpoch(); // used in new tagfile

      if (Parser._Prev_EpochRec != null)
      {
        if (Parser.EpochRec.NotEpochSamePosition(ref Parser._Prev_EpochRec))
          Parser.UpdateTagContentList(ref Parser.EpochRec, ref tmpNR);
        else
        {
          // dont report a position change for trailer record 
          Parser.EpochRec.HasLEB = false;
          Parser.EpochRec.HasLNB = false;
          Parser.EpochRec.HasLHB = false;
          Parser.EpochRec.HasREB = false;
          Parser.EpochRec.HasRNB = false;
          Parser.EpochRec.HasRHB = false;
          Parser.UpdateTagContentList(ref Parser.EpochRec, ref tmpNR);
        }
      }
      else
        Parser.UpdateTagContentList(ref Parser.EpochRec, ref tmpNR);

      var serial = Parser.EpochRec.Serial == string.Empty ? MachineSerial : Parser.EpochRec.Serial;
      var mid = Parser.EpochRec.MID == string.Empty ? MachineID : Parser.EpochRec.MID;
      Header.UpdateTagfileName(serial,mid);
      // Make sure folder exists
      Directory.CreateDirectory(TagFileFolder);

      // Put tagfile in a ToSend folder
      var toSendFolder = System.IO.Path.Combine(TagFileFolder, TagConstants.TAGFILE_FOLDER_TOSEND);
      var toSendFilePath = System.IO.Path.Combine(toSendFolder, Header.TagfileName);
      var directFilePath = System.IO.Path.Combine(TagFileFolder, Header.TagfileName);

      var outStream = new NybbleFileStream(toSendFilePath, FileMode.Create);
      try
      {
        if (Write(outStream)) // write tagfile to stream
        {
        //  _NewTagfileStarted = true;
          Parser.Reset();
          tagFileCount++;
          Log.LogInformation($"{toSendFilePath} successfully written to disk. Total Tagfiles:{tagFileCount}");
          readyToWrite = false;
        }
        else
          Log.LogWarning($"{toSendFilePath} failed to write to disk.");
      }
      finally
      {
        outStream.Dispose();
        Parser.NotSeenNewPosition = true;
        if (SendTagFilesDirect) // move up one folder for direct send
        {
          if (File.Exists(toSendFilePath))
          {
            FileInfo f = new FileInfo(toSendFilePath);
            f.MoveTo(directFilePath); // move tagfile up one folder for direct send
          }
        }

      }

    }

    /// <summary>
    /// Epoch input from socket is parsed and processed
    /// </summary>
    /// <param name="txt"></param>
    public bool ParseText(string txt)
    {

      // protected thread
      lock (updateLock)
      {
        epochCount++;
        if (!Parser.ParseText(txt))
        {
          Log.LogWarning($"Failed to parse data packet:{txt}");
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Create tagfile dictionary for all supported types
    /// </summary>
    private void CreateTagfileDictionary()
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
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileOnGroundTag, TAGDataType.t4bitUInt, idx++);
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
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileValidPositionTag, TAGDataType.t4bitUInt, idx++);
    }

    /// <summary>
    /// Used in testing this class
    /// </summary>
    public void CreateTestData()
    {

      CreateTagfileDictionary();

      Parser.EpochRec.Week = 1;
      Parser.EpochRec.CoordSys = 3;
      Parser.EpochRec.UTM = 0; // not needed so default
      Parser.EpochRec.MappingMode = 1;
      // vss supplied
      Parser.EpochRec.RadioType = "torch";
      
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
        rs + "UTM0" +
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
      // write header
      nStream.NybblePosition = 0;
      Header.TypeTableOffset = 0; // overriden later
      Header.Write(nStream);

      // write contents
      Parser.TagContent.Write(nStream);
      nStream.Pad(); // have to pad the dictionary
      var pos = nStream.stream.Position;
      // dictionary writtten last
      TagFileDictionary.Write(nStream);

      Header.TypeTableOffset = pos;
      nStream.NybblePosition = 0;
      nStream.HighNybble = false;
      // update header offset position for dictionary
      Header.Write(nStream);

      return true;
    }


  }
}
