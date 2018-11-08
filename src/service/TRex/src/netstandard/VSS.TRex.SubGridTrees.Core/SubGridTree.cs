using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Utilities;
using VSS.TRex.Utilities.ExtensionMethods;

/*
Glossary

1.	SubGrid : A structure that stores a square section of a much larger grid.
    The larger grid is an aggregate of the subgrids it contains.
2.	Cell : An entry in the list of references contained in a sub-grid. A cell
    may be a reference to another subgrid, a reference to a totally different
    class of object, or be null.
3.	‘On-the-ground cell’: A cell whose purpose is to record information about
    a single ‘grid cell’. Eg: The height, or radio latency, or Caterpillar
    CompactionValue etc recorded for a specific place (grid cell) in the site.
    These values may be singular in the case of information supplied to a client
    to fulfill a request, or many valued in the case of the processed machine
    pass history for the cell. The extents of a grid cell is defined as 1 foot
    square (Imperial) or 250mm square (Metric)

Overview

This structure is tended to provide the following capabilities:

1.	Spatial index through which any designated on-the-ground grid cell may be
    located in no more than 6 steps (ie: O(6)). On-the-ground cells are thought
    of as entities that live in a virtual integerised space where the extents
    of the cell is represented by an integer quantity (either [X, Y] grid location
    or bit interleaved liner index etc). Index is capable of uniquely identifying,
    indexing, locating and storing approximately 1 sextillion cells in the grid
    (that’s 1 followed by 18 zeros -> 1,000,000,000,000,000,000).
2.	Storage of the following on an on-the-ground cell basis
    a.	Single processed value
        eg: Fully processed machine pass, or single attribute information from a fully processed machine pass.
    b.	Complete machine pass history
        ie: List of fully processed machine passes
3.	Provide caching support
    Sub-grids containing on-the-ground cell arrays may be held on disk,
    or in-memory. If the on-the-ground sub-grid is cached, then its reference
    in the grid cells is non-null. This caching is necessary due to the data
    volumes involved.
4.	Provide consistent structure that both client and server sides may use to
    represent, store and search the IC spatial data.
5.	Provides level of detail inherent in layered nature of structure.
    A client requesting display data can determine which level of the grid they
    should request data from. Sub grids can maintain a representative value for
    this purpose.

This sub grid tree is not aware of the structured or storage mechanism used for
the bottom leaf nodes (ie: the nodes containing the actual cell data). Thus, the
access mechanisms chiefly revolve around sub-grids, rather than actual cells
themselves, apart from cell X/Y location references used to locate subgrids.
*/

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// Base class for implementation of sub grid trees that defines various parameters and constants related to them
    /// </summary>
    public class SubGridTree : ISubGridTree, IEnumerator<ISubGrid>
    {
        private static ILogger Log = Logging.Logger.CreateLogger("SubGridTree");

        /****************************** Internal members **************************/

        /// <summary>
        /// The number of bits in the X and Y cell index keys that are used by the grid. The used bits are always the LSB bits.
        /// </summary>
        private byte NumBitsInKeys => (byte)(NumLevels * SubGridTreeConsts.SubGridIndexBitsPerLevel);

        /// <summary>
        /// The maximum (positive and negative) real world value for both X and Y axes that may be encompassed by the grid
        /// </summary>
        private double MaxOrdinate;

        /// <summary>
        /// The first (top) subgrid in the tree. All other subgrids in the tree descend via this root node.
        /// </summary>
        public INodeSubGrid Root { get; set; }

        /// <summary>
        /// Internal numeric identifier for the sub grid tree. All internal operations will refer to the sub grid
        /// tree using this identitifer. 
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// External identifier (GUID) for the subgrid tree. The instance may be tagged with this ID as an 
        /// association to the primary numeric identifier.
        /// </summary>
        public Guid ExternalID { get; set; }

        /// <summary>
        /// The number of levels defined in this subgrid tree. 
        /// A 6 level tree typically defines leaf cells as relating to on-the-ground cell in the real world
        /// coordinate system (eg: cells tracking passes made by construction machines)
        /// A 5 level tree typically defines leaf cells that represent some aspect of the subgrids in the 
        /// 6th layer of the tree containing on-the-groun leaf cells (eg: subgrid existence map)
        /// This property is assignable only at the time the subgrid tree is constructed.
        /// </summary>
        public byte NumLevels { get; }

        /// <summary>
        /// Backing store field for the CellSize property
        /// </summary>
        private double cellSize;

        /// <summary>
        /// Store the 'divide by 2' version of cellsize to reduce compute in operations that use this quantity a lot
        /// </summary>
        public double CellSizeOver2;

        /// <summary>
        /// The real world size on the ground of a cell in the grid. This applies to tree with different numbers of levels.
        /// This property is assignable only at the time the subgrid tree is constructed.
        /// </summary>
        public double CellSize { get { return cellSize; } set { SetCellSize(value); } }

        /// <summary>
        /// Setter for the CellSize property that sets related members of the class dependent on the value of CellSize
        /// </summary>
        /// <param name="value"></param>
        private void SetCellSize(double value)
        {
            cellSize = value;
            CellSizeOver2 = cellSize / 2;
            MaxOrdinate = (1 << NumBitsInKeys) * cellSize;
        }

        /// <summary>
        /// The subgrid factory to create subgrids for the subgrid tree
        /// </summary>
        private ISubGridFactory SubgridFactory { get; }

        // CreateNewSubgrid creates a new subgrid relevant to the requested level
        // in the tree. This new subgrid is not added into the tree structure -
        // it is unattached until explicitly inserted.
        public ISubGrid CreateNewSubgrid(byte level) => SubgridFactory.GetSubGrid(this, level);

        /// <summary>
        /// The value of the index origin offset for this sub grid tree
        /// </summary>
        public uint IndexOriginOffset { get; }

        /// <summary>
        /// Base Sub Grid Tree constructor. Creates a tree with the requested numner of levels, 
        /// using the requested cell size for leaf cells and th esupplied subgrid factory to create
        /// its leaf and node subgrids
        /// </summary>
        /// <param name="numLevels"></param>
        /// <param name="cellSize"></param>
        /// <param name="subGridfactory"></param>
        public SubGridTree(byte numLevels,
                           double cellSize,
                           ISubGridFactory subGridfactory)
        {
            if (numLevels < 1 || numLevels > SubGridTreeConsts.SubGridTreeLevels)
            {
                // Invalid number of tree levels
                throw new ArgumentException($"Number of levels must be between 1 and {SubGridTreeConsts.SubGridTreeLevels}", nameof(numLevels));
            }

            if (cellSize < 0.01 || cellSize > 1000000)
            {
                // Invalid cell size
                throw new ArgumentException("CellSize must be between 0.01 and 1000000", nameof(cellSize));
            }

            NumLevels = numLevels;
            SetCellSize(cellSize);
            IndexOriginOffset = (uint)1 << (NumBitsInKeys - 1);

            SubgridFactory = subGridfactory ?? throw new ArgumentException("A subgrid factory must be specified", nameof(subGridfactory));

            // Construct the root node for the tree
            InitialiseRoot();
        }

        /// <summary>
        /// Clear destroys all node/leaf information contained within the
        /// sub grid tree. After Clear has been called, the state of the tree in terms
        /// of nodes it contains is the same as after initial creation of the object.
        /// </summary>
        public void Clear()
        {
            InitialiseRoot();
        }

        private void InitialiseRoot()
        {
            // Free & recreate the root node
            Root = CreateNewSubgrid(1) as INodeSubGrid;
        }

        /// <summary>
        /// CalculateIndexOfCellContainingPosition takes a world position and determines
        /// the X/Y index of the cell that the position lies in. If the position is
        /// outside of the extent covered by the grid the function returns false. 
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="CellSize"></param>
        /// <param name="IndexOriginOffset"></param>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        public static void CalculateIndexOfCellContainingPosition(double X, double Y,
                                                                  double CellSize,
                                                                  uint IndexOriginOffset,
                                                                  out uint CellX, out uint CellY)
        {
          // Calculate the cell index in a centered [0,0] origin basis
          // Convert the cell indexes into a bottom left [0,0] basis (ie: to grid cell
          // indexes in the top right quadrant).
            CellX = (uint)Math.Floor(X / CellSize) + IndexOriginOffset;
            CellY = (uint)Math.Floor(Y / CellSize) + IndexOriginOffset;
        }

        /// <summary>
        /// CalculateIndexOfCellContainingPosition takes a world position and determines
        /// the X/Y index of the cell that the position lies in. If the position is
        /// outside of the extent covered by the grid the function returns false.  
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public bool CalculateIndexOfCellContainingPosition(double X, double Y,
                                                           out uint CellX, out uint CellY)
        {
            if (Math.Abs(X) > MaxOrdinate || Math.Abs(Y) > MaxOrdinate)
            {
                CellX = uint.MaxValue;
                CellY = uint.MaxValue;

                return false;
            }

            CalculateIndexOfCellContainingPosition(X, Y, CellSize, IndexOriginOffset, out CellX, out CellY);

            return true;
        }

        /// <summary>
        /// CalculateRegionGridCoverage determines the extent of on-the-ground grid cells that correspond to the given world extent.
        /// </summary>
        /// <param name="worldExtent"></param>
        /// <param name="cellExtent"></param>
        /// <returns></returns>
        public bool CalculateRegionGridCoverage(BoundingWorldExtent3D worldExtent, out BoundingIntegerExtent2D cellExtent)
        {
            try
            {
                // First calculate the leaf data cell indexes for the given real world extent
                // If the world coorindates lie outside of the extent covered by the grid, then
                // clamp them to the boundaries of the grid.
                if (CalculateIndexOfCellContainingPosition(
                        Range.EnsureRange(worldExtent.MinX, -MaxOrdinate, MaxOrdinate),
                        Range.EnsureRange(worldExtent.MinY, -MaxOrdinate, MaxOrdinate),
                        out uint minX,
                        out uint minY) &&
                    CalculateIndexOfCellContainingPosition(
                        Range.EnsureRange(worldExtent.MaxX, -MaxOrdinate, MaxOrdinate),
                        Range.EnsureRange(worldExtent.MaxY, -MaxOrdinate, MaxOrdinate),
                        out uint maxX,
                        out uint maxY))
                {
                    cellExtent = new BoundingIntegerExtent2D((int) minX, (int) minY, (int) maxX, (int) maxY);
                    return true;
                }

                cellExtent = new BoundingIntegerExtent2D();
                return false;
            }
            catch (Exception e)
            {
              // Something bad happened...
              Log.LogDebug($"Exception in SubGridTree.CalculateRegionGridCoverage: {e}");
              cellExtent = BoundingIntegerExtent2D.Inverted();
              return false;
            }
        }

        /// <summary>
        /// Returns the maximum world extent that this grid is capable of covering.
        /// </summary>
        /// <returns></returns>
        public BoundingWorldExtent3D FullGridExtent()
        {
            return new BoundingWorldExtent3D(-IndexOriginOffset * CellSize,
                                             -IndexOriginOffset * CellSize,
                                             IndexOriginOffset * CellSize,
                                             IndexOriginOffset * CellSize);
        }

        /// <summary>
        /// FullCellExtent returns the total extent of cells within this subgridtree. 
        /// </summary>
        /// <returns></returns>
        public BoundingIntegerExtent2D FullCellExtent()
        {
            int numCellsAcrossGrid = 1 << (NumLevels * SubGridTreeConsts.SubGridIndexBitsPerLevel);

            return new BoundingIntegerExtent2D(0, 0, numCellsAcrossGrid, numCellsAcrossGrid);
        }

        /// <summary>
        /// ScanSubgrids scans all subgrids at a requested level in the tree that
        /// intersect the given real world extent. Each subgrid that exists in the
        /// extent is passed to the OnProcessLeafSubgrid event for processing 
        /// </summary>
        /// <param name="extent"></param>
        /// <param name="leafFunctor"></param>
        /// <param name="nodeFunctor"></param>
        /// <returns></returns>
        public bool ScanSubGrids(BoundingWorldExtent3D extent,
                                 Func<ISubGrid, bool> leafFunctor = null,
                                 Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null)
        {
            // First calculate the leaf data cell indexes for the given real world extent
            if (!CalculateRegionGridCoverage(extent, out BoundingIntegerExtent2D cellExtent))
            {
                // The extents requested lie at least partially ouside the tree. Ignore this request.
                Log.LogWarning($"{nameof(ScanSubGrids)} could not convert {extent} in a cell extent");
                return false;
            }

            // Given the on-the-ground cell extents we can now ask the subgrids to recursively scan themselves.

            return Root.ScanSubGrids(cellExtent, leafFunctor, nodeFunctor);
        }

        /// <summary>
        /// ScanSubgrids scans all subgrids at a requested level in the tree that
        /// intersect the given cell address space extent. Each subgrid that exists in the
        /// extent is passed to the OnProcessLeafSubgrid event for processing 
        /// </summary>
        /// <param name="extent"></param>
        /// <param name="leafFunctor"></param>
        /// <param name="nodeFunctor"></param>
        /// <returns></returns>
        public bool ScanSubGrids(BoundingIntegerExtent2D extent,
                                 Func<ISubGrid, bool> leafFunctor = null,
                                 Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null)
        {
            return Root.ScanSubGrids(extent, leafFunctor, nodeFunctor);
        }

        /// <summary>
        /// ScanSubgrids scans all subgrids. Each subgrid that exists in the
        /// extent is passed to the OnProcessLeafSubgrid event for processing 
        /// </summary>
        /// <param name="leafFunctor"></param>
        /// <param name="nodeFunctor"></param>
        /// <returns></returns>
        public bool ScanAllSubGrids(Func<ISubGrid, bool> leafFunctor = null,
                                    Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null)
        {
            return ScanSubGrids(FullGridExtent(), leafFunctor, nodeFunctor);
        }

        /// <summary>
        /// ConstructPathToCell constructs all necessary subgrids in all levels in
        /// the tree so that there is a traversable path from the root of the
        /// tree to the leaf subgrid that will contain the cell identified by
        /// CellX and CellY. If PathType is pctCreateLeaf it returns the leaf
        /// subgrid instance into which the caller may place the cell data. If
        /// PathType is pctCreatePathToLeaf it returns the node subgrid instance that
        /// owns the leaf subgrid that contains the cell
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="pathType"></param>
        /// <returns></returns>
        public ISubGrid ConstructPathToCell(uint cellX, uint cellY,
                                            SubGridPathConstructionType pathType)
        {
            byte subGridCellX, subGridCellY;

            ISubGrid result = null;
            INodeSubGrid subGrid = Root;

            // First descend through the node levels of the tree
            for (byte I = 1; I < NumLevels - 1; I++) // Yes, -1 because we choose a subgrid cell from the next level down...
            {
                if (subGrid.GetSubGridContainingCell(cellX, cellY, out subGridCellX, out subGridCellY))
                {
                    // Walk into this cell in the subgrid as the next level down in this path exists
                    subGrid = subGrid.GetSubGrid(subGridCellX, subGridCellY) as INodeSubGrid;
                }
                else
                {
                    if (pathType == SubGridPathConstructionType.ReturnExistingLeafOnly)
                    {
                        // We do not need to descend further - the leaf subgrid we are interested in does not exist.
                        break;
                    }

                    INodeSubGrid lockSubGrid = subGrid;
                    // Obtain a lock on the node subgrid to create and add the new child node subgrid to it
                    lock (lockSubGrid)
                    {
                        // Check to see if another thread was able to create the subgrid before the
                        // lock was obtained. Is so just return it
                        if (subGrid.GetSubGridContainingCell(cellX, cellY, out subGridCellX, out subGridCellY))
                        {
                            // Walk into this cell in the subgrid as the next level down in this path exists
                            subGrid = subGrid.GetSubGrid(subGridCellX, subGridCellY) as INodeSubGrid;
                        }
                        else
                        {
                            // We need to create a new subgrid
                            ISubGrid newSubGrid = CreateNewSubgrid((byte) (I + 1));

                            // ... and add it into this subgrid as the subgrid cell coordinates
                            // returned by GetSubGridContainingCell ...
                             subGrid.SetSubGrid(subGridCellX, subGridCellY, newSubGrid);

                            // ... then carry on descending into it
                            subGrid = newSubGrid as INodeSubGrid;
                        }
                    }
                }
            }

            // Now check to see if the cell in the node level directly above the leaf
            // level contains a reference to the appropriate leaf subgrid
            if (subGrid != null)
            {
                // If requested, create the new leaf subgrid, otherwise return the
                // node subgrid stored in SubGrid to the caller
                switch (pathType)
                {
                    case SubGridPathConstructionType.CreateLeaf:
                        // Get the local subgrid index in this subgrid that the leaf subgrid that
                        // contains the cell resides at
                        subGrid.GetSubGridCellIndex(cellX, cellY, out subGridCellX, out subGridCellY);

                        // Check if the leaf subgrid already exists, if not then create it.
                        result = subGrid.GetSubGrid(subGridCellX, subGridCellY);

                        if (result == null)
                        {
                            // Lock the node subgrid model while creating the new child leaf subgrid within it.
                            lock (subGrid)
                            {
                                // Check another thread has not created it before acquisition of the lock
                                result = subGrid.GetSubGrid(subGridCellX, subGridCellY);
                                if (result == null)
                                {
                                   result = CreateNewSubgrid(NumLevels);
                                   subGrid.SetSubGrid(subGridCellX, subGridCellY, result);
                                }
                            }
                        }
                        break;

                    case SubGridPathConstructionType.CreatePathToLeaf:
                        result = subGrid;
                        break;

                    case SubGridPathConstructionType.ReturnExistingLeafOnly:

                        // Get the local subgrid index in this subgrid that the leaf subgrid that
                        // contains the cell resides at
                        subGrid.GetSubGridCellIndex(cellX, cellY, out subGridCellX, out subGridCellY);

                        // Return any existing leaf
                        result = subGrid.GetSubGrid(subGridCellX, subGridCellY);
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// CountLeafSubgridsInMemory counts the number of leaf subgrids within the tree that currently reside in memory.
        /// </summary>
        /// <returns>The number of leaf subgrids in the tree</returns>
        public int CountLeafSubgridsInMemory()
        {
            int count = 0;

            return ScanAllSubGrids(subgrid => { count++; return true; }) ? count : -1;
        }

        /// <summary>
        /// LocateSubGridContaining attempts to locate a subgrid at the level in the tree
        /// given by Level that contains the on-the-ground cell identified by
        /// CellX and CellY
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public ISubGrid LocateSubGridContaining(uint cellX, uint cellY, byte level)
        {
            ISubGrid subGrid = Root;

            // Walk down the tree looking for the subgrid that contains the cell with the given X, Y location
            while (subGrid != null && subGrid.Level < level)
            {
                if (((INodeSubGrid)subGrid).GetSubGridContainingCell(cellX, cellY, out byte subGridCellX, out byte subGridCellY))
                    subGrid = subGrid.GetSubGrid(subGridCellX, subGridCellY);
                else
                    return null;
            }
            return subGrid;
        }

        /// <summary>
        /// LocateSubGridContaining attempts to locate a subgrid at the level in the tree,
        /// but defaults to looking at the bottom level CellX and CellY
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <returns></returns>
        public ISubGrid LocateSubGridContaining(uint cellX, uint cellY) => LocateSubGridContaining(cellX, cellY, NumLevels);

        /// <summary>
        // LocateClosestSubGridContaining behaves much like LocateSubGridContaining()
        // except that it walks as far through the tree as it can up to the designated
        // Level to find the requested cell, then returns that subgrid.
        // The returned node may be a leaf subgrid or a node subgrid
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public ISubGrid LocateClosestSubGridContaining(uint cellX, uint cellY, byte level)
        {
            // Walk down the tree looking for the subgrid that contains the cell
            // with the given X, Y location. Stop when we reach the leaf subgrid level,
            // or we can't descend any further through the tree, and return that subgrid.

            ISubGrid subGrid = Root;

            while (subGrid.Level < level)
            {
                if (((INodeSubGrid)subGrid).GetSubGridContainingCell(cellX, cellY, out byte subGridCellX, out byte subGridCellY))
                    subGrid = subGrid.GetSubGrid(subGridCellX, subGridCellY);
                else
                    return subGrid;
            }

            return subGrid;
        }

        /// <summary>
        /// GetCellCenterPosition conputes the real world location of the center
        /// of the on-the-ground cell identified by X and Y. X and Y are in the
        /// bottom left origin of the grid. The returned CX, CY values are translated
        /// to the centered origin of the real world coordinate system
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        public void GetCellCenterPosition(uint X, uint Y, out double cx, out double cy)
        {
            cx = (X - IndexOriginOffset) * CellSize + CellSizeOver2;
            cy = (Y - IndexOriginOffset) * CellSize + CellSizeOver2;
        }

        /// <summary>
        /// GetCellOriginPosition conputes the real world location of the origin
        /// of the on-the-ground cell identified by X and Y. X and Y are in the
        /// bottom left origin of the grid. The returned OX, OY values are translated
        /// to the centered origin of the real world coordinate system
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="ox"></param>
        /// <param name="oy"></param>
        public void GetCellOriginPosition(uint X, uint Y, out double ox, out double oy)
        {
            ox = (X - IndexOriginOffset) * CellSize;
            oy = (Y - IndexOriginOffset) * CellSize;
        }

        /// <summary>
        /// GetCellExtents computes the real world extents of the OTG cell identified
        /// by X and Y. X and Y are in the bottom left origin of the grid.
        /// The returned extents are translated to the centered origin of the real
        /// world coordinate system
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public BoundingWorldExtent3D GetCellExtents(uint X, uint Y)
        {
            double OriginX = (X - IndexOriginOffset) * CellSize;
            double OriginY = (Y - IndexOriginOffset) * CellSize;

            return new BoundingWorldExtent3D(OriginX, OriginY, OriginX + CellSize, OriginY + CellSize);
        }

      /// <summary>
      /// GetCellExtents computes the real world extents of the OTG cell identified
      /// by X and Y. X and Y are in the bottom left origin of the grid.
      /// The returned extents are translated to the centered origin of the real
      /// world coordinate system
      /// </summary>
      /// <param name="X"></param>
      /// <param name="Y"></param>
      /// <param name="extents"></param>
      /// <returns></returns>
      public void GetCellExtents(uint X, uint Y, ref BoundingWorldExtent3D extents)
      {
        double OriginX = (X - IndexOriginOffset) * CellSize;
        double OriginY = (Y - IndexOriginOffset) * CellSize;

        extents.MinX = OriginX;
        extents.MinY = OriginY;
        extents.MaxX = OriginX + CellSize;
        extents.MaxY = OriginY + CellSize;
      }

        /// <summary>
    // CreateUnattachedLeaf Creates an instance of a subgrid leaf node and returns
    // it to the caller. The newly created subgrid is _not_ attached to this grid.
    /// </summary>
    /// <returns></returns>
    public ILeafSubGrid CreateUnattachedLeaf() => CreateNewSubgrid(NumLevels) as ILeafSubGrid;

      public IEnumerator<ISubGrid> GetEnumerator()
      {
        return null;
      }

      public bool MoveNext()
      {
        throw new NotImplementedException();
      }

      public void Reset()
      {
        throw new NotImplementedException();
      }

      public ISubGrid Current { get; }

      object IEnumerator.Current => Current;

      public void Dispose()
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// The header string to be written into a serialised subgrid tree
      /// </summary>
      /// <returns></returns>
      public virtual string SerialisedHeaderName() => string.Empty;

      /// <summary>
      /// The header version to be written into a serialised subgrid tree
      /// </summary>
      /// <returns></returns>
      public virtual int SerialisedVersion() => 0;

      /// <summary>
      /// The internal logic to serialise out the content of the subgrid tree using the SubGridTreePersistor
      /// </summary>
      /// <param name="reader"></param>
      private void SerialiseInReader(BinaryReader reader) => SubGridTreePersistor.Read(this, SerialisedHeaderName(), SerialisedVersion(), reader);

      /// <summary>
      /// The internal logic to serialise in the content of the subgrid tree using the SubGridTreePersistor
      /// </summary>
      /// <param name="writer"></param>
      private void SerialiseOutWriter(BinaryWriter writer) => SubGridTreePersistor.Write(this, SerialisedHeaderName(), SerialisedVersion(), writer);

      /// <summary>
      /// Serialises the content of the subgrid tree to a byte array
      /// </summary>
      /// <returns></returns>
      public byte[] ToBytes() => FromToBytes.ToBytes(SerialiseOutWriter);

      /// <summary>
      /// Deserialises the content of the subgrid tree from a byte array
      /// </summary>
      /// <returns></returns>
      public void FromBytes(byte[] bytes) => FromToBytes.FromBytes(bytes, SerialiseInReader);

      /// <summary>
      /// Serialises the content of the subgrid tree to a memory stream
      /// </summary>
      /// <returns></returns>
      public MemoryStream ToStream() => FromToBytes.ToStream(SerialiseOutWriter);

      public void ToStream(Stream stream) => FromToBytes.ToStream(stream, SerialiseOutWriter);

      /// <summary>
      /// Deserialises the content of the subgrid tree from a memory stream
      /// </summary>
      /// <returns></returns>
      public void FromStream(MemoryStream stream) => FromToBytes.FromStream(stream, SerialiseInReader);
    }
}
