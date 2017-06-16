using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class SubGridCellPassesDataSegment
    {
        public bool Dirty { get; set; } = false;

        public ISubGrid Owner = null;

        public bool HasAllPasses { get; set; } = false;
        public bool HasLatestData { get; set; } = false;

        public SubGridCellPassesDataSegmentInfo SegmentInfo { get; set; } = null;

//        public SubGridCellSegmentPassesDataWrapper PassesData { get; set; } = null;
//        public SubGridCellSegmentPassesDataWrapper_NonStatic PassesData { get; set; } = null;
        public ISubGridCellSegmentPassesDataWrapper PassesData { get; set; } = null;
       
        public SubGridCellLatestPassDataWrapper_NonStatic LatestPasses { get; set; } = null;

        public SubGridCellPassesDataSegment()
        {
        }

        public bool SegmentMatches(DateTime time) => (time >= SegmentInfo.StartTime) && (time < SegmentInfo.EndTime);

        public void AllocateFullPassStacks()
        {
            if (PassesData == null)
            {
                HasAllPasses = true;

                //                PassesData = new SubGridCellSegmentPassesDataWrapper();
                PassesData = new SubGridCellSegmentPassesDataWrapper_NonStatic();
            }
        }
        public void AllocateLatestPassGrid()
        {
            if (LatestPasses == null)
            {
                HasLatestData = true;
                LatestPasses = new SubGridCellLatestPassDataWrapper_NonStatic();
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

            // Write out the latest cell pass grid
            for (int i = 0; i < SubGridTree.SubGridTreeDimension; i++)
            {
                for (int j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                {
                    LatestPasses.PassData[i, j].Write(writer);
                }
            }

            LatestPasses.PassDataExistanceMap.Write(writer);
            LatestPasses.CCVValuesAreFromLastPass.Write(writer);
            LatestPasses.RMVValuesAreFromLastPass.Write(writer);
            LatestPasses.FrequencyValuesAreFromLastPass.Write(writer);
            LatestPasses.GPSModeValuesAreFromLatestCellPass.Write(writer);
            LatestPasses.AmplitudeValuesAreFromLastPass.Write(writer);
            LatestPasses.TemperatureValuesAreFromLastPass.Write(writer);
            LatestPasses.MDPValuesAreFromLastPass.Write(writer);
            LatestPasses.CCAValuesAreFromLastPass.Write(writer);

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
                                               ref long LatestCellPassDataSize,
                                               ref long CellPassStacksDataSize)
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

                for (int i = 0; i < SubGridTree.SubGridTreeDimension; i++)
                {
                    for (int j = 0; j < SubGridTree.SubGridTreeDimension; j++)
                    {
                        LatestPasses.PassData[i, j].Read(reader);
                    }
                }

                LatestPasses.PassDataExistanceMap.Read(reader);
                LatestPasses.CCVValuesAreFromLastPass.Read(reader);
                LatestPasses.RMVValuesAreFromLastPass.Read(reader);
                LatestPasses.FrequencyValuesAreFromLastPass.Read(reader);
                LatestPasses.GPSModeValuesAreFromLatestCellPass.Read(reader);
                LatestPasses.AmplitudeValuesAreFromLastPass.Read(reader);
                LatestPasses.TemperatureValuesAreFromLastPass.Read(reader);
                LatestPasses.MDPValuesAreFromLastPass.Read(reader);
                LatestPasses.CCAValuesAreFromLastPass.Read(reader);
            }

            if (HasAllPasses && loadAllPasses)
            {
                reader.BaseStream.Seek(CellStacksOffset, SeekOrigin.Begin);

                PassesData.Read(reader);
            }

            return true;
        }

        public bool LoadFromStream(Stream stream,
                                   bool loadLatestData, bool loadAllPasses,
                                   SiteModel SiteModelReference)
        {
            BinaryReader reader = new BinaryReader(stream);

            SubGridStreamHeader Header = new SubGridStreamHeader(reader);
            SubGridCellPassCountRecord[,] CellPassCounts = new SubGridCellPassCountRecord[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
            CellPass[,] LatestPassData = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            long LatestCellPassDataSize = 0;
            long CellPassStacksDataSize = 0;

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
                                               ref LatestCellPassDataSize, ref CellPassStacksDataSize);
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

        public bool SaveToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            // Write the version to the stream
            SubGridStreamHeader Header = new SubGridStreamHeader()
            {
                MajorVersion = SubGridStreamHeader.kSubGridMajorVersion,
                MinorVersion = SubGridStreamHeader.kSubGridMinorVersion_Latest,
                Identifier = SubGridStreamHeader.kICServerSubgridLeafFileMoniker,
                Flags = SubGridStreamHeader.kSubGridHeaderFlag_IsSubgridSegmentFile,
                StartTime = SegmentInfo.StartTime,
                EndTime = SegmentInfo.EndTime,
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
                               ref FileSystemErrorStatus FSError)
        {
            MemoryStream MStream = new MemoryStream();
            //            int TotalPasses = 0, MaxPasses = 0;
            uint StoreGranuleIndex = 0;
            uint StoreGranuleCount = 0;
            //            InvalidatedSpatialStreams: TInvalidatedSpatialStreamArray;

            bool Result = false;
            FSError = FileSystemErrorStatus.OK;

            CalculateElevationRangeOfPasses();

            /*
             *            if (RecordSegmentCleavingOperationsToLog)
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

            if (!SaveToStream(MStream))
            {
                return false;
            }

            //            SetLength(InvalidatedSpatialStreams, 0);
            FSError = storage.WriteSpatialStreamToPersistentStore(
           Owner.Owner.ID,
           FileName,
           Owner.OriginX, Owner.OriginY,
           //           InvalidatedSpatialStreams,
           FileSystemSpatialStreamType.SubGridSegment,
           out StoreGranuleIndex,
           out StoreGranuleCount,
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
