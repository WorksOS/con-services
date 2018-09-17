using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellPassesDataSegment : ISubGridCellPassesDataSegment
  {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Tracks whether there are unsaved changes in this segment
        /// </summary>
        public bool Dirty { get; set; }

        private DateTime _StartTime = DateTime.MinValue;
        private DateTime _EndTime = DateTime.MaxValue;

        public ISubGrid Owner { get; set; }

        public bool HasAllPasses { get; set; } 
        public bool HasLatestData { get; set; } 

        public ISubGridCellPassesDataSegmentInfo SegmentInfo { get; set; } 

        public ISubGridCellSegmentPassesDataWrapper PassesData { get; set; } 
       
        public ISubGridCellLatestPassDataWrapper LatestPasses { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridCellPassesDataSegment()
        {
        }

        public SubGridCellPassesDataSegment(ISubGridCellLatestPassDataWrapper latestPasses, ISubGridCellSegmentPassesDataWrapper passesData)
        {
            LatestPasses = latestPasses;
            HasLatestData = LatestPasses != null;

            PassesData = passesData;
            HasAllPasses = PassesData != null;
        }

        /// <summary>
        /// Determines if this segments tiume range bounds the data tiem givein in the time argument
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool SegmentMatches(DateTime time) => time >= SegmentInfo.StartTime && time < SegmentInfo.EndTime;

        public void AllocateFullPassStacks()
        {
            if (PassesData == null)
            {
                HasAllPasses = true;
                PassesData = SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper();
            }
        }
        public void AllocateLatestPassGrid()
        {
            if (LatestPasses == null)
            {
                HasLatestData = true;
                LatestPasses = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper();
            }
        }

        public void DeAllocateFullPassStacks()
        {
            HasAllPasses = false;
            PassesData = null;
        }

        public void DeAllocateLatestPassGrid()
        {
            HasLatestData = false;
            LatestPasses = null;
        }

        public bool SavePayloadToStream(BinaryWriter writer)
        {
            if (!(HasAllPasses && HasLatestData && PassesData != null && LatestPasses != null))
            {
                Debug.Assert(false,
                    "Leaf subgrids being written to persistent store must be fully populated with pass stacks and latest pass grid");
                //return false;
            }

            int CellStacksOffset = -1;

            // Write the cell pass information (latest and historic cell pass stacks)
            long CellStacksOffsetOffset = writer.BaseStream.Position;
            writer.Write(CellStacksOffset);

            Debug.Assert(HasAllPasses && HasLatestData && PassesData != null && LatestPasses != null,
                   "Leaf subgrids being written to persistent store must be fully populated with pass stacks and latest pass grid");

            LatestPasses.Write(writer, new byte[10000]);

            CellStacksOffset = (int)writer.BaseStream.Position;

            PassesData.Write(writer);

            int EndPosition = (int)writer.BaseStream.Position;

            // Write out the offset to the cell pass stacks in the file
            writer.BaseStream.Seek(CellStacksOffsetOffset, SeekOrigin.Begin);
            writer.Write(CellStacksOffset);

            writer.BaseStream.Seek(EndPosition, SeekOrigin.Begin);

            return true;
        }

        public bool LoadPayloadFromStream_v2p0(BinaryReader reader,
                                               bool loadLatestData,
                                               bool loadAllPasses)
        {
            // Read the stream offset where the cell pass stacks start
            int CellStacksOffset = reader.ReadInt32();

            if (HasLatestData && loadLatestData)
            {
                if (LatestPasses == null)
                {
                    Log.LogError("Cell latest pass store not instantiated in LoadPayloadFromStream_v2p0");
                    return false;
                }

                LatestPasses.Read(reader, new byte[10000]);
            }

            if (HasAllPasses && loadAllPasses)
            {
                reader.BaseStream.Seek(CellStacksOffset, SeekOrigin.Begin);

                PassesData.Read(reader);
            }

            return true;
        }

        public bool Read(BinaryReader reader, 
                         bool loadLatestData, bool loadAllPasses )
        {
            SubGridStreamHeader Header = new SubGridStreamHeader(reader);

            _StartTime = Header.StartTime;
            _EndTime = Header.EndTime;

            // Read the version etc from the stream
            if (!Header.IdentifierMatches(SubGridStreamHeader.kICServerSubgridLeafFileMoniker))
            {
                Log.LogError($"Subgrid segment file moniker (expected {SubGridStreamHeader.kICServerSubgridLeafFileMoniker}, found {Header.Identifier}). Stream size/position = {reader.BaseStream.Length}{reader.BaseStream.Position}");
                return false;
            }

            if (!Header.IsSubGridSegmentFile)
            {
                Log.LogCritical("Subgrid grid segment file does not identify itself as such in extended header flags");
                return false;
            }

            bool Result = false;

            if (Header.MajorVersion == 2)
            {
                switch (Header.MinorVersion)
                {
                    case 0:
                        Result = LoadPayloadFromStream_v2p0(reader, loadLatestData, loadAllPasses);
                        break;

                    default:
                        Log.LogError($"Subgrid segment file version mismatch (expected {SubGridStreamHeader.kSubGridMajorVersion}.{SubGridStreamHeader.kSubGridMinorVersion_Latest}, found {Header.MajorVersion}.{Header.MinorVersion}). Stream size/position = {reader.BaseStream.Length}{reader.BaseStream.Position}");
                        break;
                }
            }
            else
            {
                Log.LogError($"Subgrid segment file version mismatch (expected {SubGridStreamHeader.kSubGridMajorVersion}.{SubGridStreamHeader.kSubGridMinorVersion_Latest}, found {Header.MajorVersion}.{Header.MinorVersion}). Stream size/position = {reader.BaseStream.Length}{reader.BaseStream.Position}");
            }

            return Result;
        }

        public bool Write(BinaryWriter writer)
        {
            // Write the version to the stream
            SubGridStreamHeader Header = new SubGridStreamHeader()
            {
                MajorVersion = SubGridStreamHeader.kSubGridMajorVersion,
                MinorVersion = SubGridStreamHeader.kSubGridMinorVersion_Latest,
                Identifier = SubGridStreamHeader.kICServerSubgridLeafFileMoniker,
                Flags = SubGridStreamHeader.kSubGridHeaderFlag_IsSubgridSegmentFile,
                StartTime = SegmentInfo?.StartTime ?? _StartTime,
                EndTime = SegmentInfo?.EndTime ?? _EndTime,
                LastUpdateTimeUTC = DateTime.Now - Time.GPS.GetLocalGMTOffset()
            };

            Header.Write(writer);

            SavePayloadToStream(writer);

            return true;
        }

        public void CalculateElevationRangeOfPasses()
        {
            if (SegmentElevationRangeCalculator.CalculateElevationRangeOfPasses(PassesData, out double Min, out double Max))
            {
                SegmentInfo.MinElevation = Min;
                SegmentInfo.MaxElevation = Max;
            }
        }

        public bool SaveToFile(IStorageProxy storage,
                               string FileName,
                               out FileSystemErrorStatus FSError)
        {
            // TODO  InvalidatedSpatialStreams: TInvalidatedSpatialStreamArray;

            bool Result;
            FSError = FileSystemErrorStatus.OK;

            CalculateElevationRangeOfPasses();

            if (TRexConfig.RecordSegmentCleavingOperationsToLog || TRexConfig.RecordItemsPersistedViaDataPersistorToLog)
            {
              SegmentTotalPassesCalculator.CalculateTotalPasses(PassesData, out uint TotalPasses, out uint MaxPasses);
      
              if (TRexConfig.RecordSegmentCleavingOperationsToLog && TotalPasses > TRexConfig.VLPD_SubGridSegmentPassCountLimit)
                  Log.LogDebug($"Saving segment {FileName} with {TotalPasses} cell passes (max:{MaxPasses}) which violates the maximum number of cell passes within a segment ({TRexConfig.VLPD_SubGridSegmentPassCountLimit})");
      
              if (TRexConfig.RecordItemsPersistedViaDataPersistorToLog)
                Log.LogDebug($"Saving segment {FileName} with {TotalPasses} cell passes (max:{MaxPasses})");
            }
      
            using (MemoryStream MStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(MStream, Encoding.UTF8, true))
                {
                    if (!Write(writer))
                        return false;
                }

                // TODO          SetLength(InvalidatedSpatialStreams, 0);
                FSError = storage.WriteSpatialStreamToPersistentStore(
                    Owner.Owner.ID,
                    FileName,
                    Owner.OriginX, Owner.OriginY,
                    FileName,
                    //  TODO         InvalidatedSpatialStreams,
                    FileSystemStreamType.SubGridSegment,
                    MStream);

                Result = FSError == FileSystemErrorStatus.OK;
            }

            return Result;
        }

        /// <summary>
        /// Determines if this segment violates either the maximum number of cell passes within a 
        /// segment limit, or the maximum numebr of cell passes within a single cell within a
        /// segment limit.
        /// If either limit is breached, this segment requires cleaving
        /// </summary>
        /// <returns></returns>
        public bool RequiresCleaving()
        {
            SegmentTotalPassesCalculator.CalculateTotalPasses(PassesData, out uint TotalPasses, out uint MaxPassCount);

            return TotalPasses > TRexConfig.VLPD_SubGridSegmentPassCountLimit ||
                   MaxPassCount > TRexConfig.VLPD_SubGridMaxSegmentCellPassesLimit;
        }

        /// <summary>
        /// Verifies if the segment time range bounds are consistent with the cell passes it contains
        /// </summary>
        /// <returns></returns>
        public bool VerifyComputedAndRecordedSegmentTimeRangeBounds()
        {
          // Determine the actual time range of the passes within the segment
          SegmentTimeRangeCalculator.CalculateTimeRange(PassesData, out DateTime CoveredTimeRangeStart, out DateTime CoveredTimeRangeEnd);

          bool Result = CoveredTimeRangeStart >= SegmentInfo.StartTime && CoveredTimeRangeEnd <= SegmentInfo.EndTime;

          if (!Result)
            Log.LogCritical($"Segment computed covered time is outside segment time range bounds (CoveredTimeRangeStart={CoveredTimeRangeStart}, CoveredTimeRangeEnd={CoveredTimeRangeEnd}, SegmentInfo.StartTime = {SegmentInfo.StartTime}, SegmentInfo.EndTime={SegmentInfo.EndTime}");

          return Result;
        }
    }
}
