using System.IO;
using System.Timers;
using Microsoft.Extensions.Logging;
using TagFiles.Common;
using TagFiles.Parser;
using TagFiles.Types;
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
    public bool ReadyToWrite = false;
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
    public byte TransmissionProtocolVersion = TagConstants.Version1; 
    public ILogger Log;

    /// <summary>
    /// Tagfile cutoff controlled by timer
    /// </summary>
    private bool _EnableTagFileCreationTimer = false;
    public bool EnableTagFileCreationTimer  // read-write instance property
    {
      get => _EnableTagFileCreationTimer;
      set
      {
        _EnableTagFileCreationTimer = value;
        TagTimer.Enabled = value;
        if (value)
        {
          TagTimer.Interval = TagFileIntervalMilliSecs; 
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
    /// Timer event for closing off tagfiles
    /// </summary>
    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
      if (!ReadyToWrite)
        ReadyToWrite = true;
      else
      {
        // Check if we need to force a write to end last broadcasted epoch
        if (epochCount > 0)
          CutOffTagFile();
      }
    }

    /// <summary>
    /// Timer event has indicated it time to close the tagfile
    /// </summary>
    public void CutOffTagFile()
    {
      var hertz = 0.0;
      if (epochCount > 0)
        hertz = epochCount / (TagTimer.Interval / 1000);
      Log.LogInformation($"TimerInterval:{(TagTimer.Interval / 1000)}s, Epochs:{epochCount}, Hertz:{hertz.ToString("#.#")}");
      epochCount = 0;
      ReadyToWrite = true;
      lock (updateLock)
      {
        WriteTagFileToDisk();
        ReadyToWrite = false;
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

      if (Parser.HeaderRequired)
      {
        Log.LogDebug($"{nameof(WriteTagFileToDisk)}. No header record recieved.");
        return;
      }

      if (Parser.NotSeenNewPosition)
      {
        Log.LogInformation($"{nameof(WriteTagFileToDisk)}. No new blade positions reported.");
        return;
      }

      Log.LogInformation($"{nameof(WriteTagFileToDisk)}. Start.");

      Parser.TrailerRequired = true;
      Parser.CloneLastEpoch(); // used as first epoc in new tagfile. Helps prevents gaps when processing tagfiles 

      var serial = Parser.EpochRec.LastSerial == string.Empty ? MachineSerial : Parser.EpochRec.LastSerial;
      var mid = Parser.EpochRec.LastMID == string.Empty ? MachineID : Parser.EpochRec.LastMID;
      Header.UpdateTagfileName(serial, mid);

      if (Parser.Prev_EpochRec != null)
      {
        if (Parser.EpochRec.MachineStateDifferent(ref Parser.Prev_EpochRec))
          Parser.UpdateTagContentList(ref Parser.EpochRec, ref tmpNR, TagConstants.UpdateReason.CutOffLastEpoch); // save epoch to tagfile before writing
        else
        {
          // dont report a position change for trailer record 
          Parser.EpochRec.LEB = double.MaxValue;
          Parser.EpochRec.LNB = double.MaxValue;
          Parser.EpochRec.LHB = double.MaxValue;
          Parser.EpochRec.REB = double.MaxValue;
          Parser.EpochRec.RNB = double.MaxValue;
          Parser.EpochRec.RHB = double.MaxValue;
          Parser.UpdateTagContentList(ref Parser.EpochRec, ref tmpNR, TagConstants.UpdateReason.CutOffNoChange);
        }
      }
      else
        Parser.UpdateTagContentList(ref Parser.EpochRec, ref tmpNR, TagConstants.UpdateReason.CutOffSoloLastEpoch);

      if (Parser.Prev_EpochRec != null)
        Parser.Prev_EpochRec.ClearEpoch();

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
          tagFileCount++;
          Log.LogInformation($"{nameof(WriteTagFileToDisk)}. End. {toSendFilePath} successfully written to disk. Updates:{Parser.EpochCount}, Total Tagfiles:{tagFileCount}");
          Parser.Reset();
        }
        else
          Log.LogWarning($"{nameof(WriteTagFileToDisk)}. End. {toSendFilePath} failed to write to disk.");
        ReadyToWrite = false;
        Parser.HeaderRequired = true;
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
      short idx = 1; // start idx from 1. Matches order in DictionaryItem
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
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileICCCVTag, TAGDataType.t12bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileICCCVTargetTag, TAGDataType.t12bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileICMDPTag, TAGDataType.t12bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileICMDPTargetTag, TAGDataType.t12bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTemperatureTag, TAGDataType.t12bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileCompactorSensorType, TAGDataType.t8bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileICModeTag, TAGDataType.t4bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileDirectionTag, TAGDataType.t4bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileTargetLiftThickness, TAGDataType.t16bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTagFileICPassTargetTag, TAGDataType.t12bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTempLevelMinTag, TAGDataType.t12bitUInt, idx++);
      TagFileDictionary.AddEntry(TAGValueNames.kTempLevelMaxTag, TAGDataType.t12bitUInt, idx++);
    }


    public void SetupDefaultConfiguration(ushort mappingMode)
    {
      CreateTagfileDictionary();
      Parser.EpochRec.Week = 1;
      Parser.EpochRec.CoordSys = 3;
      Parser.EpochRec.UTM = 0; 
      Parser.EpochRec.RadioType = "torch";
    }

    /// <summary>
    /// Write out contents of tagfile
    /// </summary>
    /// <param name="nStream">File stream to write to</param>
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
