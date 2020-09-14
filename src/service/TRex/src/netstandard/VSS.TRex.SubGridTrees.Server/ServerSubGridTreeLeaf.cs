using System;
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
        private static readonly ILogger _log = Logging.Logger.CreateLogger<ServerSubGridTreeLeaf>();

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

        public void AddPass(int cellX, int cellY, CellPass pass, bool lowestPassOnly = false)
        {
            var segment = _cells.SelectSegment(pass.Time);

            if (segment == null)
                throw new TRexSubGridTreeException("Cells.SelectSegment failed to return a segment");

            if (pass.Time == Consts.MIN_DATETIME_AS_UTC || pass.Time.Kind != DateTimeKind.Utc)
                throw new TRexSubGridTreeException("Cell passes added to cell pass stacks must have a non-null, UTC, cell pass time");

            if (!segment.HasAllPasses)
                segment.AllocateFullPassStacks();

            // Add the processed pass to the cell
            if (segment.PassesData.PassCount(cellX, cellY) == 0)
            {
              segment.PassesData.AddPass(cellX, cellY, pass);
              CellPassAdded(pass);
            }
            else if (segment.PassesData.LocateTime(cellX, cellY, pass.Time, out var passIndex))
            {
                // Replace the existing cell pass with the new one. The assumption
                // here is that more than one machine will never cross a cell center position
                // within the resolution of the cell pass time stamps
                segment.PassesData.ReplacePass(cellX, cellY, passIndex, pass);

                SetDirty();
            }
            else
            {
              if (lowestPassOnly)
              {
                if (pass.Height < segment.PassesData.Pass(cellX,cellY,0).Height)
                {
                  segment.PassesData.ReplacePass(cellX, cellY, 0, pass);
                  SetDirty();
                }
              }
              else
              {
                segment.PassesData.AddPass(cellX, cellY, pass);
                CellPassAdded(pass);
              }
            }

            segment.Dirty = true;
        }

        /// <summary>
        /// Creates the default segment metadata within the segment directory. This is only called to create the first 
        /// segment metadata spanning the entire time range.
        /// </summary>
        public void CreateDefaultSegment()
        {
            _directory.CreateDefaultSegment();
        }

        public void AllocateFullPassStacks(ISubGridCellPassesDataSegmentInfo segmentInfo)
        {
            if (segmentInfo.Segment == null)
            {
                AllocateSegment(segmentInfo);
            }

            segmentInfo.Segment?.AllocateFullPassStacks();
        }

        public void AllocateLatestPassGrid(ISubGridCellPassesDataSegmentInfo segmentInfo)
        {
            if (segmentInfo.Segment == null)
            {
                AllocateSegment(segmentInfo);
            }

            segmentInfo.Segment?.AllocateLatestPassGrid();
        }

        public bool HasAllCellPasses() => _cells != null;

        public void AllocateLeafFullPassStacks()
        {
          _cells ??= new SubGridCellPassesDataWrapper
          {
            Owner = this
          };
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
                // Include(FLeafStorageClasses, ...LatestData);
                _directory.AllocateGlobalLatestCells();
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
        private void GetAppropriateLatestValueFor(Cell_NonStatic cellPasses,
                                                  ref CellPass latestData,
                                                  int lastPassIndex,
                                                  GridDataType typeToCheck,
                                                  out bool valueFromLatestCellPass)
        {

            valueFromLatestCellPass = false;

            lastPassIndex += cellPasses.Passes.Offset;
            for (int I = lastPassIndex, limit = cellPasses.Passes.Offset; I >= limit; I--)
            {
                var pass = cellPasses.Passes.Elements[I];

                switch (typeToCheck)
                {
                    case GridDataType.CCV:
                        if (pass.CCV != CellPassConsts.NullCCV)
                        {
                            latestData.CCV = pass.CCV;
                            valueFromLatestCellPass = I == lastPassIndex;
                        }
                        break;

                    case GridDataType.RMV:
                        if (pass.RMV != CellPassConsts.NullRMV)
                        {
                            latestData.RMV = pass.RMV;
                            valueFromLatestCellPass = I == lastPassIndex;
                        }
                        break;

                    case GridDataType.Frequency:
                        if (pass.Frequency != CellPassConsts.NullFrequency)
                        {
                            latestData.Frequency = pass.Frequency;
                            valueFromLatestCellPass = I == lastPassIndex;
                        }
                        break;

                    case GridDataType.Amplitude:
                        if (pass.Amplitude != CellPassConsts.NullAmplitude)
                        {
                            latestData.Amplitude = pass.Amplitude;
                            valueFromLatestCellPass = I == lastPassIndex;
                        }
                        break;

                    case GridDataType.GPSMode:
                        {
                            // Also grab flags for half pass and rear axle
                            latestData.HalfPass = pass.HalfPass;
                            latestData.PassType = pass.PassType;

                            if (pass.gpsMode != CellPassConsts.NullGPSMode)
                            {
                                latestData.gpsMode = pass.gpsMode;
                                valueFromLatestCellPass = I == lastPassIndex;
                            }
                        }
                        break;

                    case GridDataType.Temperature:
                        if (pass.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue)
                        {
                            latestData.MaterialTemperature = pass.MaterialTemperature;
                            valueFromLatestCellPass = I == lastPassIndex;
                        }
                        break;

                    case GridDataType.MDP:
                        if (pass.MDP != CellPassConsts.NullMDP)
                        {
                            latestData.MDP = pass.MDP;
                            valueFromLatestCellPass = I == lastPassIndex;
                        }
                        break;

                    case GridDataType.CCA:
                        if (pass.CCA != CellPassConsts.NullCCA)
                        {
                            latestData.CCA = pass.CCA;
                            valueFromLatestCellPass = I == lastPassIndex;
                        }
                        break;
                }
            }
        }

        private void CalculateLatestPassDataForPassStack(Cell_NonStatic cellPasses,
                                                         ref CellPass latestData,
                                                         out bool ccvFromLatestCellPass,
                                                         out bool rmvFromLatestCellPass,
                                                         out bool frequencyFromLatestCellPass,
                                                         out bool amplitudeFromLatestCellPass,
                                                         out bool temperatureFromLatestCellPass,
                                                         out bool gpsModeFromLatestCellPass,
                                                         out bool mdpFromLatestCellPass,
                                                         out bool ccaFromLatestCellPass)
        {
            var lastPassIndex = cellPasses.PassCount - 1;

            if (lastPassIndex >= 0)
            {
              var lastCellPass = cellPasses.Passes.GetElement(lastPassIndex);

              latestData.Time = lastCellPass.Time;
              latestData.InternalSiteModelMachineIndex = lastCellPass.InternalSiteModelMachineIndex;

              // ReSharper disable once CompareOfFloatsByEqualityOperator
              if (lastCellPass.Height != Consts.NullHeight)
                latestData.Height = lastCellPass.Height;

              if (lastCellPass.RadioLatency != CellPassConsts.NullRadioLatency)
                latestData.RadioLatency = lastCellPass.RadioLatency;

              latestData.MachineSpeed = lastCellPass.MachineSpeed;
            }

            GetAppropriateLatestValueFor(cellPasses, ref latestData, lastPassIndex, GridDataType.GPSMode, out gpsModeFromLatestCellPass);
            GetAppropriateLatestValueFor(cellPasses, ref latestData, lastPassIndex, GridDataType.CCV, out ccvFromLatestCellPass);
            GetAppropriateLatestValueFor(cellPasses, ref latestData, lastPassIndex, GridDataType.RMV, out rmvFromLatestCellPass);
            GetAppropriateLatestValueFor(cellPasses, ref latestData, lastPassIndex, GridDataType.Frequency, out frequencyFromLatestCellPass);
            GetAppropriateLatestValueFor(cellPasses, ref latestData, lastPassIndex, GridDataType.Amplitude, out amplitudeFromLatestCellPass);
            GetAppropriateLatestValueFor(cellPasses, ref latestData, lastPassIndex, GridDataType.Temperature, out temperatureFromLatestCellPass);
            GetAppropriateLatestValueFor(cellPasses, ref latestData, lastPassIndex, GridDataType.MDP, out mdpFromLatestCellPass);
            GetAppropriateLatestValueFor(cellPasses, ref latestData, lastPassIndex, GridDataType.CCA, out ccaFromLatestCellPass);
        }

        public void AllocateSegment(ISubGridCellPassesDataSegmentInfo segmentInfo)
        {
            if (segmentInfo.Segment != null)
              throw new TRexSubGridTreeException("Cannot allocate a segment that is already allocated");

            _cells.PassesData.AddNewSegment(this, segmentInfo);
        }

        public override bool CellHasValue(byte cellX, byte cellY)
        {
            return _directory.GlobalLatestCells.PassDataExistenceMap.BitSet(cellX, cellY);
        }

        private void CalculateLatestPassGridForSegment(ISubGridCellPassesDataSegment segment,
                                                       ISubGridCellPassesDataSegment temporallyPrecedingSegment)
        {
            if (segment.PassesData == null)
              throw new TRexSubGridTreeException($"{nameof(CalculateLatestPassGridForSegment)} passed a segment in {Moniker()} with no cell passes allocated");

            if (_cells == null)
              throw new TRexSubGridTreeException($"Cell passes store for {Moniker()} not instantiated");

            segment.AllocateLatestPassGrid();

            var segmentPassesData = segment.PassesData;
            var segmentLatestPasses = segment.LatestPasses;
            segmentLatestPasses.Clear();
            segment.Dirty = true;

            var segmentLatestPassesPassDataExistenceMap = segmentLatestPasses.PassDataExistenceMap;
            var segmentLatestPassesCcvValuesAreFromLastPass = segmentLatestPasses.CCVValuesAreFromLastPass;
            var segmentLatestPassesRmvValuesAreFromLastPass = segmentLatestPasses.RMVValuesAreFromLastPass;
            var segmentLatestPassesFrequencyValuesAreFromLastPass = segmentLatestPasses.FrequencyValuesAreFromLastPass;
            var segmentLatestPassesAmplitudeValuesAreFromLastPass = segmentLatestPasses.AmplitudeValuesAreFromLastPass;
            var segmentLatestPassesGpsModeValuesAreFromLatestCellPass = segmentLatestPasses.GPSModeValuesAreFromLatestCellPass;
            var segmentLatestPassesTemperatureValuesAreFromLastPass = segmentLatestPasses.TemperatureValuesAreFromLastPass;
            var segmentLatestPassesMdpValuesAreFromLastPass = segmentLatestPasses.MDPValuesAreFromLastPass;
            var segmentLatestPassesCcaValuesAreFromLastPass = segmentLatestPasses.CCAValuesAreFromLastPass;

            var temporallyPrecedingSegmentLatestPasses = temporallyPrecedingSegment?.LatestPasses;

            // Seed the latest value tags for this segment with the latest data from the previous segment
            if (temporallyPrecedingSegmentLatestPasses != null)
            {
              segmentLatestPasses.AssignValuesFromLastPassFlags(temporallyPrecedingSegmentLatestPasses);
            }

            // Iterate over the values in the child leaf sub grid looking for
            // the first cell with passes in it
            SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                var updatedCell = false;

                if (temporallyPrecedingSegmentLatestPasses != null &&
                    temporallyPrecedingSegmentLatestPasses.PassDataExistenceMap.BitSet(i, j))
                {
                    // Seed the latest data for this segment with the latest data from the previous segment
                    segmentLatestPasses[i, j] = temporallyPrecedingSegmentLatestPasses[i, j];

                    updatedCell = true;
                }

                // Update the latest data from any previous segment with the information contained in this segment
                if (segmentPassesData.PassCount(i, j) > 0)
                {
                     var latestPass = ((SubGridCellLatestPassDataWrapper_NonStatic) segmentLatestPasses)[i, j];

                     CalculateLatestPassDataForPassStack(segmentPassesData.ExtractCellPasses(i, j),
                        ref latestPass,
                        out var ccvFromLatestCellPass,
                        out var rmvFromLatestCellPass,
                        out var frequencyFromLatestCellPass,
                        out var amplitudeFromLatestCellPass,
                        out var temperatureFromLatestCellPass,
                        out var gpsModeFromLatestCellPass,
                        out var mdpFromLatestCellPass,
                        out var ccaFromLatestCellPass);

                     ((SubGridCellLatestPassDataWrapper_NonStatic) segmentLatestPasses)[i, j] = latestPass;

                     segmentLatestPassesCcvValuesAreFromLastPass.SetBitValue(i, j, ccvFromLatestCellPass);
                     segmentLatestPassesRmvValuesAreFromLastPass.SetBitValue(i, j, rmvFromLatestCellPass);
                     segmentLatestPassesFrequencyValuesAreFromLastPass.SetBitValue(i, j, frequencyFromLatestCellPass);
                     segmentLatestPassesAmplitudeValuesAreFromLastPass.SetBitValue(i, j, amplitudeFromLatestCellPass);
                     segmentLatestPassesGpsModeValuesAreFromLatestCellPass.SetBitValue(i, j, gpsModeFromLatestCellPass);
                     segmentLatestPassesTemperatureValuesAreFromLastPass.SetBitValue(i, j, temperatureFromLatestCellPass);
                     segmentLatestPassesMdpValuesAreFromLastPass.SetBitValue(i, j, mdpFromLatestCellPass);
                     segmentLatestPassesCcaValuesAreFromLastPass.SetBitValue(i, j, ccaFromLatestCellPass);
              
                     updatedCell = true;
                }
                else
                {
                    if (temporallyPrecedingSegmentLatestPasses != null)
                    {
                        segmentLatestPassesCcvValuesAreFromLastPass.SetBitValue(i, j, temporallyPrecedingSegmentLatestPasses.CCVValuesAreFromLastPass.BitSet(i, j));
                        segmentLatestPassesRmvValuesAreFromLastPass.SetBitValue(i, j, temporallyPrecedingSegmentLatestPasses.RMVValuesAreFromLastPass.BitSet(i, j));
                        segmentLatestPassesFrequencyValuesAreFromLastPass.SetBitValue(i, j, temporallyPrecedingSegmentLatestPasses.FrequencyValuesAreFromLastPass.BitSet(i, j));
                        segmentLatestPassesAmplitudeValuesAreFromLastPass.SetBitValue(i, j, temporallyPrecedingSegmentLatestPasses.AmplitudeValuesAreFromLastPass.BitSet(i, j));
                        segmentLatestPassesGpsModeValuesAreFromLatestCellPass.SetBitValue(i, j, temporallyPrecedingSegmentLatestPasses.GPSModeValuesAreFromLatestCellPass.BitSet(i, j));
                        segmentLatestPassesTemperatureValuesAreFromLastPass.SetBitValue(i, j, temporallyPrecedingSegmentLatestPasses.TemperatureValuesAreFromLastPass.BitSet(i, j));
                        segmentLatestPassesMdpValuesAreFromLastPass.SetBitValue(i, j, temporallyPrecedingSegmentLatestPasses.MDPValuesAreFromLastPass.BitSet(i, j));
                        segmentLatestPassesCcaValuesAreFromLastPass.SetBitValue(i, j, temporallyPrecedingSegmentLatestPasses.CCAValuesAreFromLastPass.BitSet(i, j));
                    }
                }

                if (updatedCell)
                  segmentLatestPassesPassDataExistenceMap.SetBit(i, j);
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
            var segment = _directory.SegmentDirectory.Last().Segment;

            var globalLatestCells = _directory.GlobalLatestCells;
            var latestPasses = segment.LatestPasses;

            if (latestPasses == null) 
              return;

            globalLatestCells.Clear();
            globalLatestCells.Assign(latestPasses);

            var globalLatestCellsCast = (SubGridCellLatestPassDataWrapper_NonStatic) globalLatestCells;
            var latestPassesCast = (SubGridCellLatestPassDataWrapper_NonStatic) latestPasses;

            segment.LatestPasses.PassDataExistenceMap.ForEachSetBit((x, y) => globalLatestCellsCast[x, y] = latestPassesCast[x, y]);
        }

        public void ComputeLatestPassInformation(bool fullRecompute, IStorageProxy storageProxyForSubGridSegments)
        {
            //Log.LogInformation($"ComputeLatestPassInformation: Segment dir for {Moniker()}:");
            //Directory.DumpSegmentDirectoryToLog();

            if (!Dirty)
            {
              throw new TRexSubGridTreeException($"Sub grid {Moniker()} not marked as dirty when computing latest pass information");
            }

            var iterator = new SubGridSegmentIterator(this, _directory, storageProxyForSubGridSegments)
            {
                IterationDirection = IterationDirection.Forwards,
                ReturnDirtyOnly = !fullRecompute
            };
            var numProcessedSegments = 0;

            // We are in the process of recalculating latest data, so don't ask the iterator to
            // read the latest data information as it will be reconstructed here. The full cell pass
            // stacks are required though...
            iterator.RetrieveLatestData = false;
            iterator.RetrieveAllPasses = true;

            ISubGridCellPassesDataSegmentInfo seedSegmentInfo = null;
            ISubGridCellPassesDataSegment lastSegment = null;

            // Locate the segment immediately previous to the first dirty segment in the list of segments

            var segmentDirectory = _directory.SegmentDirectory;
            for (int I = 0, limit = segmentDirectory.Count; I < limit; I++)
            {
                if (segmentDirectory[I].Segment != null && segmentDirectory[I].Segment.Dirty)
                {
                    if (I > 0)
                    {
                        seedSegmentInfo = segmentDirectory[I - 1];
                    }
                    break;
                }
            }

            // If we chose the first segment and it was dirty, then clear it
            if (seedSegmentInfo?.Segment != null)
            {
                lastSegment = seedSegmentInfo.Segment;
            }

            // If there was such a last segment, then make sure its latest pass information
            // has been read from the store

            if (seedSegmentInfo != null && seedSegmentInfo.ExistsInPersistentStore &&
               (seedSegmentInfo.Segment == null || !seedSegmentInfo.Segment.HasLatestData))
            {
                if (seedSegmentInfo.Segment == null)
                {
                    AllocateSegment(seedSegmentInfo);
                }

                if (seedSegmentInfo.Segment != null)
                {
                    if (((ServerSubGridTree)Owner).LoadLeafSubGridSegment(storageProxyForSubGridSegments, new SubGridCellAddress(OriginX, OriginY), true, false,
                                                                          this, seedSegmentInfo.Segment) == FileSystemErrorStatus.OK)
                    {
                        lastSegment = seedSegmentInfo.Segment;
                    }
                    else
                    {
                        _log.LogCritical($"Failed to load segment from sub grid where segment was marked as present in persistent store for {new SubGridCellAddress(OriginX, OriginY)}");
                    }
                }
            }

            // The first MoveNext will locate the first segment in the directory marked as dirty
            while (iterator.MoveNext())
            {
                if (iterator.CurrentSubGridSegment != null && !iterator.CurrentSubGridSegment.Dirty && iterator.ReturnDirtyOnly)
                {
                    throw new TRexException($"Iterator returned non-dirty segment in ComputeLatestPassInformation for {Moniker()}");
                }

                numProcessedSegments++;

                CalculateLatestPassGridForSegment(iterator.CurrentSubGridSegment, lastSegment);

                lastSegment = iterator.CurrentSubGridSegment;

                // A segment has been processed... By definition, all segments after the first segment must have the
                // latest values processed, so instruct the iterator to return all segments from now on
                iterator.ReturnDirtyOnly = false;
            }

            // Note: It is possible that there were no processed segments (NumProcessedSegments = 0) as a result of processing
            // a TAG file that caused no changes to the database (e.g. it had been processed earlier)
            if (numProcessedSegments > 0)
            {
                // Now compute the final global latest pass data for the directory (though this will be the
                // same as the last segment)
                CalculateLatestPassGridForAllSegments();
            }

            //Log.LogInformation($"Completed ComputeLatestPassInformation for {Moniker()}");
        }

        public FileSystemErrorStatus LoadSegmentFromStorage(IStorageProxy storageProxy, string fileName, ISubGridCellPassesDataSegment segment, bool loadLatestData, bool loadAllPasses)
        {
             if (loadAllPasses && segment.Dirty)
             {
                 _log.LogCritical("Leaf sub grid segment loads of cell pass data may not be performed while the segment is dirty. The information should be taken from the cache instead");
                 return FileSystemErrorStatus.ElementToBeReadIsDirty;
             }

             var fsError = storageProxy.ReadSpatialStreamFromPersistentStore
              (Owner.ID, fileName, OriginX, OriginY,
               segment.SegmentInfo.StartTime.Ticks,
               segment.SegmentInfo.EndTime.Ticks,
               segment.SegmentInfo.Version,
               FileSystemStreamType.SubGridSegment, out var sms);

             try
             {
               if (fsError != FileSystemErrorStatus.OK)
               {
                 _log.LogError(fsError == FileSystemErrorStatus.FileDoesNotExist
                   ? $"Expected leaf sub grid segment {fileName}, model {Owner.ID} does not exist."
                   : $"Unable to load leaf sub grid segment {fileName}, model {Owner.ID}. Details: {fsError}");
               }
               else
               {
                 sms.Position = 0;
                 using var reader = new BinaryReader(sms, Encoding.UTF8, true);
                 if (!segment.Read(reader, loadLatestData, loadAllPasses))
                 {
                    fsError = FileSystemErrorStatus.DeserializationError;
                 }

                 if (loadAllPasses && segment.PassesData == null)
                   _log.LogError($"Segment {fileName} passes data is null after reading from store with LoadAllPasses=true.");
               }
             }
             finally
             {
                sms?.Dispose();
             }

             return fsError;
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
            _log.LogError(fsError == FileSystemErrorStatus.FileDoesNotExist
              ? $"Expected leaf sub grid segment {fileName}, model {Owner.ID} does not exist for removal."
              : $"Unable to remove leaf sub grid segment {fileName}, model {Owner.ID}. Details: {fsError}");
          }

          return result;
        }

        public bool SaveDirectoryToStream(Stream stream)
        {
          using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
          var header = new SubGridStreamHeader
          {
            Identifier = SubGridStreamHeader.kICServerSubGridDirectoryFileMoniker,
            Flags = SubGridStreamHeader.kSubGridHeaderFlag_IsSubGridDirectoryFile,
            StartTime = _leafStartTime,
            EndTime = _leafEndTime,
            LastUpdateTimeUTC = DateTime.UtcNow
          };

          // Write the header/version to the stream
          header.Write(writer);

          _directory.Write(writer);

          return true;
        }

        public bool SaveDirectoryToFile(IStorageProxy storage,
                                        string fileName)
        {
          using var stream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();
          SaveDirectoryToStream(stream);

          var result = storage.WriteSpatialStreamToPersistentStore
          (Owner.ID, fileName, OriginX, OriginY, -1, -1, _version,
            FileSystemStreamType.SubGridDirectory, stream, this) == FileSystemErrorStatus.OK;

          if (!result)
            _log.LogWarning($"Call to WriteSpatialStreamToPersistentStore failed. Filename:{fileName}");

          return result;
        }

      /// <summary>
      /// Generates the affinity key for this sub grid that identifies this element in the persistent data store
      /// </summary>
      public ISubGridSpatialAffinityKey AffinityKey() => new SubGridSpatialAffinityKey(_version, Owner.ID, OriginX, OriginY);

      public bool LoadDirectoryFromStream(Stream stream)
        {
          using var reader = new BinaryReader(stream, Encoding.UTF8, true);
          var header = new SubGridStreamHeader(reader);

          // long LatestCellPassDataSize;
          // long CellPassStacksDataSize;

          if (!header.IdentifierMatches(SubGridStreamHeader.kICServerSubGridDirectoryFileMoniker))
          {
            _log.LogError(
              $"Sub grid directory file header mismatch (expected [Header: {SubGridStreamHeader.kICServerSubGridDirectoryFileMoniker}, found {header.Identifier}]).");
            return false;
          }

          if (!header.IsSubGridDirectoryFile)
          {
            throw new TRexSubGridIOException(
              "Sub grid directory file does not identify itself as such in extended header flags");
          }

          if (header.Version != 1 && header.Version != 2)
          {
            _log.LogError($"Sub grid directory file version or header mismatch (expected [Version in: [1..{SubGridStreamHeader.VERSION}], found {header.Version}]");
            return false;
          }

          //  FLastUpdateTimeUTC := Header.LastUpdateTimeUTC;
          _leafStartTime = header.StartTime;
          _leafEndTime = header.EndTime;

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

          if (header.Version == 1)
            _directory.ReadUnversioned(reader);
          else
            _directory.Read(reader);

          return true;
        }

        public bool LoadDirectoryFromFile(IStorageProxy storage, string fileName)
        {
            if (_version == 0)
              _log.LogError($"Version for {Moniker()} is 0");

            var fsError = storage.ReadSpatialStreamFromPersistentStore(Owner.ID, fileName, OriginX, OriginY, -1, -1, _version,
                                                                       FileSystemStreamType.SubGridDirectory, out var sms);
            try
            {
              if (fsError != FileSystemErrorStatus.OK || sms == null)
              {
                if (fsError == FileSystemErrorStatus.FileDoesNotExist)
                  _log.LogError($"Expected leaf sub grid file {fileName} does not exist.");
                else if (fsError != FileSystemErrorStatus.SpatialStreamIndexGranuleLocationNull &&
                         fsError != FileSystemErrorStatus.GranuleDoesNotExist)
                  _log.LogWarning($"Unable to load leaf sub grid file '{fileName}'. Details: {fsError}");

                return false;
              }

              sms.Position = 0;
              return LoadDirectoryFromStream(sms);
            }
            finally
            {
              sms?.Dispose();
            }
        }

        /// <summary>
        /// Deletes the directory file from the persistent store
        /// </summary>
        public bool RemoveDirectoryFromStorage(IStorageProxy storage, string fileName)
        {
            var fsError = storage.RemoveSpatialStreamFromPersistentStore(Owner.ID, fileName, OriginX, OriginY, -1, -1, _version,
            FileSystemStreamType.SubGridDirectory);

            if (fsError != FileSystemErrorStatus.OK)
            {
                if (fsError == FileSystemErrorStatus.FileDoesNotExist)
                {
                    _log.LogError($"Expected leaf sub grid file {fileName} does not exist.");
                }
                else if (fsError != FileSystemErrorStatus.SpatialStreamIndexGranuleLocationNull &&
                         fsError != FileSystemErrorStatus.GranuleDoesNotExist)
                {
                  _log.LogWarning($"Unable to load leaf sub grid file '{fileName}'. Details: {fsError}");
                }

                return false;
            }

            return true;
        }

        public void Integrate(IServerLeafSubGrid source,
                              ISubGridSegmentIterator iterator,
                              bool integratingIntoIntermediaryGrid)
        {
            // _log.LogInformation($"Integrating sub grid {Moniker()}, intermediary?:{integratingIntoIntermediaryGrid}");

            if (source == null)
            {
              throw new TRexSubGridTreeException("Source sub grid not defined in ServerSubGridTreeLeaf.Integrate");
            }

            if (source.Cells.PassesData.Count == 0)
            {
                // No cells added to this sub grid during processing
                _log.LogCritical($"Empty sub grid {Moniker()} passed to Integrate");
                return;
            }

            if (source.Cells.PassesData.Count != 1)
            {
                _log.LogCritical($"source integrated sub grids must have only one segment in Integrate ({Moniker()})");
                return;
            }

            iterator.SubGrid = this;
            iterator.Directory = _directory;

            var sourceSegment = source.Cells.PassesData[0];

            UpdateStartEndTimeRange(source.LeafStartTime);
            UpdateStartEndTimeRange(source.LeafEndTime);

            for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
                for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                    var sourceSegmentCell = sourceSegment.PassesData.ExtractCellPasses(i, j);
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

                        segment.PassesData.Integrate(i, j, sourceSegmentCell, 
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

            //_log.LogInformation($"Completed integrating sub grid {Moniker()}, intermediary?:{integratingIntoIntermediaryGrid}, {addedCount} cell passes added, {modifiedCount} modified");
            //_log.LogInformation($"Completed integrating sub grid {Moniker()}, intermediary?:{integratingIntoIntermediaryGrid}");
        }

    /// <summary>
    /// Constructs a 'filename' representing this leaf sub grid
    /// </summary>
    public static string FileNameFromOriginPosition(SubGridCellAddress origin) => $"{origin.X:D10}-{origin.Y:D10}";

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (_disposedValue)
        return;

      // Treat disposal and finalization as the same, dependent on the primary disposedValue flag
      if (Cells?.PassesData != null)
      {
        for (int i = 0, limit = Cells.PassesData.Count; i < limit; i++)
        {
          Cells.PassesData[i].Dispose();
        }
      }

      Directory?.Dispose();
      Directory = null;

      _disposedValue = true;
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}

