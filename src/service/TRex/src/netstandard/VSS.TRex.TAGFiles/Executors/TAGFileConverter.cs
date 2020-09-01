using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Geometry;
using VSS.TRex.Logging;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.OEM.Volvo;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Executors
{
  /// <summary>
  /// Converts a TAG file from the vector based measurements of the machine's operation into the cell pass
  /// and events based description used in a TRex data model
  /// </summary>
  public class TAGFileConverter : IDisposable
  {
    private static readonly ILogger Log = Logger.CreateLogger<TAGFileConverter>();

    /// <summary>
    /// The overall result of processing the TAG information in the file
    /// </summary>
    public TAGReadResult ReadResult { get; set; } = TAGReadResult.NoError;

    /// <summary>
    /// The number of measurement epochs encountered in the TAG data
    /// </summary>
    public int ProcessedEpochCount { get; set; }

    /// <summary>
    /// The number of cell passes generated from the cell data
    /// </summary>
    public int ProcessedCellPassCount { get; set; }

    /// <summary>
    /// The target site model representing the ultimate recipient of the cell pass and event 
    /// generated from the TAG file
    /// </summary>
    public ISiteModel SiteModel { get; set; }

    /// <summary>
    /// The target machines within the target site model that generated the overall set TAG file(s) being processed
    /// </summary>
    public IMachinesList Machines { get; set; }

    /// <summary>
    /// The target machine within the target site model that generated the current TAG file being processed
    /// </summary>
    public IMachine Machine { get; set; }

    /// <summary>
    /// SiteModelGridAggregator is an object that aggregates all the cell passes generated while processing the file. 
    /// These are then integrated into the primary site model in a single step at a later point in processing
    /// </summary>
    public IServerSubGridTree SiteModelGridAggregator { get; set; }

    /// <summary>
    /// MachineTargetValueChangesAggregator is an object that aggregates all the
    /// machine state events of interest that we encounter while processing the
    /// file. These are then integrated into the machine events in a single step
    /// at a later point in processing
    /// </summary>
    public MachinesProductionEventLists MachinesTargetValueChangesAggregator { get; set; }

    /// <summary>
    /// The processor used as the sink for values reader from the TAG file by the TAG file reader.
    /// Once the TAG file is converted, this contains the final state of the TAGProcessor state machine.
    /// </summary>
    public TAGProcessor Processor { get; set; }

    /// <summary>
    /// Is coordinate system of type UTM
    /// </summary>
    public bool IsUTMCoordinateSystem { get; set; }

    /// <summary>
    /// Target sitemodel
    /// </summary>
    private Guid? _targetSiteModel;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public TAGFileConverter(Guid? targetSiteModel = null)
    {
      _targetSiteModel = targetSiteModel;
      Initialise();
    }

    private void Initialise()
    {
      ProcessedEpochCount = 0;
      ProcessedCellPassCount = 0;

      // Note: Intermediary TAG file processing contexts don't store their data to any persistence context
      // so the SiteModel constructed to contain the data processed from a TAG file does not need a 
      // storage proxy assigned to it
      SiteModel = DIContext.Obtain<ISiteModelFactory>().NewSiteModel(StorageMutability.Mutable);

      SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID, StorageMutability.Mutable)
      {
        CellSize = SiteModel.CellSize
      };

      MachinesTargetValueChangesAggregator = new MachinesProductionEventLists(SiteModel, 0);

      Machines = new MachinesList();
    }

    /// <summary>
    /// Resets the internal state of the converter ready for another TAG file
    /// </summary>
    public void Reset()
    {
    }

    /// <summary>
    /// Fill out the local class properties with the information wanted from the TAG file
    /// </summary>
    /// <param name="processor"></param>
    private void SetPublishedState(TAGProcessor processor)
    {
      ProcessedEpochCount = processor.ProcessedEpochCount;
      ProcessedCellPassCount = processor.ProcessedCellPassesCount;

      // Set the site model's last modified date...
      SiteModel.LastModifiedDate = DateTime.UtcNow;

      //Update latest status for the machine
      Machine.LastKnownX = processor.DataLeft.X;
      Machine.LastKnownY = processor.DataLeft.Y;
      Machine.LastKnownPositionTimeStamp = processor.DataTime;
      Machine.MachineHardwareID = processor.HardwareID;
      Machine.MachineType = processor.MachineType;
      Machine.Name = processor.MachineID;
    }

    /// <summary>
    /// Execute the conversion operation on the Volvo earthworks CSV file
    /// NOTE: This is a POC implementation and does not support some behaviours in the legacy TAG file ingest pathway
    /// </summary>
    public bool ExecuteVolvoEarthworksCSVFile(string filename, Stream tagData, Guid assetUid, bool isJohnDoe)
    {
      ReadResult = TAGReadResult.NoError;

      Log.LogInformation($"In {nameof(ExecuteVolvoEarthworksCSVFile)}: reading file {filename} for asset {assetUid}, JohnDoe: {isJohnDoe}");

      try
      {
        var fileDescriptor = new VolvoEarthworksFileNameDescriptor(filename);

        Processor?.Dispose();

        // Locate the machine in the local set of machines, adding one if necessary
        Machine = Machines.Locate(assetUid, true /*isJohnDoe - hard code Volvo machines to be John Does for POC*/);

        var machineType = MachineType.Unknown;
        var machineHardwareId = fileDescriptor.MachineID;
        var machineId = fileDescriptor.MachineID;

        if (Machine == null)
        {
          Log.LogDebug($"Creating new machine in common converter for AssetUid = {assetUid}, JohnDoe = {isJohnDoe}, machineId = {machineId}, machineHardwareId = {machineHardwareId}");

          Machine = Machines.CreateNew(machineId, machineHardwareId, machineType, DeviceTypeEnum.MANUALDEVICE, isJohnDoe, assetUid);
        }

        var holdMachineType = Machine.MachineType;

        // Locate the aggregator, adding one if necessary
        var machineTargetValueChangesAggregator = MachinesTargetValueChangesAggregator[Machine.InternalSiteModelMachineIndex] as ProductionEventLists;
        if (machineTargetValueChangesAggregator == null)
        {
          machineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, Machine.InternalSiteModelMachineIndex);
          MachinesTargetValueChangesAggregator.Add(machineTargetValueChangesAggregator);
        }

        Processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, machineTargetValueChangesAggregator);

        var sink = new TAGValueSink(Processor);
        var reader = new VolvoEarthworksCSVReader(tagData);

        ReadResult = reader.Read(sink, Processor);

        // Notify the processor that all reading operations have completed for the file
        Processor.DoPostProcessFileAction(ReadResult == TAGReadResult.NoError);

        SetPublishedState(Processor);
        Machine.MachineType = holdMachineType;

        if (ReadResult != TAGReadResult.NoError)
          return false;
      }
      catch (Exception e) // make sure any exception is trapped to return correct response to caller
      {
        Log.LogError(e, "Exception occurred while converting a Volvo CSV file");
        return false;
      }

      return true;
    }

    /// <summary>
    /// Scan tag file for each epoch position and convert from position's UTM zone to WGS84LL. Then back to the coordinate sytem used by project.
    /// </summary>
    private bool CollectAndConvertBladePostions(Guid? targetProjectUid, ref Stream tagData, ref List<UTMCoordPointPair> aCSBladePositions, ref List<UTMCoordPointPair> aCSRearAxlePositions, ref List<UTMCoordPointPair> aCSTrackPositions, ref List<UTMCoordPointPair> aCSWheelPositions)
    {

      var tagFilePreScanACS = new TAGFilePreScanACS(); // special scanner to collect positions
      tagFilePreScanACS.Execute(tagData, ref aCSBladePositions, ref aCSRearAxlePositions, ref aCSTrackPositions, ref aCSWheelPositions);
      tagData.Position = 0; // reset

      if (tagFilePreScanACS.ReadResult == TAGReadResult.NoError)
      {
        Log.LogInformation($"Successful ACS prescan of tagfile. {tagFilePreScanACS.ReadResult}. # Blade positions to convert:{aCSBladePositions.Count}, RearAxle:{aCSRearAxlePositions.Count}, Track:{aCSTrackPositions.Count}, Wheel:{aCSWheelPositions.Count}");
        var acsTranslator = DIContext.Obtain<IACSTranslator>();
        if (acsTranslator == null)
        {
          Log.LogError($"IACSTranslator not implemented");
          return false;
        }
        var hold = ReadResult;
        ReadResult = TAGReadResult.CoordinateConversionFailure;
        if (acsTranslator.TranslatePositions(targetProjectUid, aCSBladePositions) == null) return false;
        if (acsTranslator.TranslatePositions(targetProjectUid, aCSRearAxlePositions) == null) return false;
        if (acsTranslator.TranslatePositions(targetProjectUid, aCSTrackPositions) == null) return false;
        if (acsTranslator.TranslatePositions(targetProjectUid, aCSWheelPositions) == null) return false;
        ReadResult = hold;
      }
      else
      {
        Log.LogWarning($"Unsuccessful PrescanACS of tagfile. {tagFilePreScanACS.ReadResult}");
        return false;
      }

      return true;
    }

    private bool ValidPositionsforPair(UTMCoordPointPair uTMCoordPointPair)
    {
      return !(uTMCoordPointPair.Left.X == Consts.NullReal || uTMCoordPointPair.Left.Y == Consts.NullReal || uTMCoordPointPair.Right.X == Consts.NullReal || uTMCoordPointPair.Right.Y == Consts.NullReal);
    }

    /// <summary>
    /// Execute the conversion operation on the TAG file, returning a boolean success result.
    /// Sets up local state detailing the pre-scan fields retrieved from the TAG file
    /// </summary>
    public bool ExecuteLegacyTAGFile(string filename, Stream tagData, Guid assetUid, bool isJohnDoe)
    {
      Log.LogInformation($"In {nameof(ExecuteLegacyTAGFile)}: reading file {filename} for asset {assetUid}, JohnDoe: {isJohnDoe}");

      ReadResult = TAGReadResult.NoError;
      List<UTMCoordPointPair> aCSBladePositions = null;
      List<UTMCoordPointPair> ACSRearAxlePositions = null;
      List<UTMCoordPointPair> ACSTrackPositions = null;
      List<UTMCoordPointPair> ACSWheelPositions = null;

      try
      {
        Processor?.Dispose();

        // Locate the machine in the local set of machines, adding one if necessary
        Machine = Machines.Locate(assetUid, isJohnDoe);

        var machineType = MachineType.Unknown;
        var machineHardwareId = string.Empty;
        var machineId = string.Empty;

        //Prescan to get all relevant information necessary for processing the tag file. e.g. Machinetype for swather, Type of coordinate system (ACS)
        var tagFilePreScan = new TAGFilePreScan();
        tagFilePreScan.Execute(tagData);
        tagData.Position = 0; // reset
        if (tagFilePreScan.ReadResult == TAGReadResult.NoError)
        {
          machineType = tagFilePreScan.MachineType; // used in creation of swather
          machineHardwareId = tagFilePreScan.HardwareID;
          machineId = tagFilePreScan.MachineID;
          IsUTMCoordinateSystem = !tagFilePreScan.IsCSIBCoordSystemTypeOnly; // do we need to convert UTM coordinates to project coordinates
          if (IsUTMCoordinateSystem && tagFilePreScan.ProcessedEpochCount > 0)
          {
            Log.LogInformation($"{nameof(ExecuteLegacyTAGFile)}: ACS coordinate system detected. {filename}");
            aCSBladePositions = new List<UTMCoordPointPair>();
            ACSRearAxlePositions = new List<UTMCoordPointPair>();
            ACSTrackPositions = new List<UTMCoordPointPair>();
            ACSWheelPositions = new List<UTMCoordPointPair>();
            if (!CollectAndConvertBladePostions(_targetSiteModel, ref tagData, ref aCSBladePositions, ref ACSRearAxlePositions, ref ACSTrackPositions, ref ACSWheelPositions))
            {
              Log.LogError($"{nameof(ExecuteLegacyTAGFile)}: Failed to collect and convert blade positions for tagfile processing with ACS. TAG FILE:{filename}");
              ReadResult = TAGReadResult.CoordinateConversionFailure;
              return false;
            }
          }
        }
        else
        {
          Log.LogError($"Unsuccessful prescan of tagfile. {tagFilePreScan.ReadResult}");
          return false;
        }

        if (Machine == null)
        {
          // Now we know more about the machine have another go finding it
          Machine = Machines.Locate(assetUid, machineId, isJohnDoe);
        }

        if (Machine == null)
        {
          Log.LogDebug($"Creating new machine in common converter for AssetUid = {assetUid}, JohnDoe = {isJohnDoe}, machineId = {machineId}, machineHardwareId = {machineHardwareId}");

          Machine = Machines.CreateNew(machineId, machineHardwareId, machineType, DeviceTypeEnum.MANUALDEVICE, isJohnDoe, assetUid);
        }

        if (Machine.MachineType == MachineType.Unknown && machineType != MachineType.Unknown)
          Machine.MachineType = machineType;

        var holdMachineType = Machine.MachineType;

        // Locate the aggregator, adding one if necessary
        var machineTargetValueChangesAggregator = MachinesTargetValueChangesAggregator[Machine.InternalSiteModelMachineIndex] as ProductionEventLists;
        if (machineTargetValueChangesAggregator == null)
        {
          machineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, Machine.InternalSiteModelMachineIndex);
          MachinesTargetValueChangesAggregator.Add(machineTargetValueChangesAggregator);
        }

        Processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, machineTargetValueChangesAggregator);

        // If ACS coordinate system populate converted UTM coordinates
        if (IsUTMCoordinateSystem && tagFilePreScan.ProcessedEpochCount > 0)
        {
          if (aCSBladePositions != null && aCSBladePositions.Count > 0)
            Processor.ConvertedBladePositions.AddRange(aCSBladePositions);
          if (ACSRearAxlePositions != null && ACSRearAxlePositions.Count > 0)
            Processor.ConvertedRearAxlePositions.AddRange(ACSRearAxlePositions);
          if (ACSTrackPositions != null && ACSTrackPositions.Count > 0)
            Processor.ConvertedTrackPositions.AddRange(ACSTrackPositions);
          if (ACSWheelPositions != null && ACSWheelPositions.Count > 0)
            Processor.ConvertedWheelPositions.AddRange(ACSWheelPositions);
        }

        var sink = new TAGValueSink(Processor);
        using (var reader = new TAGReader(tagData))
        {
          var tagFile = new TAGFile();

          ReadResult = tagFile.Read(reader, sink);

          // Notify the processor that all reading operations have completed for the file
          Processor.DoPostProcessFileAction(ReadResult == TAGReadResult.NoError);

          SetPublishedState(Processor);
          Machine.MachineType = holdMachineType;

          if (ReadResult != TAGReadResult.NoError)
            return false;
        }
      }
      catch (Exception e) // make sure any exception is trapped to return correct response to caller
      {
        Log.LogError(e, "Exception occurred while converting a TAG file");
        return false;
      }

      return true;
    }

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          Processor?.Dispose();
        }

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }
}
