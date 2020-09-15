using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.IO.Helpers;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellPassesDataSegment : ISubGridCellPassesDataSegment
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubGridCellPassesDataSegment>();

    /// <summary>
    /// Tracks whether there are unsaved changes in this segment
    /// </summary>
    public bool Dirty { get; set; }

    public DateTime StartTime = Consts.MIN_DATETIME_AS_UTC;
    public DateTime EndTime = Consts.MAX_DATETIME_AS_UTC;

    public IServerLeafSubGrid Owner { get; set; }

    public bool HasAllPasses => PassesData != null;
    public bool HasLatestData => LatestPasses != null;

    public ISubGridCellPassesDataSegmentInfo SegmentInfo { get; set; }

    public ISubGridCellSegmentPassesDataWrapper PassesData { get; set; }

    public ISubGridCellLatestPassDataWrapper LatestPasses { get; set; }

    private static readonly int _subGridSegmentPassCountLimit = DIContext.Obtain<IConfigurationStore>().GetValueInt("VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT", Consts.VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT);
    private static readonly int _subGridMaxSegmentCellPassesLimit = DIContext.Obtain<IConfigurationStore>().GetValueInt("VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT", Consts.VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT);
    private static readonly bool _segmentCleavingOperationsToLog = DIContext.Obtain<IConfigurationStore>().GetValueBool("SEGMENTCLEAVINGOOPERATIONS_TOLOG", Consts.SEGMENTCLEAVINGOOPERATIONS_TOLOG);
    private static readonly bool _itemsPersistedViaDataPersistorToLog = DIContext.Obtain<IConfigurationStore>().GetValueBool("ITEMSPERSISTEDVIADATAPERSISTOR_TOLOG", Consts.ITEMSPERSISTEDVIADATAPERSISTOR_TOLOG);

    private static readonly ISubGridCellLatestPassesDataWrapperFactory _subGridCellLatestPassesDataWrapperFactory = DIContext.Obtain<ISubGridCellLatestPassesDataWrapperFactory>();
    private static readonly ISubGridCellSegmentPassesDataWrapperFactory _subGridCellSegmentPassesDataWrapperFactory = DIContext.Obtain<ISubGridCellSegmentPassesDataWrapperFactory>();

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SubGridCellPassesDataSegment()
    {
    }

    public SubGridCellPassesDataSegment(ISubGridCellLatestPassDataWrapper latestPasses, ISubGridCellSegmentPassesDataWrapper passesData)
    {
      LatestPasses = latestPasses;
      PassesData = passesData;
    }

    /// <summary>
    /// Determines if this segments time range bounds the data time given in the time argument
    /// </summary>
    public bool SegmentMatches(DateTime time) => time >= SegmentInfo.StartTime && time < SegmentInfo.EndTime;

    public void AllocateFullPassStacks()
    {
      PassesData ??= Owner.IsMutable
        ? _subGridCellSegmentPassesDataWrapperFactory.NewMutableWrapper()
        : _subGridCellSegmentPassesDataWrapperFactory.NewImmutableWrapper();
    }

    public void AllocateLatestPassGrid()
    {
      LatestPasses ??= Owner.IsMutable
        ? _subGridCellLatestPassesDataWrapperFactory.NewMutableWrapper_Global()
        : _subGridCellLatestPassesDataWrapperFactory.NewImmutableWrapper_Global();
    }

    public void DeAllocateFullPassStacks()
    {
      PassesData?.Dispose();
      PassesData = null;
    }

    public void DeAllocateLatestPassGrid()
    {
      LatestPasses?.Dispose();
      LatestPasses = null;
    }

    public bool SavePayloadToStream(BinaryWriter writer)
    {
      if (!HasAllPasses)
        throw new TRexSubGridIOException("Leaf sub grids being written to persistent store must be fully populated with pass stacks");

      var cellStacksOffset = -1;

      // Write the cell pass information (latest and historic cell pass stacks)
      var cellStacksOffsetOffset = writer.BaseStream.Position;
      writer.Write(cellStacksOffset);

      // Segments may not have any defined latest pass information
      writer.Write(LatestPasses != null);
      LatestPasses?.Write(writer);

      cellStacksOffset = (int) writer.BaseStream.Position;

      PassesData.Write(writer);

      var endPosition = (int) writer.BaseStream.Position;

      // Write out the offset to the cell pass stacks in the file
      writer.BaseStream.Seek(cellStacksOffsetOffset, SeekOrigin.Begin);
      writer.Write(cellStacksOffset);

      writer.BaseStream.Seek(endPosition, SeekOrigin.Begin);

      return true;
    }

    public bool LoadPayloadFromStream(BinaryReader reader,
      bool loadLatestData,
      bool loadAllPasses)
    {
      // Read the stream offset where the cell pass stacks start
      var cellStacksOffset = reader.ReadInt32();

      if (HasLatestData && loadLatestData)
      {
        if (reader.ReadBoolean())
          LatestPasses.Read(reader);
        else
          LatestPasses = null;
      }

      if (!HasAllPasses && loadAllPasses)
      {
        _log.LogError("LoadPayloadFromStream asked to load all passes but segment does not have all passes store allocated");
      }

      if (HasAllPasses && loadAllPasses)
      {
        reader.BaseStream.Seek(cellStacksOffset, SeekOrigin.Begin);

        PassesData.Read(reader);
      }

      return true;
    }

    public bool Read(BinaryReader reader,
      bool loadLatestData, bool loadAllPasses)
    {
      var header = new SubGridStreamHeader(reader);

      StartTime = header.StartTime;
      EndTime = header.EndTime;

      // Read the version etc from the stream
      if (!header.IdentifierMatches(SubGridStreamHeader.kICServerSubGridLeafFileMoniker))
      {
        _log.LogError($"Sub grid segment file moniker (expected {SubGridStreamHeader.kICServerSubGridLeafFileMoniker}, found {header.Identifier}). Stream size/position = {reader.BaseStream.Length}{reader.BaseStream.Position}");
        return false;
      }

      if (!header.IsSubGridSegmentFile)
      {
        _log.LogCritical("Sub grid grid segment file does not identify itself as such in extended header flags");
        return false;
      }

      var result = false;

      if (header.Version == 1 || header.Version == 2)
        result = LoadPayloadFromStream(reader, loadLatestData, loadAllPasses);
      else
        _log.LogError($"Sub grid segment file version mismatch (expected {SubGridStreamHeader.VERSION}, found {header.Version}). Stream size/position = {reader.BaseStream.Length}{reader.BaseStream.Position}");

      return result;
    }

    public bool Write(BinaryWriter writer)
    {
      // Write the version to the stream
      var header = new SubGridStreamHeader
      {
        Identifier = SubGridStreamHeader.kICServerSubGridLeafFileMoniker,
        Flags = SubGridStreamHeader.kSubGridHeaderFlag_IsSubGridSegmentFile,
        StartTime = SegmentInfo?.StartTime ?? StartTime,
        EndTime = SegmentInfo?.EndTime ?? EndTime,
        LastUpdateTimeUTC = DateTime.UtcNow
      };

      header.Write(writer);

      SavePayloadToStream(writer);

      return true;
    }

    private void CalculateElevationRangeOfPasses()
    {
      if (!SegmentElevationRangeCalculator.CalculateElevationRangeOfPasses(PassesData, out var min, out var max))
        return;

      SegmentInfo.MinElevation = min;
      SegmentInfo.MaxElevation = max;
    }

    private void CalculateMachineDirectory()
    {
      if (!Owner.IsMutable)
        throw new ArgumentException("Only mutable cell pass collections support machine directory construction");

      SegmentInfo.MachineDirectory = PassesData.CalculateMachineDirectory();
    }

    public bool SaveToFile(IStorageProxy storage, string fileName, out FileSystemErrorStatus fsError)
    {
      bool result;
      fsError = FileSystemErrorStatus.OK;

      CalculateElevationRangeOfPasses();
      CalculateMachineDirectory();

      if (_segmentCleavingOperationsToLog || _itemsPersistedViaDataPersistorToLog)
      {
        PassesData.CalculateTotalPasses(out var totalPasses, out _, out var maxPasses);

        if (_segmentCleavingOperationsToLog && totalPasses > _subGridSegmentPassCountLimit)
          _log.LogDebug($"Saving segment {fileName} with {totalPasses} cell passes (max:{maxPasses}) which violates the maximum number of cell passes within a segment ({_subGridSegmentPassCountLimit})");

        if (_itemsPersistedViaDataPersistorToLog)
          _log.LogDebug($"Saving segment {fileName} with {totalPasses} cell passes (max:{maxPasses})");
      }

      using var stream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();
      using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
      {
        result = Write(writer);
      }

      // Log.LogInformation($"Segment persistence stream (uncompressed) for segment {FileName} containing {PassesData.SegmentPassCount} cell passes using storage proxy {storage.Mutability} is {MStream.Length} bytes (average = {MStream.Length / (1.0 * PassesData.SegmentPassCount)})");

      if (result)
      {
        fsError = storage.WriteSpatialStreamToPersistentStore(
          Owner.Owner.ID,
          fileName,
          Owner.OriginX, Owner.OriginY,
          SegmentInfo.StartTime.Ticks,
          SegmentInfo.EndTime.Ticks,
          SegmentInfo.Version,
          FileSystemStreamType.SubGridSegment,
          stream,
          this);

        result = fsError == FileSystemErrorStatus.OK;
      }

      return result;
    }

    /// <summary>
    /// Determines if this segment violates either the maximum number of cell passes within a 
    /// segment limit, or the maximum number of cell passes within a single cell within a
    /// segment limit.
    /// If either limit is breached, this segment requires cleaving
    /// </summary>
    public bool RequiresCleaving(out int totalPasses, out int maxPassCount)
    {
      const int CLEAVE_SEGMENT_PASS_COUNT_DEAD_BAND = 100;

      PassesData.CalculateTotalPasses(out totalPasses, out _, out maxPassCount);

      return totalPasses > _subGridSegmentPassCountLimit + CLEAVE_SEGMENT_PASS_COUNT_DEAD_BAND ||
             maxPassCount > _subGridMaxSegmentCellPassesLimit;
    }

    /// <summary>
    /// Verifies if the segment time range bounds are consistent with the cell passes it contains
    /// </summary>
    public bool VerifyComputedAndRecordedSegmentTimeRangeBounds()
    {
      // Determine the actual time range of the passes within the segment
      PassesData.CalculateTimeRange(out var coveredTimeRangeStart, out var coveredTimeRangeEnd);

      var result = coveredTimeRangeStart >= SegmentInfo.StartTime && coveredTimeRangeEnd <= SegmentInfo.EndTime;

      if (!result)
      {
        _log.LogCritical($"Segment computed covered time is outside segment time range bounds (CoveredTimeRangeStart={coveredTimeRangeStart}, CoveredTimeRangeEnd={coveredTimeRangeEnd}, SegmentInfo.StartTime = {SegmentInfo.StartTime}, SegmentInfo.EndTime={SegmentInfo.EndTime}");
      }

      return result;
    }

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          LatestPasses?.Dispose();
          LatestPasses = null;

          PassesData?.Dispose();
          PassesData = null;
        }

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }
}
