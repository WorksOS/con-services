using VSS.TRex.Common.Exceptions;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
  /*
  This unit implements an iterator for sub grid trees.It's purpose is to allow iteration
  over the sub grids in the same was as the ScanSubGrid() methods on the TSubGridTree
  descendants, except that mediation of the scanning is not performed by the SubGridTree
  class, but by the iterator.

  This iterator allows a separate thread to perform a scan over a sub grid tree without
  causing significant thread inter-operation issues as the scanning state is separate
  */

  public class SubGridTreeIterator
  {
    // Grid is the grid that the iterator will return sub grids from. If a scanner is set then
    // the grid will be taken from the supplied scanner
    public ISubGridTree Grid { get; set; }

    // Scanner is the scanning delegate through which additional control may be exercised
    // over pruning of the nodes and leaves that will be scanned
    public SubGridTreeScannerBase Scanner { get; set; } = null;

    // CurrentSubGrid is a reference to the current sub grid that the iterator is currently
    // up to in the sub grid tree scan. By definition, this sub grid is locked for
    // the period that FCurrent references it.
    public ISubGrid CurrentSubGrid { get; set; }

    // IterationState records the progress of the iteration by recording the path through
    // the sub grid tree which marks the progress of the iteration
    private readonly SubGridTreeIteratorStateIndex[] iterationState = new SubGridTreeIteratorStateIndex[SubGridTreeConsts.SubGridTreeLevels];

    private readonly IStorageProxy StorageProxy; //IStorageProxy[] SpatialStorageProxy = null;

    // StorageClasses controls the storage classes that will be retrieved from the database
    // for each sub grid in the iteration
    //   FStorageClasses : TICSubGridCellStorageClasses;

    // SubGridsInServerDiskStore indicates that the sub grid tree we are iterating
    // over is persistently stored on disk. In this case we must use the server
    // interface for accessing these sub grids. If this property is false then the
    // sub grid tree is completely self-contained
    private readonly bool SubGridsInServerDiskStore;

    // ReturnCachedItemsOnly allows the caller of the iterator to restrict the
    // iteration to the items that are currently in the cache.
    private readonly bool returnCachedItemsOnly = false;

    // ReturnedFirstItemInIteration keeps track of whether the first item in the iteration
    // has been returned to the caller.
    public bool ReturnedFirstItemInIteration;

    //    FDataStoreCache: TICDataStoreCache;

    protected void InitialiseIterator()
    {
      for (int I = 1; I < SubGridTreeConsts.SubGridTreeLevels; I++)
        iterationState[I].Initialise();

      if (Scanner != null)
        iterationState[1].SubGrid = Scanner.Grid.Root;
      else
        iterationState[1].SubGrid = Grid?.Root ?? throw new TRexSubGridProcessingException("Sub grid iterators require either a scanner or a grid to work from");
    }

    //        protected virtual ISubGrid GetCurrentSubGrid() => CurrentSubGrid;

    public SubGridTreeIterator(IStorageProxy storageProxy,
                               bool subGridsInServerDiskStore)
    {
      // FDataStoreCache = ADataStoreCache;
      // FStorageClasses = [icsscAllPasses];

      SubGridsInServerDiskStore = subGridsInServerDiskStore;
      StorageProxy = storageProxy;
    }

    //        public void CurrentSubgridDestroyed() => CurrentSubGrid = null;

    protected ISubGrid LocateNextSubGridInIteration()
    {
      int LevelIdx = 1;
      SubGridTreeBitmapSubGridBits DummyExistenceMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      if (iterationState[1].SubGrid == null)
        throw new TRexSubGridProcessingException("No root sub grid node assigned to iteration state");

      // Scan the levels in the iteration state until we find the lowest one we are up to
      // (This is identified by a null sub grid reference.

      while (LevelIdx < SubGridTreeConsts.SubGridTreeLevels && iterationState[LevelIdx].SubGrid != null)
        LevelIdx++;

      // Pop up to the previous level as that is the level from which we can choose a sub grid to descend into
      LevelIdx--;

      // Locate the next sub grid node...
      do
      {
        while (iterationState[LevelIdx].NextCell()) // do
        {
          ISubGrid SubGrid;
          if (LevelIdx == SubGridTreeConsts.SubGridTreeLevels - 1)
          {
            // It's a leaf sub grid we are looking for - check the existence map
            if (SubGridsInServerDiskStore)
            {
              if (returnCachedItemsOnly)
              {
                SubGrid = iterationState[LevelIdx].SubGrid.GetSubGrid((byte)iterationState[LevelIdx].XIdx, (byte)iterationState[LevelIdx].YIdx);
              }
              else
              {
                uint CellX = (uint)(iterationState[LevelIdx].SubGrid.OriginX + iterationState[LevelIdx].XIdx * SubGridTreeConsts.SubGridTreeDimension);
                uint CellY = (uint)(iterationState[LevelIdx].SubGrid.OriginY + iterationState[LevelIdx].YIdx * SubGridTreeConsts.SubGridTreeDimension);

                SubGrid = Utilities.SubGridUtilities.LocateSubGridContaining
                           (StorageProxy,
                            iterationState[LevelIdx].SubGrid.Owner as ServerSubGridTree,
                            //null, //FDataStoreCache,
                            CellX, CellY,
                            SubGridTreeConsts.SubGridTreeLevels,
                            false, false);
              }
            }
            else
            {
              SubGrid = iterationState[LevelIdx].SubGrid.GetSubGrid((byte)iterationState[LevelIdx].XIdx, (byte)iterationState[LevelIdx].YIdx);
            }
          }
          else
          {
            SubGrid = iterationState[LevelIdx].SubGrid.GetSubGrid((byte)iterationState[LevelIdx].XIdx, (byte)iterationState[LevelIdx].YIdx);
          }

          if (SubGrid != null)
          {
            bool allowedToUseSubGrid = false;

            // Are we allowed to do anything with it?
            if (SubGrid.IsLeafSubGrid())
            {
              allowedToUseSubGrid = Scanner == null || Scanner.OnProcessLeafSubgrid(SubGrid);
            }
            else
            {
              if (Scanner != null)
              {
                switch (Scanner.OnProcessNodeSubgrid(SubGrid, DummyExistenceMap))
                {
                  case Types.SubGridProcessNodeSubGridResult.OK:
                    allowedToUseSubGrid = true;
                    break;

                  case Types.SubGridProcessNodeSubGridResult.DontDescendFurther:
                    break;

                  case Types.SubGridProcessNodeSubGridResult.TerminateProcessing:
                    Scanner.Abort();
                    return null;
                }
              }
              else
              {
                allowedToUseSubGrid = true;
              }
            }

            if (!allowedToUseSubGrid)
            {
              continue;
            }

            if (SubGrid.IsLeafSubGrid())
            {
              // It's a leaf sub grid - so use it
              return SubGrid;
            }

            // It's a node sub grid that contains other node sub grids or leaf sub grids - descend into it
            LevelIdx++;
            iterationState[LevelIdx].SubGrid = SubGrid;

          }
        }

        LevelIdx--;
      } while (LevelIdx > 0);

      return null;
    }

    // MoveToFirstSubGrid moves to the first sub grid in the tree that satisfies the scanner
    private bool MoveToFirstSubGrid()
    {
      InitialiseIterator();

      ReturnedFirstItemInIteration = true;

      return MoveToNextSubGrid();
    }

    // MoveToNextSubGrid moves to the next sub grid in the tree that satisfies the scanner
    public bool MoveToNextSubGrid()
    {
      ISubGrid SubGrid;

      bool Result;

      if (!ReturnedFirstItemInIteration)
      {
        return MoveToFirstSubGrid() && CurrentSubGrid != null;
      }

      do
      {
        SubGrid = LocateNextSubGridInIteration();

        if (SubGrid == null) // We are at the end of the iteration
        {
          CurrentSubGrid = null;
          return false;
        }

        Result = Scanner == null || (Scanner.OnProcessLeafSubgrid(SubGrid) && !Scanner.Aborted);
      } while (!Result && !(Scanner != null && Scanner.Aborted));

      CurrentSubGrid = Result ? SubGrid : null;

      return Result;
    }

    public void Reset() => ReturnedFirstItemInIteration = false;
  }
}
