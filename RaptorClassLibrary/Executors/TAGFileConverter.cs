﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Sinks;
using VSS.VisionLink.Raptor.TAGFiles.Classes.States;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.Executors
{
    /// <summary>
    /// Converts a TAG file from the vector based measurements of the machine's operation into the cell pass
    /// and events based description used in a Raptor data model
    /// </summary>
    public class TAGFileConverter
    {
        /// <summary>
        /// The overall result of processign the TAG information in the file
        /// </summary>
        public TAGReadResult ReadResult { get; set; } = TAGReadResult.NoError;

        /// <summary>
        /// The number of measurement epochs encountered in the TAG data
        /// </summary>
        public int ProcessedEpochCount { get; set; } = 0;

        /// <summary>
        /// The numebr of cell passes generatyed from the cell data
        /// </summary>
        public int ProcessedCellPassCount { get; set; } = 0;

        /// <summary>
        /// The target site model representing the ultimate recipient of the cell pass and event 
        /// generated from the TAG file
        /// </summary>
        public SiteModel SiteModel { get; set; } = null;

        /// <summary>
        /// The target machine within the target site model that generated the TAG file being processed
        /// </summary>
        public Machine Machine { get; set; } = null;

        /// <summary>
        /// The events from the primary target site model
        /// </summary>
        public ProductionEventChanges Events = null;


        /// <summary>
        // SiteModelGridAggregator is an object that aggregates all the cell passes generated while processing the file. 
        // These are then integrated into the primary site model in a single step at a later point in processing
        /// </summary>
        public ServerSubGridTree SiteModelGridAggregator { get; set; } = null;

        /// <summary>
        // MachineTargetValueChangesAggregator is an object that aggregates all the
        // machine state events of interest that we encounter while processing the
        // file. These are then integrated into the machine events in a single step
        // at a later point in processing
        /// </summary>
        public ProductionEventChanges MachineTargetValueChangesAggregator { get; set; } = null;

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

            SiteModel = new SiteModel(-1);
            Events = new ProductionEventChanges(SiteModel, 0 /*Machine.ID*/);

            Machine = new Machine()
            {
                TargetValueChanges = Events
            };

            SiteModelGridAggregator = new ServerSubGridTree(SiteModel);
            if (SiteModel.Grid != null)
            {
                SiteModelGridAggregator.CellSize = SiteModel.Grid.CellSize;
            }

            MachineTargetValueChangesAggregator = new ProductionEventChanges(SiteModel, long.MaxValue);
        }

        /// <summary>
        /// Fill out the local class properties with the information wanted from the TAG file
        /// </summary>
        /// <param name="Processor"></param>
        private void SetPublishedState(TAGProcessor Processor)
        {
            ProcessedEpochCount = Processor.ProcessedEpochCount;
            ProcessedCellPassCount = Processor.ProcessedCellPassesCount;
        }

        /// <summary>
        /// Execute the conversion operation on the TAG file, returning a booleam success result.
        /// Sets up local state detailing the prescan fields retried from the ATG file
        /// </summary>
        /// <param name="TAGData"></param>
        /// <returns></returns>
        public bool Execute(Stream TAGData)
        {
            try
            {
                Initialise();

                TAGProcessor Processor = new TAGProcessor(SiteModel, Machine, Events, SiteModelGridAggregator, MachineTargetValueChangesAggregator);
                TAGValueSink Sink = new TAGValueSink(Processor);
                TAGReader Reader = new TAGReader(TAGData);
                TAGFile TagFile = new TAGFile();

                ReadResult = TagFile.Read(Reader, Sink);

                if (ReadResult != TAGReadResult.NoError)
                {
                    return false;
                }

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
