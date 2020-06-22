﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.IO.Helpers;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.Types;
using SubGridUtilities = VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// The core class containing a description of all cell passes recorded within the spatial confines
    /// of a sub grid on the ground.
    /// </summary>
    public class ServerSubGridTreeLeaf : ServerLeafSubGridBase, IServerLeafSubGrid
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<ServerSubGridTreeLeaf>();

        private long _version = 1;

        /// <summary>
        /// The version number of this segment when it is stored in the persistent layer, defined
        /// as the number of ticks in DateTime.UtcNow at the time it is written.
        /// </summary>
        public long Version { get => _version; set => _version = value; }
          
        /// <summary>
        /// Controls whether segment and cell pass information held within this sub grid is represented
        /// in the mutable or immutable forms supported by TRex
        /// </summary>
        public bool IsMutable { get; private set; }

        public void SetIsMutable(bool isMutable)
        {
          IsMutable = isMutable;
          _directory = new SubGridDirectory
          {
            IsMutable = isMutable
          };
        }

        /// <summary>
        /// Does this sub grid contain directory information for all the segments that exist within it?
        /// </summary>
        public bool HasSubGridDirectoryDetails => _directory.SegmentDirectory.Count > 0;

        private DateTime _leafEndTime;
        /// <summary>
        /// The date time of the last observed cell pass within this sub grid
        /// </summary>
        public DateTime LeafEndTime { get => _leafEndTime; set => _leafEndTime = value; }

        private DateTime _leafStartTime;
        /// <summary>
        /// The date time of the first observed cell pass within this sub grid
        /// </summary>
        public DateTime LeafStartTime { get => _leafStartTime; set => _leafStartTime = value; }

        private ISubGridDirectory _directory;
        /// <summary>
        /// A directory containing metadata regarding the segments present within this sub grid
        /// </summary>
        public ISubGridDirectory Directory { get => _directory; set => _directory = value; }

        private ISubGridCellPassesDataWrapper _cells;
        /// <summary>
        /// The primary wrapper containing all segments that have been loaded
        /// </summary>
        public ISubGridCellPassesDataWrapper Cells { get => _cells; set => _cells = value; } // Use AllocateLeafFullPassStacks() to create new SubGridCellPassesDataWrapper();

        /// <summary>
        /// 
        /// </summary>
        private void InitialiseStartEndTime()
        {
            LeafStartTime = Consts.MAX_DATETIME_AS_UTC;
            LeafEndTime = Consts.MIN_DATETIME_AS_UTC; 
        }

        /// <summary>
        /// Clears the state of the segment to be empty, with a null date range, no cells and no segments
        /// </summary>
        public override void Clear()
        {
            InitialiseStartEndTime();

            _cells?.Clear();
            _directory?.Clear();
        }

        private void CellPassAdded(CellPass pass)
        {
            UpdateStartEndTimeRange(pass.Time);
             
            SetDirty();
        }

        /// <summary>
        /// Takes a date/time and expands the sub grid leaf time range to include it if necessary
        /// </summary>
        /// <param name="time"></param>
        private void UpdateStartEndTimeRange(DateTime time)
        {
            if (time < _leafStartTime)
                _leafStartTime = time;

            if (time > _leafEndTime)
                _leafEndTime = time;
        }

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public ServerSubGridTreeLeaf()
        {
        }

        public ServerSubGridTreeLeaf(ISubGridTree owner,
                                     ISubGrid parent,
                                     byte level,
                                     StorageMutability mutability) : base(owner, parent, level)
        {
          SetIsMutable(mutability == StorageMutability.Mutable);
        }

        public void AddPass(int cellX, int cellY, CellPass Pass, bool lowestPassOnly = false)
        {
            var Segment = _cells.SelectSegment(Pass.Time);

            if (Segment == null)
                throw new TRexSubGridTreeException("Cells.SelectSegment failed to return a segment");

            if (Pass.Time == Consts.MIN_DATETIME_AS_UTC || Pass.Time.Kind != DateTimeKind.Utc)
                throw new TRexSubGridTreeException("Cell passes added to cell pass stacks must have a non-null, UTC, cell pass time");

            if (!Segment.HasAllPasses)
                Segment.AllocateFullPassStacks();

            // Add the processed pass to the cell
            if (Segment.PassesData.PassCount(cellX, cellY) == 0)
            {
              Segment.PassesData.AddPass(cellX, cellY, Pass);
              CellPassAdded(Pass);
            }
            else if (Segment.PassesData.LocateTime(cellX, cellY, Pass.Time, out int PassIndex))
            {
                // Replace the existing cell pass with the new one. The assumption
                // here is that more than one machine will never cross a cell center position
                // within the resolution of the cell pass time stamps
                Segment.PassesData.ReplacePass(cellX, cellY, PassIndex, Pass);

                SetDirty(); 
            }
            else
            {
              if (lowestPassOnly)
              {
                if (Pass.Height < Segment.PassesData.Pass(cellX,cellY,0).Height)
                {
                  Segment.PassesData.ReplacePass(cellX, cellY, 0, Pass);
                  SetDirty();
                }
              }
              else
              {
                Segment.PassesData.AddPass(cellX, cellY, Pass);
                CellPassAdded(Pass);
              }
            }

          Segment.Dirty = true;
        }

        /// <summary>
        /// Creates the default segment metadata within the segment directory. This is only called to create the first 
        /// segment metadata spanning the entire time range.
        /// </summary>
        public void CreateDefaultSegment()
        {
            _directory.CreateDefaultSegment();
        }

        public void AllocateFullPassStacks(ISubGridCellPassesDataSegmentInfo SegmentInfo)
        {
            if (SegmentInfo.Segment == null)
            {
                AllocateSegment(SegmentInfo);
            }

            SegmentInfo.Segment?.AllocateFullPassStacks();
            //                FCachedMemorySizeOutOfDate:= True;
        }

        public void AllocateLatestPassGrid(ISubGridCellPassesDataSegmentInfo SegmentInfo)
        {
            if (SegmentInfo.Segment == null)
            {
                AllocateSegment(SegmentInfo);
            }

            SegmentInfo.Segment?.AllocateLatestPassGrid();
            //                FCachedMemorySizeOutOfDate:= True;
        }

        public bool HasAllCellPasses() => _cells != null;

        public void AllocateLeafFullPassStacks()
        {
            if (_cells == null)
            {
                //                Include(FLeafStorageClasses, icsscAllPasses);
                _cells = new SubGridCellPassesDataWrapper
                {
                    Owner = this
                };

                //       FCachedMemorySizeOutOfDate:= True;
            }
        }

        public void DeAllocateLeafFullPassStacks()
        {
          _cells?.PassesData?.Items.ForEach(x => x.Dispose());
          _cells = null;
        }

        public bool HasLatestData() => _directory.GlobalLatestCells != null;

        public void AllocateLeafLatestPassGrid()
        {
            if (_directory.GlobalLatestCells == null)
            {
                // Include(FLeafStorageClasses, icsscLatestData);
                _directory.AllocateGlobalLatestCells();

                // FCachedMemorySizeOutOfDate:= True;
            }
        }

        public void DeAllocateLeafLatestPassGrid()
        {
            if (_directory != null)
            {
                _directory.GlobalLatestCells?.Dispose();
                _directory.GlobalLatestCells = null;
            }
        }

        /// <summary>
        /// Certain types of grid attribute data requests may need us to select
        /// a pass that is not the latest pass in the pass list. Such an instance is
        /// when request CCV value where null CCV values are passed over in favor of
        /// non-null CCV values in passes that are older in the pass list for the cell.
        /// Important: Also see the PassIsAcceptable() function in
        /// CellPassAttributeFilter.FilterSinglePass() to ensure that the logic
        /// here is consistent (or at least not contradictory) with the logic here.
        /// The checks are duplicated as there may be different logic applied to the
        /// selection of the 'latest' pass from a cell pass state versus selection of
        /// an appropriate filtered pass given other filtering criteria in play.
        /// </summary>
        /// <param name="LastPassIndex"></param>
        /// <param name="TypeToCheck"></param>
        /// <param name="ValueFromLatestCellPass"></param>
        /// <param name="CellPasses"></param>
        /// <param name="LatestData"></param>
        private void GetAppropriateLatestValueFor(Cell_NonStatic CellPasses,
                                                  ref CellPass LatestData,
                                                  int LastPassIndex,
                                                  GridDataType TypeToCheck,
                                                  out bool ValueFromLatestCellPass)
        {

            ValueFromLatestCellPass = false;

            LastPassIndex += CellPasses.Passes.Offset;
            for (int I = LastPassIndex, limit = CellPasses.Passes.Offset; I >= limit; I--)
            {
                var pass = CellPasses.Passes.Elements[I];

                switch (TypeToCheck)
                {
                    case GridDataType.CCV:
                        if (pass.CCV != CellPassConsts.NullCCV)
                        {
                            LatestData.CCV = pass.CCV;
                            ValueFromLatestCellPass = I == LastPassIndex;
                        }
                        break;

                    case GridDataType.RMV:
                        if (pass.RMV != CellPassConsts.NullRMV)
                        {
                            LatestData.RMV = pass.RMV;
                            ValueFromLatestCellPass = I == LastPassIndex;
                        }
                        break;

                    case GridDataType.Frequency:
                        if (pass.Frequency != CellPassConsts.NullFrequency)
                        {
                            LatestData.Frequency = pass.Frequency;
                            ValueFromLatestCellPass = I == LastPassIndex;
                        }
                        break;

                    case GridDataType.Amplitude:
                        if (pass.Amplitude != CellPassConsts.NullAmplitude)
                        {
                            LatestData.Amplitude = pass.Amplitude;
                            ValueFromLatestCellPass = I == LastPassIndex;
                        }
                        break;

                    case GridDataType.GPSMode:
                        {
                            // Also grab flags for half pass and rear axle
                            LatestData.HalfPass = pass.HalfPass;
                            LatestData.PassType = pass.PassType;

                            if (pass.gpsMode != CellPassConsts.NullGPSMode)
                            {
                                LatestData.gpsMode = pass.gpsMode;
                                ValueFromLatestCellPass = I == LastPassIndex;
                            }
                        }
                        break;

                    case GridDataType.Temperature:
                        if (pass.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue)
                        {
                            LatestData.MaterialTemperature = pass.MaterialTemperature;
                            ValueFromLatestCellPass = I == LastPassIndex;
                        }
                        break;

                    case GridDataType.MDP:
                        if (pass.MDP != CellPassConsts.NullMDP)
                        {
                            LatestData.MDP = pass.MDP;
                            ValueFromLatestCellPass = I == LastPassIndex;
                        }
                        break;

                    case GridDataType.CCA:
                        if (pass.CCA != CellPassConsts.NullCCA)
                        {
                            LatestData.CCA = pass.CCA;
                            ValueFromLatestCellPass = I == LastPassIndex;
                        }
                        break;
                }
            }
        }

        private void CalculateLatestPassDataForPassStack(Cell_NonStatic CellPasses,
                                                         ref CellPass LatestData,
                                                         out bool CCVFromLatestCellPass,
                                                         out bool RMVFromLatestCellPass,
                                                         out bool FrequencyFromLatestCellPass,
                                                         out bool AmplitudeFromLatestCellPass,
                                                         out bool TemperatureFromLatestCellPass,
                                                         out bool GPSModeFromLatestCellPass,
                                                         out bool MDPFromLatestCellPass,
                                                         out bool CCAFromLatestCellPass)
        {
            int LastPassIndex = CellPasses.PassCount - 1;

            if (LastPassIndex >= 0)
            {
              var lastCellPass = CellPasses.Passes.GetElement(LastPassIndex);

              LatestData.Time = lastCellPass.Time;
              LatestData.InternalSiteModelMachineIndex = lastCellPass.InternalSiteModelMachineIndex;

              if (lastCellPass.Height != Consts.NullHeight)
                LatestData.Height = lastCellPass.Height;

              if (lastCellPass.RadioLatency != CellPassConsts.NullRadioLatency)
                LatestData.RadioLatency = lastCellPass.RadioLatency;

              LatestData.MachineSpeed = lastCellPass.MachineSpeed;
            }

            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.GPSMode, out GPSModeFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.CCV, out CCVFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.RMV, out RMVFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.Frequency, out FrequencyFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.Amplitude, out AmplitudeFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.Temperature, out TemperatureFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.MDP, out MDPFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.CCA, out CCAFromLatestCellPass);
        }

        public void AllocateSegment(ISubGridCellPassesDataSegmentInfo segmentInfo)
        {
            if (segmentInfo.Segment != null)
              throw new TRexSubGridTreeException("Cannot allocate a segment that is already allocated");

            _cells.PassesData.AddNewSegment(this, segmentInfo);

            //        CachedMemorySizeOutOfDate:= True;
        }

        public override bool CellHasValue(byte CellX, byte CellY)
        {
            return _directory.GlobalLatestCells.PassDataExistenceMap.BitSet(CellX, CellY);
        }

        private void CalculateLatestPassGridForSegment(ISubGridCellPassesDataSegment Segment,
                                                       ISubGridCellPassesDataSegment TemporallyPrecedingSegment)
        {
            if (Segment.PassesData == null)
              throw new TRexSubGridTreeException($"{nameof(CalculateLatestPassGridForSegment)} passed a segment in {Moniker()} with no cell passes allocated");

            if (_cells == null)
              throw new TRexSubGridTreeException($"Cell passes store for {Moniker()} not instantiated");

            Segment.AllocateLatestPassGrid();

            var segment_passesData = Segment.PassesData;
            var segment_latestPasses = Segment.LatestPasses;
            segment_latestPasses.Clear();
            Segment.Dirty = true;

            var segment_latestPasses_PassDataExistenceMap = segment_latestPasses.PassDataExistenceMap;
            var segment_latestPasses_CCVValuesAreFromLastPass = segment_latestPasses.CCVValuesAreFromLastPass;
            var segment_latestPasses_RMVValuesAreFromLastPass = segment_latestPasses.RMVValuesAreFromLastPass;
            var segment_latestPasses_FrequencyValuesAreFromLastPass = segment_latestPasses.FrequencyValuesAreFromLastPass;
            var segment_latestPasses_AmplitudeValuesAreFromLastPass = segment_latestPasses.AmplitudeValuesAreFromLastPass;
            var segment_latestPasses_GPSModeValuesAreFromLatestCellPass = segment_latestPasses.GPSModeValuesAreFromLatestCellPass;
            var segment_latestPasses_TemperatureValuesAreFromLastPass = segment_latestPasses.TemperatureValuesAreFromLastPass;
            var segment_latestPasses_MDPValuesAreFromLastPass = segment_latestPasses.MDPValuesAreFromLastPass;
            var segment_latestPasses_CCAValuesAreFromLastPass = segment_latestPasses.CCAValuesAreFromLastPass;

            var temporallyPrecedingSegmentLatestPasses = TemporallyPrecedingSegment?.LatestPasses;

            // Seed the latest value tags for this segment with the latest data from the previous segment
            if (temporallyPrecedingSegmentLatestPasses != null)
            {
              segment_latestPasses.AssignValuesFromLastPassFlags(temporallyPrecedingSegmentLatestPasses);
            }

            // Iterate over the values in the child leaf sub grid looking for
            // the first cell with passes in it
            SubGridUtilities.SubGridDimensionalIterator((I, J) =>
            {
                bool UpdatedCell = false;

                if (temporallyPrecedingSegmentLatestPasses != null &&
                    temporallyPrecedingSegmentLatestPasses.PassDataExistenceMap.BitSet(I, J))
                {
                // Seed the latest data for this segment with the latest data from the previous segment
                    segment_latestPasses[I, J] = temporallyPrecedingSegmentLatestPasses[I, J];

                    UpdatedCell = true;
                }

                // Update the latest data from any previous segment with the information contained in this segment
                if (segment_passesData.PassCount(I, J) > 0)
                {
                     var latestPass = ((SubGridCellLatestPassDataWrapper_NonStatic) segment_latestPasses)[I, J];

                     CalculateLatestPassDataForPassStack(segment_passesData.ExtractCellPasses(I, J),
                        ref latestPass,
                        out bool CCVFromLatestCellPass,
                        out bool RMVFromLatestCellPass,
                        out bool FrequencyFromLatestCellPass,
                        out bool AmplitudeFromLatestCellPass,
                        out bool TemperatureFromLatestCellPass,
                        out bool GPSModeFromLatestCellPass,
                        out bool MDPFromLatestCellPass,
                        out bool CCAFromLatestCellPass);

                     ((SubGridCellLatestPassDataWrapper_NonStatic) segment_latestPasses)[I, J] = latestPass;

                     segment_latestPasses_CCVValuesAreFromLastPass.SetBitValue(I, J, CCVFromLatestCellPass);
                     segment_latestPasses_RMVValuesAreFromLastPass.SetBitValue(I, J, RMVFromLatestCellPass);
                     segment_latestPasses_FrequencyValuesAreFromLastPass.SetBitValue(I, J, FrequencyFromLatestCellPass);
                     segment_latestPasses_AmplitudeValuesAreFromLastPass.SetBitValue(I, J, AmplitudeFromLatestCellPass);
                     segment_latestPasses_GPSModeValuesAreFromLatestCellPass.SetBitValue(I, J, GPSModeFromLatestCellPass);
                     segment_latestPasses_TemperatureValuesAreFromLastPass.SetBitValue(I, J, TemperatureFromLatestCellPass);
                     segment_latestPasses_MDPValuesAreFromLastPass.SetBitValue(I, J, MDPFromLatestCellPass);
                     segment_latestPasses_CCAValuesAreFromLastPass.SetBitValue(I, J, CCAFromLatestCellPass);
              
                     UpdatedCell = true;
                }
                else
                {
                    if (temporallyPrecedingSegmentLatestPasses != null)
                    {
                        segment_latestPasses_CCVValuesAreFromLastPass.SetBitValue(I, J, temporallyPrecedingSegmentLatestPasses.CCVValuesAreFromLastPass.BitSet(I, J));
                        segment_latestPasses_RMVValuesAreFromLastPass.SetBitValue(I, J, temporallyPrecedingSegmentLatestPasses.RMVValuesAreFromLastPass.BitSet(I, J));
                        segment_latestPasses_FrequencyValuesAreFromLastPass.SetBitValue(I, J, temporallyPrecedingSegmentLatestPasses.FrequencyValuesAreFromLastPass.BitSet(I, J));
                        segment_latestPasses_AmplitudeValuesAreFromLastPass.SetBitValue(I, J, temporallyPrecedingSegmentLatestPasses.AmplitudeValuesAreFromLastPass.BitSet(I, J));
                        segment_latestPasses_GPSModeValuesAreFromLatestCellPass.SetBitValue(I, J, temporallyPrecedingSegmentLatestPasses.GPSModeValuesAreFromLatestCellPass.BitSet(I, J));
                        segment_latestPasses_TemperatureValuesAreFromLastPass.SetBitValue(I, J, temporallyPrecedingSegmentLatestPasses.TemperatureValuesAreFromLastPass.BitSet(I, J));
                        segment_latestPasses_MDPValuesAreFromLastPass.SetBitValue(I, J, temporallyPrecedingSegmentLatestPasses.MDPValuesAreFromLastPass.BitSet(I, J));
                        segment_latestPasses_CCAValuesAreFromLastPass.SetBitValue(I, J, temporallyPrecedingSegmentLatestPasses.CCAValuesAreFromLastPass.BitSet(I, J));
                    }
                }

                if (UpdatedCell)
                  segment_latestPasses_PassDataExistenceMap.SetBit(I, J);
            });
        }

        private void CalculateLatestPassGridForAllSegments()
        {
            AllocateLeafLatestPassGrid();

            // This statement does assume that the last segment has at least it's latest
            // passes in the cache. This is, currently, a safe assumption as the directory
            // is only written in response to changes in the cell passes in the segments,
            // which in turn will cause the latest cells in the affected segments to be
            // modified which will always cause the latest cells in the latest segment to be
            // modified.
            var Segment = _directory.SegmentDirectory.Last().Segment;

            var _GlobalLatestCells = _directory.GlobalLatestCells;
            var _LatestPasses = Segment.LatestPasses;

            if (_LatestPasses != null)
            {
              _GlobalLatestCells.Clear();
              _GlobalLatestCells.Assign(_LatestPasses);

              var __GlobalLatestCells = (SubGridCellLatestPassDataWrapper_NonStatic) _GlobalLatestCells;
              var __LatestPasses = (SubGridCellLatestPassDataWrapper_NonStatic) _LatestPasses;

              Segment.LatestPasses.PassDataExistenceMap.ForEachSetBit((x, y) => __GlobalLatestCells[x, y] = __LatestPasses[x, y]);
            }
        }

        public void ComputeLatestPassInformation(bool fullRecompute, IStorageProxy storageProxy)
        {
            //Log.LogInformation($"ComputeLatestPassInformation: Segment dir for {Moniker()}:");
            //Directory.DumpSegmentDirectoryToLog();

            if (!Dirty)
            {
              throw new TRexSubGridTreeException($"Sub grid {Moniker()} not marked as dirty when computing latest pass information");
            }

            var Iterator = new SubGridSegmentIterator(this, _directory, storageProxy)
            {
                IterationDirection = IterationDirection.Forwards,
                ReturnDirtyOnly = !fullRecompute
            };
            int NumProcessedSegments = 0;

            // We are in the process of recalculating latest data, so don't ask the iterator to
            // read the latest data information as it will be reconstructed here. The full cell pass
            // stacks are required though...
            Iterator.RetrieveLatestData = false;
            Iterator.RetrieveAllPasses = true;

            ISubGridCellPassesDataSegmentInfo SeedSegmentInfo = null;
            ISubGridCellPassesDataSegment LastSegment = null;

            // Locate the segment immediately previous to the first dirty segment in the list of segments

            var segmentDirectory = _directory.SegmentDirectory;
            for (int I = 0, limit = segmentDirectory.Count; I < limit; I++)
            {
                if (segmentDirectory[I].Segment != null && segmentDirectory[I].Segment.Dirty)
                {
                    if (I > 0)
                    {
                        SeedSegmentInfo = segmentDirectory[I - 1];
                    }
                    break;
                }
            }

            // If we chose the first segment and it was dirty, then clear it
            if (SeedSegmentInfo?.Segment != null)
            {
                LastSegment = SeedSegmentInfo.Segment;
            }

            // If there was such a last segment, then make sure its latest pass information
            // has been read from the store

            if (SeedSegmentInfo != null && SeedSegmentInfo.ExistsInPersistentStore &&
               (SeedSegmentInfo.Segment == null || !SeedSegmentInfo.Segment.HasLatestData))
            {
                if (SeedSegmentInfo.Segment == null)
                {
                    AllocateSegment(SeedSegmentInfo);
                }

                if (SeedSegmentInfo.Segment != null)
                {
                    if (((ServerSubGridTree)Owner).LoadLeafSubGridSegment(storageProxy, new SubGridCellAddress(OriginX, OriginY), true, false,
                                                                          this, SeedSegmentInfo.Segment))
                    {
                        LastSegment = SeedSegmentInfo.Segment;
                    }
                    else
                    {
                        Log.LogCritical($"Failed to load segment from sub grid where segment was marked as present in persistent store for {new SubGridCellAddress(OriginX, OriginY)}");
                    }
                }
            }

            // The first MoveNext will locate the first segment in the directory marked as dirty
            while (Iterator.MoveNext())
            {
                if (Iterator.CurrentSubGridSegment == null && !Iterator.CurrentSubGridSegment.Dirty)
                {
                    throw new TRexException($"Iterator returned non-dirty segment in ComputeLatestPassInformation for {Moniker()}");
                }

                NumProcessedSegments++;

                CalculateLatestPassGridForSegment(Iterator.CurrentSubGridSegment, LastSegment);

                LastSegment = Iterator.CurrentSubGridSegment;

                // A segment has been processed... By definition, all segments after the first segment must have the
                // latest values processed, so instruct the iterator to return all segments from now on
                Iterator.ReturnDirtyOnly = false;
            }

            // Note: It is possible that there were no processed segments (NumProcessedSegments = 0) as a result of processing
            // a TAG file that caused no changes to the database (e.g. it had been processed earlier)
            if (NumProcessedSegments > 0)
            {
                // Now compute the final global latest pass data for the directory (though this will be the
                // same as the last segment)
                CalculateLatestPassGridForAllSegments();
            }

            //Log.LogInformation($"Completed ComputeLatestPassInformation for {Moniker()}");
        }

        public bool LoadSegmentFromStorage(IStorageProxy storageProxy, string FileName, ISubGridCellPassesDataSegment Segment, bool loadLatestData, bool loadAllPasses)
        {
            if (loadAllPasses && Segment.Dirty)
            {
                Log.LogCritical("Leaf sub grid segment loads of cell pass data may not be performed while the segment is dirty. The information should be taken from the cache instead");
                return false;
            }

             var FSError = storageProxy.ReadSpatialStreamFromPersistentStore
              (Owner.ID, FileName, OriginX, OriginY, 
               Segment.SegmentInfo.StartTime.Ticks,
               Segment.SegmentInfo.EndTime.Ticks,
               Segment.SegmentInfo.Version, 
               FileSystemStreamType.SubGridSegment, out MemoryStream SMS);

             bool Result = FSError == FileSystemErrorStatus.OK;

             try
             {
               if (!Result)
               {
                 Log.LogError(FSError == FileSystemErrorStatus.FileDoesNotExist
                   ? $"Expected leaf sub grid segment {FileName}, model {Owner.ID} does not exist."
                   : $"Unable to load leaf sub grid segment {FileName}, model {Owner.ID}. Details: {FSError}");
               }
               else
               {
                 SMS.Position = 0;
                 using (var reader = new BinaryReader(SMS, Encoding.UTF8, true))
                 {
                   Result = Segment.Read(reader, loadLatestData, loadAllPasses);

                   if (loadAllPasses && Segment.PassesData == null)
                     Log.LogError(
                       $"Segment {FileName} passes data is null after reading from store with LoadAllPasses=true.");
                 }
               }
             }
             finally
             {
                SMS?.Dispose();
             }

             return Result;
        }

        public bool RemoveSegmentFromStorage(IStorageProxy storageProxy, string fileName, ISubGridCellPassesDataSegmentInfo segmentInfo)
        {
          var fsError = storageProxy.RemoveSpatialStreamFromPersistentStore
          (Owner.ID, fileName, OriginX, OriginY,
            segmentInfo.StartTime.Ticks,
            segmentInfo.EndTime.Ticks,
            segmentInfo.Version,
            FileSystemStreamType.SubGridSegment);

          var result = fsError == FileSystemErrorStatus.OK;

          if (!result)
          {
            Log.LogError(fsError == FileSystemErrorStatus.FileDoesNotExist
              ? $"Expected leaf sub grid segment {fileName}, model {Owner.ID} does not exist."
              : $"Unable to load leaf sub grid segment {fileName}, model {Owner.ID}. Details: {fsError}");
          }

          return result;
        }

        public bool SaveDirectoryToStream(Stream stream)
        {
          using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
          {
            var Header = new SubGridStreamHeader
            {
              Identifier = SubGridStreamHeader.kICServerSubGridDirectoryFileMoniker,
              Flags = SubGridStreamHeader.kSubGridHeaderFlag_IsSubGridDirectoryFile,
              StartTime = _leafStartTime,
              EndTime = _leafEndTime,
              LastUpdateTimeUTC = DateTime.UtcNow
            };

            // Write the header/version to the stream
            Header.Write(writer);

            _directory.Write(writer);
          }

          return true;
        }

        public bool SaveDirectoryToFile(IStorageProxy storage,
                                        string FileName)
        {
          using (var stream = RecyclableMemoryStreamManagerHelper.Manager.GetStream())
          {
            SaveDirectoryToStream(stream);

            bool Result = storage.WriteSpatialStreamToPersistentStore
                          (Owner.ID, FileName, OriginX, OriginY, -1, -1, _version,
                            FileSystemStreamType.SubGridDirectory, stream, this) == FileSystemErrorStatus.OK;

            if (!Result)
              Log.LogWarning($"Call to WriteSpatialStreamToPersistentStore failed. Filename:{FileName}");

            return Result;
          }
        }

      /// <summary>
      /// Generates the affinity key for this sub grid that identifies this element in the persistent data store
      /// </summary>
      /// <returns></returns>
      public ISubGridSpatialAffinityKey AffinityKey() => new SubGridSpatialAffinityKey(_version, Owner.ID, OriginX, OriginY);

      public bool LoadDirectoryFromStream(Stream stream)
        {
          using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
          {
            var Header = new SubGridStreamHeader(reader);

            // long LatestCellPassDataSize;
            // long CellPassStacksDataSize;

            if (!Header.IdentifierMatches(SubGridStreamHeader.kICServerSubGridDirectoryFileMoniker))
            {
              Log.LogError(
                $"Sub grid directory file header mismatch (expected [Header: {SubGridStreamHeader.kICServerSubGridDirectoryFileMoniker}, found {Header.Identifier}]).");
              return false;
            }

            if (!Header.IsSubGridDirectoryFile)
              throw new TRexSubGridIOException(
                "Sub grid directory file does not identify itself as such in extended header flags");

            //  FLastUpdateTimeUTC := Header.LastUpdateTimeUTC;
            _leafStartTime = Header.StartTime;
            _leafEndTime = Header.EndTime;

            // Global latest cell passes are always read in from the sub grid directory, even if the 'latest
            // cells' storage class is not contained in the leaf storage classes. This is currently done due
            // to some operations (namely aggregation of processed cell passes into the production
            // data model) may request sub grids that have not yet been persisted to the data store.
            // Ultimately such requests result in the sub grid being read from disk if the storage classes
            // in the request do not match the storage classes of the leaf sub grid in the cache.
            // reading the latest cells does impose a small performance penalty, however, this
            // data is likely to be required in common use cases so we will load it until a
            // more concrete case for not doing this is made.
            _directory.AllocateGlobalLatestCells();

            if (Header.Version == 1)
              _directory.Read(reader); //, Directory.GlobalLatestCells.PassData, out LatestCellPassDataSize, out CellPassStacksDataSize);
            else
              Log.LogError(
                $"Sub grid directory file version or header mismatch (expected [Version: {SubGridStreamHeader.VERSION_NUMBER}, found {Header.Version}] [Header: {SubGridStreamHeader.kICServerSubGridDirectoryFileMoniker}, found {Header.Identifier}]).");
          }

          return true;
        }

        public bool LoadDirectoryFromFile(IStorageProxy storage, string fileName)
        {
           if (_version == 0)
             Log.LogError($"Version for {Moniker()} is 0");

            var FSError = storage.ReadSpatialStreamFromPersistentStore(Owner.ID, fileName, OriginX, OriginY, -1, -1, _version,
                                                                       FileSystemStreamType.SubGridDirectory, out MemoryStream SMS);
            try
            {
              if (FSError != FileSystemErrorStatus.OK || SMS == null)
              {
                if (FSError == FileSystemErrorStatus.FileDoesNotExist)
                  Log.LogError($"Expected leaf sub grid file {fileName} does not exist.");
                else if (FSError != FileSystemErrorStatus.SpatialStreamIndexGranuleLocationNull &&
                         FSError != FileSystemErrorStatus.GranuleDoesNotExist)
                  Log.LogWarning($"Unable to load leaf sub grid file '{fileName}'. Details: {FSError}");

                return false;
              }

              SMS.Position = 0;
              return LoadDirectoryFromStream(SMS);
            }
            finally
            {
              SMS?.Dispose();
            }
        }

        /// <summary>
        /// Deletes the directory file from the persistent store
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool RemoveDirectoryFromStorage(IStorageProxy storage, string fileName)
        {
            var FSError = storage.RemoveSpatialStreamFromPersistentStore(Owner.ID, fileName, OriginX, OriginY, -1, -1, _version,
            FileSystemStreamType.SubGridDirectory);

            if (FSError != FileSystemErrorStatus.OK)
            {
                if (FSError == FileSystemErrorStatus.FileDoesNotExist)
                { 
                    Log.LogError($"Expected leaf sub grid file {fileName} does not exist.");
                }
                else if (FSError != FileSystemErrorStatus.SpatialStreamIndexGranuleLocationNull &&
                         FSError != FileSystemErrorStatus.GranuleDoesNotExist)
                {
                  Log.LogWarning($"Unable to load leaf sub grid file '{fileName}'. Details: {FSError}");
                }

                return false;
            }

            return true;
        }

        public void Integrate(IServerLeafSubGrid source,
                              ISubGridSegmentIterator iterator,
                              bool integratingIntoIntermediaryGrid)
        {
            //Log.LogInformation($"Integrating sub grid {Moniker()}, intermediary?:{IntegratingIntoIntermediaryGrid}");

            if (source == null)
            {
              throw new TRexSubGridTreeException("Source sub grid not defined in ServerSubGridTreeLeaf.Integrate");
            }

            if (source.Cells.PassesData.Count == 0)
            {
                // No cells added to this sub grid during processing
                Log.LogCritical($"Empty sub grid {Moniker()} passed to Integrate");
                return;
            }

            if (source.Cells.PassesData.Count != 1)
            {
                Log.LogCritical($"source integrated sub grids must have only one segment in Integrate ({Moniker()})");
                return;
            }

            iterator.SubGrid = this;
            iterator.Directory = _directory;

            var sourceSegment = source.Cells.PassesData[0];

            UpdateStartEndTimeRange(source.LeafStartTime);
            UpdateStartEndTimeRange(source.LeafEndTime);

            for (var I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
            {
                for (var J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
                {
                    var sourceSegmentCell = sourceSegment.PassesData.ExtractCellPasses(I, J);
                    var sourceSegmentCellPasses = sourceSegmentCell.Passes;

                    if (sourceSegmentCellPasses.Count == 0)
                      continue;

                    // Perform the physical integration of the new cell passes into the target sub grid
                    var sourceSegmentStartIndex = sourceSegmentCellPasses.Offset;
                    var sourceSegmentFinalIndex = sourceSegmentCellPasses.OffsetPlusCount - 1;

                    var sourceElements = sourceSegmentCellPasses.Elements;

                    // Restrict the iterator to examining only those segments that fall within the
                    // time range covered by the passes in the cell being processed.
                    iterator.SetTimeRange(sourceElements[sourceSegmentStartIndex].Time,
                                          sourceElements[sourceSegmentFinalIndex].Time);

                    // Now iterate over the time bounded segments in the database and integrate the new cell passes
                    iterator.InitialiseIterator();
                    while (iterator.MoveToNextSubGridSegment())
                    {
                        var segment = iterator.CurrentSubGridSegment;

                        if (sourceSegmentStartIndex <= sourceSegmentFinalIndex && sourceElements[sourceSegmentStartIndex].Time >= segment.SegmentInfo.EndTime)
                            continue;

                        var endIndex = sourceSegmentStartIndex;
                        var endTime = segment.SegmentInfo.EndTime;

                        while (endIndex < sourceSegmentFinalIndex && sourceElements[endIndex + 1].Time < endTime)
                            endIndex++;

                        segment.PassesData.Integrate(I, J, sourceSegmentCell, 
                          sourceSegmentStartIndex - sourceSegmentCellPasses.Offset, endIndex - sourceSegmentCellPasses.Offset, 
                          out var addedCount, out var modifiedCount);

                        if (addedCount > 0 || modifiedCount > 0)
                            segment.Dirty = true;

                        if (addedCount != 0)
                            segment.PassesData.SegmentPassCount += addedCount;

                        sourceSegmentStartIndex = endIndex + 1;

                        if (sourceSegmentStartIndex >= sourceSegmentFinalIndex)
                            break; // We are finished
                    }
                }
            }

            //Log.LogInformation($"Completed integrating sub grid {Moniker()}, intermediary?:{IntegratingIntoIntermediaryGrid}, {AddedCount} cell passes added, {ModifiedCount} modified");

      // CachedMemorySizeOutOfDate = true;
    }

    /// <summary>
    /// Constructs a 'filename' representing this leaf sub grid
    /// </summary>
    /// <param name="Origin"></param>
    /// <returns></returns>
    public static string FileNameFromOriginPosition(SubGridCellAddress Origin) => $"{Origin.X:D10}-{Origin.Y:D10}";

    #region IDisposable Support
    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        // Treat disposal and finalization as the same, dependent on the primary disposedValue flag
        for (int i = 0, limit = Cells?.PassesData?.Count ?? 0; i < limit; i++)
        {
           Cells.PassesData[i]?.Dispose();
        }
        Directory?.Dispose();
        Directory = null;

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}

