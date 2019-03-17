﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server
{
  public class ServerSubGridTree : SubGridTree, IServerSubGridTree 
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ServerSubGridTree>();

    /// <summary>
    /// Controls emission of sub grid reading activities into the log.
    /// </summary>
    public bool RecordSubGridFileReadingToLog { get; set; } = false;

    private readonly bool _segmentCleavingOperationsToLog = DIContext.Obtain<IConfigurationStore>().GetValueBool("SEGMENTCLEAVINGOOPERATIONS_TOLOG", Consts.SEGMENTCLEAVINGOOPERATIONS_TOLOG);
    
    public ServerSubGridTree(Guid siteModelID) :
      base(SubGridTreeConsts.SubGridTreeLevels, SubGridTreeConsts.DefaultCellSize,
        new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>())
    {
      ID = siteModelID; // Ensure the ID of the sub grid tree matches the datamodel ID
    }

    public ServerSubGridTree(byte numLevels,
      double cellSize,
      ISubGridFactory subGridFactory) : base(numLevels, cellSize, subGridFactory)
    {
    }

    /// <summary>
    /// Computes a unique file name for a segment within a particular sub grid
    /// </summary>
    /// <param name="CellAddress"></param>
    /// <param name="SegmentInfo"></param>
    /// <returns></returns>
    public static string GetLeafSubGridSegmentFullFileName(SubGridCellAddress CellAddress,
      ISubGridCellPassesDataSegmentInfo SegmentInfo)
    {
      // Work out the cell address of the origin cell in the appropriate leaf
      // sub grid. We use this cell position to derive the name of the file
      // containing the leaf sub grid data
      return SegmentInfo.FileName((uint) (CellAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask),
        (uint) (CellAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask));
    }

    public bool LoadLeafSubGridSegment(IStorageProxy storageProxy,
      SubGridCellAddress cellAddress,
      bool loadLatestData,
      bool loadAllPasses,
      IServerLeafSubGrid SubGrid,
      ISubGridCellPassesDataSegment Segment)
    {
      bool needToLoadLatestData = loadLatestData && !Segment.HasLatestData;
      bool needToLoadAllPasses = loadAllPasses && !Segment.HasAllPasses;

      if (!needToLoadLatestData && !needToLoadAllPasses)
        return true; // Nothing more to do here

      // Lock the segment briefly while its contents is being loaded
      lock (Segment)
      {
        if (loadLatestData == !Segment.HasLatestData && needToLoadAllPasses == !Segment.HasAllPasses)
          return true; // The load operation was performed on another thread. Leave quietly

        // Ensure the appropriate storage is allocated
        if (needToLoadLatestData)
          Segment.AllocateLatestPassGrid();

        if (needToLoadAllPasses)
          Segment.AllocateFullPassStacks();

        if (!Segment.SegmentInfo.ExistsInPersistentStore)
          return true; // Nothing more to do here

        // Locate the segment file and load the data from it
        string FullFileName = GetLeafSubGridSegmentFullFileName(cellAddress, Segment.SegmentInfo);

        // Load the cells into it from its file
        bool FileLoaded = SubGrid.LoadSegmentFromStorage(storageProxy, FullFileName, Segment, needToLoadLatestData, needToLoadAllPasses);

        if (!FileLoaded)
        {
          // Oops, something bad happened. Remove the segment from the list. Return failure to the caller.
          if (loadAllPasses)
            Segment.DeAllocateFullPassStacks();

          if (loadLatestData)
            Segment.DeAllocateLatestPassGrid();

          return false;
        }
      }

      return true;
    }

    public bool LoadLeafSubGrid(IStorageProxy storageProxy,
      SubGridCellAddress CellAddress,
      bool loadAllPasses, bool loadLatestPasses,
      IServerLeafSubGrid SubGrid)
    {
      string FullFileName = string.Empty;
      bool Result = false;

      try
      {
        // Loading contents into a dirty sub grid (which should happen only on the mutable nodes), or
        // when there is already content in the segment directory are strictly forbidden and break immutability
        // rules for sub grids
        if (SubGrid.Dirty)
          throw new TRexSubGridIOException("Leaf sub grid directory loads may not be performed while the sub grid is dirty. The information should be taken from the cache instead.");

        // Load the cells into it from its file
        // Briefly lock this sub grid just for the period required to read its contents
        lock (SubGrid)
        {
          // If more than thread wants this sub grid then they may concurrently attempt to load it.
          // Make a check to see if this has happened and another thread has already loaded this sub grid directory.
          // If so, just return success. Previously the commented out assert was enforced causing exceptions
          // Debug.Assert(SubGrid.Directory?.SegmentDirectory?.Count == 0, "Loading a leaf sub grid directory when there are already segments present within it.");

          // Check this thread is the winner of the lock to be able to load the contents
          if (SubGrid.Directory?.SegmentDirectory?.Count != 0)
            return true; // The load has occurred on another thread, leave quietly...

          // Ensure the appropriate storage is allocated
          if (loadAllPasses)
            SubGrid.AllocateLeafFullPassStacks();

          if (loadLatestPasses)
            SubGrid.AllocateLeafLatestPassGrid();

          FullFileName = GetLeafSubGridFullFileName(CellAddress);

          // Briefly lock this sub grid just for the period required to read its contents
          Result = SubGrid.LoadDirectoryFromFile(storageProxy, FullFileName);
        }
      }
      finally
      {
        if (Result && RecordSubGridFileReadingToLog)
          Log.LogDebug($"Sub grid file {FullFileName} read from persistent store containing {SubGrid.Directory.SegmentDirectory.Count} segments (Moniker: {SubGrid.Moniker()}");
      }

      return Result;
    }

    public static string GetLeafSubGridFullFileName(SubGridCellAddress CellAddress)
    {
      // Work out the cell address of the origin cell in the appropriate leaf
      // sub grid. We use this cell position to derive the name of the file
      // containing the leaf sub grid data
      return ServerSubGridTreeLeaf.FileNameFromOriginPosition(new SubGridCellAddress((uint) (CellAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask), (uint) (CellAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask)));
    }

    /// <summary>
    /// Orchestrates all the activities relating to saving the state of a created or modified sub grid, including
    /// cleaving, saving updated elements, creating new elements and arranging for the retirement of
    /// elements that have been replaced in the persistent store as a result of this activity.
    /// </summary>
    /// <param name="subGrid"></param>
    /// <param name="storageProxy"></param>
    /// <returns></returns>
    public bool SaveLeafSubGrid(IServerLeafSubGrid subGrid, IStorageProxy storageProxy, List<ISubGridSpatialAffinityKey> invalidatedSpatialStreams)
    {
      try
      {
        // Perform segment cleaving as the first activity in the action of saving a leaf sub grid
        // to disk. This reduces the number of segment cleaving actions that would otherwise
        // be performed in the context of the aggregated integrator.

        if (_segmentCleavingOperationsToLog)
          Log.LogDebug($"About to perform segment cleaving on {subGrid.Moniker()}");

        var cleaver = new SubGridSegmentCleaver();

        cleaver.PerformSegmentCleaving(storageProxy, subGrid);

        // Calculate the cell last pass information here, immediately before it is
        // committed to the persistent store. The reason for this is to remove this
        // compute intensive operation from the critical path in TAG file processing
        // (which is the only writer of this information in the Raptor system).
        subGrid.ComputeLatestPassInformation(true, storageProxy);

        if (_segmentCleavingOperationsToLog)
          Log.LogInformation($"SaveLeafSubGrid: {subGrid.Moniker()} ({subGrid.Cells.PassesData.Count} segments)");

        var ModifiedOriginaSegments = new List<ISubGridCellPassesDataSegment>(100);
        var NewSegmentsFromCleaving = new List<ISubGridCellPassesDataSegment>(100);

        var OriginAddress = new SubGridCellAddress(subGrid.OriginX, subGrid.OriginY);

        // The following used to be an assert/exception. However, this is may readily
        // happen if there are no modified segments resulting from processing a
        // process TAG file where the Dirty flag for the sub grid is set but no cell
        // passes are added to segments in that sub grid. As this is not an integrity
        // issue the persistence of the modified sub grid is allowed, but noted in
        // the log for posterity.
        if (subGrid.Cells.PassesData.Count == 0)
          Log.LogInformation(
            $"Note: Saving a sub grid, {subGrid.Moniker()}, (Segments = {subGrid.Cells.PassesData.Count}, Dirty = {subGrid.Dirty}) with no cached sub grid segments to the persistent store in SaveLeafSubGrid (possible reprocessing of TAG file with no cell pass changes). " +
            $"SubGrid.Directory.PersistedClovenSegments.Count={cleaver.PersistedClovenSegments?.Count}, ModifiedOriginalFiles.Count={ModifiedOriginaSegments.Count}, NewSegmentsFromCleaving.Count={NewSegmentsFromCleaving.Count}");

        SubGridSegmentIterator Iterator = new SubGridSegmentIterator(subGrid, storageProxy)
        {
          IterationDirection = IterationDirection.Forwards,
          ReturnDirtyOnly = true,

          // We don't consider saving of segments to disk to be equivalent to
          // 'use' of the segments so they are not touched with respect to the cache
          // when saved. This aids segments that might not be updated again in TAG
          // processing to exit the cache sooner and also removes clashes with the
          // cache with it performs cache resize operations.
          MarkReturnedSegmentsAsTouched = false
        };

        //**********************************************************************
        //***Construct list of original segment files that have been modified***
        //*** These files may be updated in situ with no sub grid/segment    ***
        //*** integrity issues wrt the segment directory in the sub grid     ***
        //**********************************************************************

        Iterator.MoveToFirstSubGridSegment();
        while (Iterator.CurrentSubGridSegment != null)
        {
          if (Iterator.CurrentSubGridSegment.SegmentInfo.ExistsInPersistentStore && Iterator.CurrentSubGridSegment.Dirty)
            ModifiedOriginaSegments.Add(Iterator.CurrentSubGridSegment);
          Iterator.MoveToNextSubGridSegment();
        }

        //**********************************************************************
        //*** Construct list of spatial streams that will be deleted or replaced
        //*** in the FS file. These will be passed to the call that saves the
        //*** sub grid directory file as an instruction to place them into the
        //*** deferred deletion list
        //**********************************************************************

        if (cleaver.PersistedClovenSegments != null)
          invalidatedSpatialStreams.AddRange(cleaver.PersistedClovenSegments);

        invalidatedSpatialStreams.AddRange(ModifiedOriginaSegments.Select(x => x.SegmentInfo.AffinityKey(ID)));

        //**********************************************************************
        //*** Construct list of new segment files that have been created     ***
        //*** by the cleaving of other segments in the sub grid              ***
        //**********************************************************************

        Iterator.MoveToFirstSubGridSegment();
        while (Iterator.CurrentSubGridSegment != null)
        {
          if (!Iterator.CurrentSubGridSegment.SegmentInfo.ExistsInPersistentStore)
            NewSegmentsFromCleaving.Add(Iterator.CurrentSubGridSegment);

          Iterator.MoveToNextSubGridSegment();
        }

        if (NewSegmentsFromCleaving.Count > 0)
        {
          //**********************************************************************
          //***    Write new segment files generated by cleaving               ***
          //*** File system integrity failures here will have no effect on the ***
          //*** sub grid/segment directory as they are not referenced by it.   ***
          //*** At worst they become orphans they may be cleaned by in the FS  ***
          //*** recovery phase                                                 ***
          //**********************************************************************

          if (_segmentCleavingOperationsToLog)
            Log.LogInformation($"Sub grid has {NewSegmentsFromCleaving.Count} new segments from cleaving");

          foreach (var segment in NewSegmentsFromCleaving)
          {
            // Update the version of the segment as it is about to be written
            segment.SegmentInfo.Touch();

            segment.SaveToFile(storageProxy, GetLeafSubGridSegmentFullFileName(OriginAddress, segment.SegmentInfo), out FileSystemErrorStatus FSError);

            if (FSError == FileSystemErrorStatus.OK)
            {
              if (_segmentCleavingOperationsToLog)
                Log.LogInformation($"Saved new cloven grid segment file: {GetLeafSubGridSegmentFullFileName(OriginAddress, segment.SegmentInfo)}");
            }
            else
            {
              Log.LogWarning($"Failed to save grid segment {GetLeafSubGridSegmentFullFileName(OriginAddress, segment.SegmentInfo)}: Error:{FSError}");
              return false;
            }
          }
        }

        if (ModifiedOriginaSegments.Count > 0)
        {
          //**********************************************************************
          //***    Write modified segment files                                ***
          //*** File system integrity failures here will have no effect on the ***
          //*** sub grid/segment directory as the previous version of the      ***
          //*** modified file being written will be recovered.                 ***
          //**********************************************************************

          Log.LogDebug($"Sub grid has {ModifiedOriginaSegments.Count} modified segments");

          foreach (var segment in ModifiedOriginaSegments)
          {
            // Update the version of the segment as it is about to be written
            segment.SegmentInfo.Touch();

            if (segment.SaveToFile(storageProxy, GetLeafSubGridSegmentFullFileName(OriginAddress, segment.SegmentInfo), out FileSystemErrorStatus FSError))
              Log.LogDebug($"Saved modified grid segment file: {segment}");
            else
            {
              Log.LogError($"Failed to save grid segment {GetLeafSubGridSegmentFullFileName(OriginAddress, segment.SegmentInfo)}: Error:{FSError}");
              return false;
            }
          }
        }

        //**********************************************************************
        //***                 Write the sub grid directory file              ***
        //**********************************************************************

        // Add the stream representing the sub grid directory file to the list of
        // invalidated streams as this stream will be replaced with the stream
        // containing the updated directory information. Note: This only needs to
        // be done if the sub grid has previously been read from the FS file (if not
        // it has been created and now yet persisted to the store.

        if (subGrid.Directory.ExistsInPersistentStore)
        {
          // Include an additional invalidated spatial stream for the sub grid directory stream
          invalidatedSpatialStreams.Add(subGrid.AffinityKey());
        }

        if (subGrid.SaveDirectoryToFile(storageProxy, GetLeafSubGridFullFileName(OriginAddress)))
        {
          Log.LogDebug($"Saved grid directory file: {GetLeafSubGridFullFileName(OriginAddress)}");
        }
        else
        {
          Log.LogError($"Failed to save grid: {GetLeafSubGridFullFileName(OriginAddress)}");
          return false;
        }

        //**********************************************************************
        //***                   Reset segment dirty flags                    ***
        //**********************************************************************

        Iterator.MoveToFirstSubGridSegment();
        while (Iterator.CurrentSubGridSegment != null)
        {
          Iterator.CurrentSubGridSegment.Dirty = false;
          Iterator.CurrentSubGridSegment.SegmentInfo.ExistsInPersistentStore = true;

          Iterator.MoveToNextSubGridSegment();
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception raised in SaveLeafSubGrid");
      }

      return false;
    }
  }
}
