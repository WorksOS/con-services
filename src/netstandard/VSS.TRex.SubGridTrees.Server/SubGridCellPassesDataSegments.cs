using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellPassesDataSegments : ISubGridCellPassesDataSegments
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(nameof(SubGridCellPassesDataSegments));

    private readonly bool _performSegmentAdditionIntegrityChecks = DIContext.Obtain<IConfigurationStore>().GetValueBool("DEBUG_PERFORMSEGMENT_ADDITIONALINTEGRITYCHECKS", Consts.kPerformSegmentAdditionIntegrityChecksDefault);

    public List<ISubGridCellPassesDataSegment> Items { get; set; } = new List<ISubGridCellPassesDataSegment>();

    public void Clear()
    {
      Items?.Clear();
    }

    public int Count => Items?.Count ?? 0;

    public ISubGridCellPassesDataSegment this[int index]
    {
      get { return Items[index]; }
    }

    public int Add(ISubGridCellPassesDataSegment item)
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

    /// <summary>
    /// Dumps the segment metadata for this subgrid to the log
    /// </summary>
    private void DumpSegmentsToLog()
    {
      for (int i = 0; i < Count; i++)
        Log.LogInformation($"Seg #{i}: {Items[i]}");
    }

    public ISubGridCellPassesDataSegment AddNewSegment(IServerLeafSubGrid subGrid,
      ISubGridCellPassesDataSegmentInfo segmentInfo)
    {
      if (segmentInfo == null)
      {
        Log.LogCritical($"Null segment info passed to AddNewSegment for subgrid {subGrid.Moniker()}");
        return null;
      }

      if (segmentInfo.Segment != null)
      {
        Log.LogCritical($"'Segment info passed to AddNewSegment for subgrid {subGrid.Moniker()} already contains an allocated segment");
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

            if (_performSegmentAdditionIntegrityChecks)
            {
              int Counter = 0;
              for (int J = 0; J < Count - 1; J++)
                if (Items[J].SegmentInfo.StartTime >= Items[J + 1].SegmentInfo.StartTime)
                {
                  Log.LogCritical($"Segment passes list out of order {Items[J].SegmentInfo.StartTime} versus {Items[J + 1].SegmentInfo.StartTime}. Segment count = {Count}");
                  Counter++;
                }

              if (Counter > 0)
                DumpSegmentsToLog();
            }

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
