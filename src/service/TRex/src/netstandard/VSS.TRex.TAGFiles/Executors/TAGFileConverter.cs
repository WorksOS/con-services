using System;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Logging;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Executors
{
    /// <summary>
    /// Converts a TAG file from the vector based measurements of the machine's operation into the cell pass
    /// and events based description used in a TRex data model
    /// </summary>
    public class TAGFileConverter
    {
      private static ILogger Log = Logger.CreateLogger<TAGFileConverter>();

        //private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The overall result of processign the TAG information in the file
        /// </summary>
        public TAGReadResult ReadResult { get; set; } = TAGReadResult.NoError;

        /// <summary>
        /// The number of measurement epochs encountered in the TAG data
        /// </summary>
        public int ProcessedEpochCount { get; set; }

        /// <summary>
        /// The numebr of cell passes generated from the cell data
        /// </summary>
        public int ProcessedCellPassCount { get; set; }

        /// <summary>
        /// The target site model representing the ultimate recipient of the cell pass and event 
        /// generated from the TAG file
        /// </summary>
        public ISiteModel SiteModel { get; set; }

        /// <summary>
        /// The target machine within the target site model that generated the TAG file being processed
        /// </summary>
        public IMachine Machine { get; set; }

        /// <summary>
        // SiteModelGridAggregator is an object that aggregates all the cell passes generated while processing the file. 
        // These are then integrated into the primary site model in a single step at a later point in processing
        /// </summary>
        public IServerSubGridTree SiteModelGridAggregator { get; set; }

        /// <summary>
        // MachineTargetValueChangesAggregator is an object that aggregates all the
        // machine state events of interest that we encounter while processing the
        // file. These are then integrated into the machine events in a single step
        // at a later point in processing
        /// </summary>
        public ProductionEventLists MachineTargetValueChangesAggregator { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public TAGFileConverter()
        {
        }

      private void Initialise()
      {
        ProcessedEpochCount = 0;
        ProcessedCellPassCount = 0;

        // Note: Intermediary TAG file processing contexts don't store their data to any persistence context
        // so the SiteModel constructed to contain the data processed from a TAG file does not need a 
        // storage proxy assigned to it
        SiteModel = DIContext.Obtain<ISiteModelFactory>().NewSiteModel();
        Machine = new Machine();

        SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID);
        if (SiteModel.Grid != null)
        {
          SiteModelGridAggregator.CellSize = SiteModel.Grid.CellSize;
        }

        MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      }

      /// <summary>
        /// Fill out the local class properties with the information wanted from the TAG file
        /// </summary>
        /// <param name="Processor"></param>
        private void SetPublishedState(TAGProcessor Processor)
        {
            ProcessedEpochCount = Processor.ProcessedEpochCount;
            ProcessedCellPassCount = Processor.ProcessedCellPassesCount;

            // Set the site model's last modified date...
            SiteModel.LastModifiedDate = DateTime.UtcNow;
        
            //Update latest status for the machine
            Machine.LastKnownX = Processor.DataLeft.X;
            Machine.LastKnownY = Processor.DataLeft.Y;
            Machine.LastKnownPositionTimeStamp = Processor.DataTime;
            Machine.MachineHardwareID = Processor.HardwareID;
            Machine.MachineType = Processor.MachineType;
        }

        /// <summary>
        /// Execute the conversion operation on the TAG file, returning a boolean success result.
        /// Sets up local state detailing the pre-scan fields retried from the ATG file
        /// </summary>
        /// <param name="TAGData"></param>
        /// <returns></returns>
        public bool Execute(Stream TAGData)
        {
            try
            {
                Initialise();

                TAGProcessor Processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);
                TAGValueSink Sink = new TAGValueSink(Processor);
                TAGReader Reader = new TAGReader(TAGData);
                TAGFile TagFile = new TAGFile();

                ReadResult = TagFile.Read(Reader, Sink);

                // Notify the Processor that all reading operations have completed for the file
                Processor.DoPostProcessFileAction(ReadResult == TAGReadResult.NoError);

                if (ReadResult != TAGReadResult.NoError)
                    return false;

                SetPublishedState(Processor);
            }
            catch (Exception e) // make sure any exception is trapped to return correct response to caller
            {
                Log.LogError("Exception occurred while converting a TAG file", e);
                return false;
            }

            return true;
        }
    }
}
