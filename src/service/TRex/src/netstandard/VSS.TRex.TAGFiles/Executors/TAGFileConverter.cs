using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Logging;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Types;
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
        /// Default no-arg constructor
        /// </summary>
        public TAGFileConverter()
        {
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
        /// Execute the conversion operation on the TAG file, returning a boolean success result.
        /// Sets up local state detailing the pre-scan fields retried from the ATG file
        /// </summary>
        /// <param name="tagData"></param>
        /// <param name="assetUid"></param>
        /// <param name="isJohnDoe"></param>
        /// <returns></returns>
        public bool Execute(Stream tagData, Guid assetUid, bool isJohnDoe)
        {
            ReadResult = TAGReadResult.NoError;

            try
            {
                Processor?.Dispose();

                // Locate the machine in the local set of machines, adding one if necessary
                var machine = Machines.FirstOrDefault(x => x.ID == assetUid);
                if (machine == null)
                {
                  machine = new Machine("", "", MachineType.Unknown, DeviceTypeEnum.MANUALDEVICE, assetUid, (short) Machines.Count, isJohnDoe);
                  Machines.Add(machine);
                }

                // Locate the aggregator, adding one if necessary
                var machineTargetValueChangesAggregator = MachinesTargetValueChangesAggregator[machine.InternalSiteModelMachineIndex] as ProductionEventLists;
                if (machineTargetValueChangesAggregator == null)
                {
                  machineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, machine.InternalSiteModelMachineIndex);
                  MachinesTargetValueChangesAggregator.Add(machineTargetValueChangesAggregator);
                }

                Processor = new TAGProcessor(SiteModel, machine, SiteModelGridAggregator, machineTargetValueChangesAggregator);
                var sink = new TAGValueSink(Processor);
                using (var reader = new TAGReader(tagData))
                {
                  var tagFile = new TAGFile();

                  ReadResult = tagFile.Read(reader, sink);

                  // Notify the processor that all reading operations have completed for the file
                  Processor.DoPostProcessFileAction(ReadResult == TAGReadResult.NoError);

                  SetPublishedState(Processor);

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
