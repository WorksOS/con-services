using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class SubGridCellPassesDataSegment
    {
        public bool Dirty { get; set; }

        private DateTime _StartTime = DateTime.MinValue;
        private DateTime _EndTime = DateTime.MaxValue;

        public ISubGrid Owner = null;

        public bool HasAllPasses { get; set; } 
        public bool HasLatestData { get; set; } 

        public SubGridCellPassesDataSegmentInfo SegmentInfo { get; set; } 

        public ISubGridCellSegmentPassesDataWrapper PassesData { get; set; } 
       
        public ISubGridCellLatestPassDataWrapper LatestPasses { get; set; }

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
            int EndPosition;
            bool Result = true;
            int CellStacksOffset = -1;

            // Write the cell pass information (latest and historic cell pass stacks)
            long CellStacksOffsetOffset = writer.BaseStream.Position;
            writer.Write(CellStacksOffset);

            Debug.Assert(HasAllPasses && HasLatestData && (PassesData != null) && (LatestPasses != null),
                   "Leaf subgrids being written to persistent store must be fully populated with pass stacks and latest pass grid");

            LatestPasses.Write(writer, new byte[10000]);

            CellStacksOffset = (int)writer.BaseStream.Position;

            PassesData.Write(writer);

            EndPosition = (int)writer.BaseStream.Position;

            // Write out the offset to the cell pass stacks in the file
            writer.BaseStream.Seek(CellStacksOffsetOffset, SeekOrigin.Begin);
            writer.Write(CellStacksOffset);

            writer.BaseStream.Seek(EndPosition, SeekOrigin.Begin);

            return Result;
        }

        public bool LoadPayloadFromStream_v2p0(BinaryReader reader,
                                               bool loadLatestData,
                                               bool loadAllPasses,
                                               SubGridCellPassCountRecord[,] CellPassCounts,
                                               out long LatestCellPassDataSize,
                                               out long CellPassStacksDataSize)
        {
            LatestCellPassDataSize = 0;
            CellPassStacksDataSize = 0;

            // Read the stream offset where the cell pass stacks start
            int CellStacksOffset = reader.ReadInt32();

            if (HasLatestData && loadLatestData)
            {
                if (LatestPasses == null)
                {
                    // TODO readd when logging available
                    // SIGLogMessage.PublishNoODS(Self, 'Cell latest pass store not instantiated in LoadPayloadFromStream_v2p0', slmcAssert);
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

        public bool Read(BinaryReader reader, //Stream stream,
                         bool loadLatestData, bool loadAllPasses /*
                         SiteModel SiteModelReference*/)
        {
            SubGridStreamHeader Header = new SubGridStreamHeader(reader);

            _StartTime = Header.StartTime;
            _EndTime = Header.EndTime;

            SubGridCellPassCountRecord[,] CellPassCounts = new SubGridCellPassCountRecord[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
            CellPass[,] LatestPassData = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            // Read the version etc from the stream
            if (!Header.IdentifierMatches(SubGridStreamHeader.kICServerSubgridLeafFileMoniker))
            {
                /* TODO readd when logging available
                SIGLogMessage.Publish(Self,
                                      'Subgrid segment file moniker (expected %1, found %2). Stream size/position = %3/%4', { SKIP}
                                      [String(kICServerSubgridLeafFileMoniker), String(Identitifer),
                                      InttoStr(Stream.Size), InttoStr(Stream.Position)],
                                      slmcError);
                */
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
                        Result = LoadPayloadFromStream_v2p0(reader, loadLatestData, loadAllPasses, CellPassCounts, // LatestPasses.PassData,
                                               out long LatestCellPassDataSize, out long CellPassStacksDataSize);
                        break;

                    default:
                        /* Todo: Readd when logging available
                        SIGLogMessage.Publish(Self,
                                              'Subgrid segment file version mismatch (expected %1.%2, found %3.%4). Stream size/position = %5/%6', { SKIP}
                          [IntToStr(kSubGridMajorVersion), IntToStr(kSubGridMinorVersion_Latest),
                           IntToStr(MajorVersion), IntToStr(MinorVersion),
                           InttoStr(Stream.Size), InttoStr(Stream.Position)],
                          slmcError);*/
                        break;
                }
            }
            else
            {
                /* Todo: Readd when logging available  
                     SIGLogMessage.Publish(Self,
                        'Subgrid segment file version mismatch (expected %1.%2, found %3.%4). Stream size/position = %5/%6', {SKIP}
                        [IntToStr(kSubGridMajorVersion), IntToStr(kSubGridMinorVersion_Latest),
                         IntToStr(MajorVersion), IntToStr(MinorVersion),
                         InttoStr(Stream.Size), InttoStr(Stream.Position)],
                        slmcError);*/
            }

            /*  {$IFDEF STATIC_CELL_PASSES}
              if Result then
                begin
                  if Assigned(PassesData) then
                    Result := Result and PassesData.PerformEncodingForInternalCache(CellPassCounts, LatestCellPassDataSize, CellPassStacksDataSize, SiteModelReference);

                  if Assigned(LatestPasses) then
                    Result := Result and  LatestPasses.PerformEncodingForInternalCache(LatestPassData, LatestCellPassDataSize, CellPassStacksDataSize);
                end;
              {$ENDIF}*/

            return Result;
        }

        public bool Write(BinaryWriter writer /*Stream stream*/)
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
            double Min = 1E10;
            double Max = -1E10;

            for (uint i = 0; i < SubGridTree.SubGridTreeDimension; i++)
            {
                for (uint j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                {
                    uint _PassCount = PassesData.PassCount(i, j);

                    if (_PassCount > 0)
                    {
                        for (uint PassIndex = 0; PassIndex < _PassCount; PassIndex++)
                        {
                            float _height = PassesData.PassHeight(i, j, PassIndex);

                            if (_height > Max)
                            {
                                Max = _height;
                            }
                            if (_height < Min)
                            {
                                Min = _height;
                            }
                        }
                    }
                }
            }

            if (Min < Max)
            {
                SegmentInfo.MinElevation = Min;
                SegmentInfo.MaxElevation = Max;
            }
        }

        public bool SaveToFile(IStorageProxy storage,
                               string FileName,
                               out FileSystemErrorStatus FSError)
        {
            //            int TotalPasses = 0, MaxPasses = 0;
            //            InvalidatedSpatialStreams: TInvalidatedSpatialStreamArray;

            bool Result = false;
            FSError = FileSystemErrorStatus.OK;

            CalculateElevationRangeOfPasses();

            /*
             *   if (RecordSegmentCleavingOperationsToLog)
               {
                   CalculateTotalPasses(ref TotalPasses, ref MaxPasses);

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

            MemoryStream MStream = new MemoryStream();
            using (var writer = new BinaryWriter(MStream, Encoding.UTF8, true))
            {
                if (!Write(writer))
                {
                    return false;
                }
            }

            //            SetLength(InvalidatedSpatialStreams, 0);
            FSError = storage.WriteSpatialStreamToPersistentStore(
           Owner.Owner.ID,
           FileName,
           Owner.OriginX, Owner.OriginY,
           FileName,
           //           InvalidatedSpatialStreams,
           FileSystemStreamType.SubGridSegment,
           out uint StoreGranuleIndex,
           out uint StoreGranuleCount,
           MStream);

            Result = FSError == FileSystemErrorStatus.OK;

            if (Result)
            {
                // Assign the store granule index and count into the segment for later reference
                SegmentInfo.FSGranuleIndex = StoreGranuleIndex;
                SegmentInfo.FSGranuleCount = StoreGranuleCount;
            }

            return Result;
        }
    }
}
