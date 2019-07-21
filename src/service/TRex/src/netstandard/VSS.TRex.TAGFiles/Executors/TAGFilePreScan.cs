using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

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
    public double? SeedNorthing { get; set; }
    public double? SeedEasting { get; set; }
    public double? SeedElevation { get; set; }

    public DateTime? SeedTimeUTC { get; set; }

    public byte UTMZone { get; set; }

    public int ProcessedEpochCount { get; set; }

    public string RadioType { get; set; } = string.Empty;
    public string RadioSerial { get; set; } = string.Empty;

    public MachineType MachineType { get; set; } = CellPassConsts.MachineTypeNull;

    public string MachineID { get; set; } = string.Empty;
    public string HardwareID { get; set; } = string.Empty;

    public string ApplicationVersion { get; set; } = string.Empty;

    public string DesignName { get; set; } = string.Empty;

    public MachineControlPlatformType PlatformType {get; set;}


    public TAGReadResult ReadResult { get; set; } = TAGReadResult.NoError;

    /// <summary>
    /// The time stamp of the last measured epoch in the TAG file.
    /// </summary>
    public DateTime? LastDataTime { get; set; }

    /// <summary>
    /// Set the state of the executor to an initialized state
    /// </summary>
    private void Initialise()
    {
      LastDataTime = null;

      SeedLatitude = null;
      SeedLongitude = null;
      SeedNorthing = null;
      SeedEasting = null;

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
    /// <param name="processor"></param>
    private void SetPublishedState(TAGProcessorPreScanState processor)
    {
      LastDataTime = processor.DataTime;
      SetSeedPosition(processor);
     
      ProcessedEpochCount = processor.ProcessedEpochCount;
      RadioType = processor.RadioType;
      RadioSerial = processor.RadioSerial;
      MachineType = processor.MachineType;
      MachineID = processor.MachineID;
      HardwareID = processor.HardwareID;
      ApplicationVersion = processor.ApplicationVersion;
      DesignName = processor.Design;
      PlatformType = processor.GetPlatformType();
    }

    /// <summary>
    /// Grid point from on-machine positions
    /// Scenarios
    /// a) Lat/long present
    ///        TFA to use this as the seed location
    /// b) No Lat/long, but UTM zone present
    ///         Discussion with Grant and Raymond:
    ///         Potential corner case where UTMZone may be different to the projects CSIB.
    ///         safer to convert to lat/long using the UTMZone
    ///         unable to find any samples
    /// c) No Lat/long, no UTM zone, but has a NEE
    ///       TFA to use project CSIBs and NEE to determine potential LLs
    /// </summary>
    /// <param name="processor"></param>
    private void SetSeedPosition(TAGProcessorPreScanState processor)
    {
      PopulateNEE(processor);

      if (processor.LLHLatRecordedTime.HasValue)
      {
        SeedLatitude = Math.Abs(processor.LLHLat - Consts.NullDouble) < Consts.TOLERANCE_DECIMAL_DEGREE ? (double?) null : processor.LLHLat;
        SeedLongitude = Math.Abs(processor.LLHLon - Consts.NullDouble) < Consts.TOLERANCE_DECIMAL_DEGREE ? (double?) null : processor.LLHLon;
        SeedHeight = Math.Abs(processor.LLHHeight - Consts.NullDouble) < Consts.TOLERANCE_DECIMAL_DEGREE ? (double?) null : processor.LLHHeight;
        SeedTimeUTC = processor.LLHLatRecordedTime; //We arbitrarily choose LLHLat, in the majority of cases this will be same for any LLH.
      }

      UTMZone = processor.UTMZone;
    }

    private void PopulateNEE(TAGProcessorPreScanState processor)
    {
      SeedTimeUTC = processor._FirstDataTime;

      if (processor.HaveReceivedValidTipPositions)
      {
        SeedNorthing = (processor.DataLeft.Y + processor.DataRight.Y) / 2;
        SeedEasting = (processor.DataLeft.X + processor.DataRight.X) / 2;
        SeedElevation = (processor.DataLeft.Z + processor.DataRight.Z) / 2;
      }
      else
      {
        if (processor.HaveReceivedValidRearPositions)
        {
          SeedNorthing = (processor.DataRearLeft.Y + processor.DataRearRight.Y) / 2;
          SeedEasting = (processor.DataRearLeft.X + processor.DataRearRight.X) / 2;
          SeedElevation = (processor.DataRearLeft.Z + processor.DataRearRight.Z) / 2;
        }
        else
        {
          if (processor.HaveReceivedValidWheelPositions)
          {
            SeedNorthing = (processor.DataWheelLeft.Y + processor.DataWheelRight.Y) / 2;
            SeedEasting = (processor.DataWheelLeft.X + processor.DataWheelRight.X) / 2;
            SeedElevation = (processor.DataWheelLeft.Z + processor.DataWheelRight.Z) / 2;
          }
          else
          {
            if (processor.HaveReceivedValidTrackPositions)
            {
              SeedNorthing = (processor.DataTrackLeft.Y + processor.DataTrackRight.Y) / 2;
              SeedEasting = (processor.DataTrackLeft.X + processor.DataTrackRight.X) / 2;
              SeedElevation = (processor.DataTrackLeft.Z + processor.DataTrackRight.Z) / 2;
            }
          }
        }
      }
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

        using (var Processor = new TAGProcessorPreScanState())
        {
          var Sink = new TAGVisionLinkPrerequisitesValueSink(Processor);
          using (var Reader = new TAGReader(TAGData))
          {
            var TagFile = new TAGFile();

            ReadResult = TagFile.Read(Reader, Sink);
          }

          if (ReadResult != TAGReadResult.NoError)
            return false;

          SetPublishedState(Processor);
        }
      }
      catch // (Exception E) // make sure any exception is trapped to return correct response to caller
      {
        return false;
      }

      return true;
    }
  }
}
