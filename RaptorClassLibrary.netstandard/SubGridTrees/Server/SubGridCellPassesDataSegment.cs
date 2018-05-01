using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Utilities;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class SubGridCellPassesDataSegment
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Tracks whether there are unsaved changes in this segment
        /// </summary>
        public bool Dirty { get; set; }

        private DateTime _StartTime = DateTime.MinValue;
        private DateTime _EndTime = DateTime.MaxValue;

        public ISubGrid Owner = null;

        public bool HasAllPasses { get; set; } 
        public bool HasLatestData { get; set; } 

        public SubGridCellPassesDataSegmentInfo SegmentInfo { get; set; } 

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
        public bool SegmentMatches(DateTime time) => (time >= SegmentInfo.StartTime) && (time < SegmentInfo.EndTime);

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
                LatestPasses = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(); // new SubGridCellLatestPassDataWrapper_NonStatic();
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
                return false;
            }

            //bool Result = true;
            int CellStacksOffset = -1;

            // Write the cell pass information (latest and historic cell pass stacks)
            long CellStacksOffsetOffset = writer.BaseStream.Position;
            writer.Write(CellStacksOffset);

            Debug.Assert(HasAllPasses && HasLatestData && (PassesData != null) && (LatestPasses != null),
                   "Leaf subgrids being written to persistent store must be fully populated with pass stacks and latest pass grid");

            LatestPasses.Write(writer, new byte[10000]);

            CellStacksOffset = (int)writer.BaseStream.Position;

            PassesData.Write(writer);

            int EndPosition = (int)writer.BaseStream.Position;

            // Write out the offset to the cell pass stacks in the file
            writer.BaseStream.Seek(CellStacksOffsetOffset, SeekOrigin.Begin);
            writer.Write(CellStacksOffset);

            writer.BaseStream.Seek(EndPosition, SeekOrigin.Begin);

            return true; // Result;
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
                    Log.Error("Cell latest pass store not instantiated in LoadPayloadFromStream_v2p0");
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
                Log.Error($"Subgrid segment file moniker (expected {SubGridStreamHeader.kICServerSubgridLeafFileMoniker}, found {Header.Identifier}). Stream size/position = {reader.BaseStream.Length}{reader.BaseStream.Position}");
                return false;
            }

            if (!Header.IsSubGridSegmentFile)
            {
                // TODO readd when logging available
                // SIGLogMessage.Publish(Self, 'Subgrid grid segment file does not identify itself as such in extended header flags', slmcAssert); { SKIP}
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
                        Log.Error($"Subgrid segment file version mismatch (expected {SubGridStreamHeader.kSubGridMajorVersion}.{SubGridStreamHeader.kSubGridMinorVersion_Latest}, found {Header.MajorVersion}.{Header.MinorVersion}). Stream size/position = {reader.BaseStream.Length}{reader.BaseStream.Position}");
                        break;
                }
            }
            else
            {
                Log.Error($"Subgrid segment file version mismatch (expected {SubGridStreamHeader.kSubGridMajorVersion}.{SubGridStreamHeader.kSubGridMinorVersion_Latest}, found {Header.MajorVersion}.{Header.MinorVersion}). Stream size/position = {reader.BaseStream.Length}{reader.BaseStream.Position}");
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
            //  TODO          InvalidatedSpatialStreams: TInvalidatedSpatialStreamArray;

            bool Result;
            FSError = FileSystemErrorStatus.OK;

            CalculateElevationRangeOfPasses();

            /* todo
             *   if (RecordSegmentCleavingOperationsToLog)
               {
                   CalculateTotalPasses(out int TotalPasses, out int MaxPasses);

                   // TODO ...
                   if (TotalPasses > VLPDSvcLocations.VLPD_SubGridSegmentPassCountLimit)
                   {
                       // TODO readd when logging available
                       //SIGLogMessage.PublishNoODS(this, "Saving segment {0} with {1} cell passes (max:{2}) which violates the maximum number of cell passes within a segment ({4})",        
                       //                           Filename, TotalPasses, MaxPasses, VLPDSvcLocations.VLPD_SubGridSegmentPassCountLimit], slmcDebug);
                   }
               }
            */

            /* TODO...
            if (VLPDSvcLocations.RecordItemsPersistedViaDataPersistorToLog)
            {
                SIGLogMessage.PublishNoODS(this, "Saving segment {0} with {1} cell passes (max:{2})',
                [Filename, IntToStr(TotalPasses), IntToStr(MaxPasses)], slmcDebug);
            }
            */

            using (MemoryStream MStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(MStream, Encoding.UTF8, true))
                {
                    if (!Write(writer))
                        return false;
                }

                //  TODO          SetLength(InvalidatedSpatialStreams, 0);
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

            return TotalPasses > RaptorConfig.VLPD_SubGridSegmentPassCountLimit ||
                   MaxPassCount > RaptorConfig.VLPD_SubGridMaxSegmentCellPassesLimit;
        }
    }
}
