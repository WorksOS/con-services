using System.Collections.Generic;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class SubGridCellPassesDataSegments
    {
        public List<SubGridCellPassesDataSegment> Items { get; set; } = new List<SubGridCellPassesDataSegment>();

        public void Clear()
        {
            Items?.Clear();
        }

        public int Count => Items?.Count ?? 0;

        public SubGridCellPassesDataSegment this[int index] { get { return Items[index]; } }

        public int Add(SubGridCellPassesDataSegment item)
        {
            //{$IFDEF DEBUG}
            //Counter: integer;
            //{$ENDIF}

            int Index = Count - 1;

            while ((Index >= 0) && (item.SegmentInfo.StartTime < Items[Index].SegmentInfo.StartTime))
            {
                Index--;
            }

            Index++;

            Items.Insert(Index, item);

            return Index;

            /*
             *  {$IFDEF DEBUG}
              Counter := 0;
              for Index := 0 to Count - 2 do
                if Items[Index].SegmentInfo.StartTime >= Items[Index + 1].SegmentInfo.StartTime then
                  begin
                    SIGLogMessage.PublishNoODS(Self, Format('Segment passes list out of order %.6f versus %.6f. Segment count = %d', { SKIP}
            [Items[Index].SegmentInfo.StartTime, Items[Index + 1].SegmentInfo.StartTime, Count]), slmcAssert);
                    Inc(Counter);
            end;
              if Counter > 0 then
                DumpSegmentsToLog;
              {$ENDIF}
            */
        }

        public SubGridCellPassesDataSegment AddNewSegment(IServerLeafSubGrid subGrid,
                                                          SubGridCellPassesDataSegmentInfo segmentInfo)
        {
            if (segmentInfo == null)
            {
                //TODO add when lgogin available
                //                SIGLogMessage.PublishNoODS(Self, Format('Null segment info passed to TICSubGridCellPassesDataList.AddNewSegment for subgrid %s', { SKIP}
                //                                              [SubGrid.Moniker]), slmcAssert);
                return null;
            }

            if (segmentInfo.Segment != null)
            {
                // TODO add when logging available
                //      SIGLogMessage.PublishNoODS(Self, Format('Segment info passed to TICSubGridCellPassesDataList.AddNewSegment for subgrid %s already contains an allocated segment', { SKIP}
                //        [SubGrid.Moniker]), slmcAssert);
                return null;
            }

            SubGridCellPassesDataSegment Result = new SubGridCellPassesDataSegment
            {
                Owner = subGrid,
                SegmentInfo = segmentInfo
            };
            segmentInfo.Segment = Result;

            //  SubGrid.CachedMemorySizeOutOfDate = True;

            //###RPW### this insertion process could be modified to use a better than linear lookup to find the
            // appropriate location to insert the segment. 

            try
            {
                for (int I = 0; I < Count; I++)
                {
                    if (segmentInfo.EndTime <= Items[I].SegmentInfo.StartTime)
                    {
                        Items.Insert(I, Result);

                        /* TODO add when more Raptor config vailable
                         * if VLPDSvcLocations.Debug_PerformSegmentAdditionIntegrityChecks then
                           begin
                             Counter:= 0;
                             for J := 0 to Count - 2 do
                                 if Items[J].SegmentInfo.StartTime >= Items[J + 1].SegmentInfo.StartTime then
                                   begin
                                     SIGLogMessage.PublishNoODS(Self, Format('Segment passes list out of order %.6f versus %.6f. Segment count = %d', { SKIP}
                                     [Items[J].SegmentInfo.StartTime, Items[J + 1].SegmentInfo.StartTime, Count]), slmcAssert);
                                     Inc(Counter);
                                   end;

                             if Counter > 0 then
                               DumpSegmentsToLog;
                           end;
                        */
                        return Result;
                    }
                }

                // if we get to here, then the new segment is at the end of the list, so just add it to the end
                Add(Result);
            }
            finally
            {
                /*
                if (Result.Owner.PresentInCache)
                {
                    if (!DataStoreInstance.GridDataCache.SubGridSegmentTouched(Result))
                        SIGLogMessage.PublishNoODS(Self, Format('Failed to touch newly created segment in TICSubGridCellPassesDataList.AddNewSegment %s [%s]', [Result.Owner.Moniker, Result.ToString]), slmcException);
                }
                */
            }

            return Result;
        }
    }
}
