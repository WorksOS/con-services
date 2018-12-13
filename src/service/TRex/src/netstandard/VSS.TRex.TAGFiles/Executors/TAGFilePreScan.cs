using System;
using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Executors
{
  /// <summary>
  /// Executes a TAG file pre scan to extract the pieces of information useful for determining how 
  /// to process the TAG file information
  /// </summary>
  public class TAGFilePreScan
  {
    public double? SeedLatitude { get; set; }
    public double? SeedLongitude { get; set; }

    public double? SeedHeight { get; set; }

    public DateTime? SeedTimeUTC { get; set; }

    public int ProcessedEpochCount { get; set; }

    public string RadioType { get; set; } = string.Empty;
    public string RadioSerial { get; set; } = string.Empty;

    public byte MachineType { get; set; } = CellPassConsts.MachineTypeNull;

    public string MachineID { get; set; } = string.Empty;
    public string HardwareID { get; set; } = string.Empty;

    public string ApplicationVersion { get; set; } = string.Empty;

    public string DesignName { get; set; } = string.Empty;


    public TAGReadResult ReadResult { get; set; } = TAGReadResult.NoError;

    /// <summary>
    /// Set the state of the executor to an initialised state
    /// </summary>
    private void Initialise()
    {
      SeedLatitude = null;
      SeedLongitude = null;
      ProcessedEpochCount = 0;

      RadioType = string.Empty;
      RadioSerial = string.Empty;

      MachineType = CellPassConsts.MachineTypeNull;

      MachineID = string.Empty;
      HardwareID = string.Empty;

      ReadResult = TAGReadResult.NoError;
    }

    /// <summary>
    /// Default no-arg constructor. Sets up initial null state for information returned from a TAG file
    /// </summary>
    public TAGFilePreScan()
    {
      Initialise();
    }

    /// <summary>
    /// Fill out the local class properties with the information wanted from the TAG file
    /// </summary>
    /// <param name="Processor"></param>
    private void SetPublishedState(TAGProcessorPreScanState Processor)
    {
      SeedLatitude = Processor.LLHLat;
      SeedLongitude = Processor.LLHLon;
      SeedHeight = Processor.LLHHeight;
      SeedTimeUTC = Processor.LLHLatRecordedTime; //We arbitrarily choose LLHLat, in the majority of cases this will be same for any LLH.

      ProcessedEpochCount = Processor.ProcessedEpochCount;
      RadioType = Processor.RadioType;
      RadioSerial = Processor.RadioSerial;
      MachineType = Processor.MachineType;
      MachineID = Processor.MachineID;
      HardwareID = Processor.HardwareID;
      ApplicationVersion = Processor.ApplicationVersion;
      DesignName = Processor.Design;
    }

    /// <summary>
    /// Execute the pre-scan operation on the TAG file, returning a boolean success result.
    /// Sets up local state detailing the pre-scan fields retried from the ATG file
    /// </summary>
    /// <param name="TAGData"></param>
    /// <returns></returns>
    public bool Execute(Stream TAGData)
    {
      try
      {
        Initialise();

        TAGProcessorPreScanState Processor = new TAGProcessorPreScanState();
        TAGValueSink Sink = new TAGVisionLinkPrerequisitesValueSink(Processor);
        TAGReader Reader = new TAGReader(TAGData);
        TAGFile TagFile = new TAGFile();

        ReadResult = TagFile.Read(Reader, Sink);

        if (ReadResult != TAGReadResult.NoError)
          return false;

        SetPublishedState(Processor);
      }
      catch // (Exception E) // make sure any exception is trapped to return correct response to caller
      {
        return false;
      }

      return true;
    }

  }
}
