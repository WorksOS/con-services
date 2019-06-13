using System;
using System.IO;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Common.Utilities.ExtensionMethods;

/*
Glossary

1.	SubGrid : A structure that stores a square section of a much larger grid.
    The larger grid is an aggregate of the sub grids it contains.
2.	Cell : An entry in the list of references contained in a sub-grid. A cell
    may be a reference to another sub grid, a reference to a totally different
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
themselves, apart from cell X/Y location references used to locate sub grids.
*/

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// Base class for implementation of sub grid trees that defines various parameters and constants related to them
    /// </summary>
    public class SubGridTree : ISubGridTree
    {
        // private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridTree>();

        /****************************** Internal members **************************/

        /// <summary>
        /// The number of bits in the X and Y cell index keys that are used by the grid. The used bits are always the LSB bits.
        /// </summary>
        private byte NumBitsInKeys => (byte)(NumLevels * SubGridTreeConsts.SubGridIndexBitsPerLevel);

        /// <summary>
        /// The maximum (positive and negative) real world value for both X and Y axes that may be encompassed by the grid
        /// </summary>
        public double MaxOrdinate { get; private set; }

        /// <summary>
        /// The first (top) sub grid in the tree. All other sub grids in the tree descend via this root node.
        /// </summary>
        public INodeSubGrid Root { get; private set; }

        /// <summary>
        /// Internal numeric identifier for the sub grid tree. All internal operations will refer to the sub grid
        /// tree using this identifier. 
        /// </summary>
        public Guid ID { get; set; }

        private byte numLevels;
        /// <summary>
        /// The number of levels defined in this sub grid tree. 
        /// A 6 level tree typically defines leaf cells as relating to on-the-ground cell in the real world
        /// coordinate system (eg: cells tracking passes made by construction machines)
        /// A 5 level tree typically defines leaf cells that represent some aspect of the sub grids in the 
        /// 6th layer of the tree containing on-the-ground leaf cells (eg: sub grid existence map)
        /// This property is assignable only at the time the sub grid tree is constructed.
        /// </summary>
        public byte NumLevels { get => numLevels; }

        /// <summary>
        /// Backing store field for the CellSize property
        /// </summary>
        private double cellSize;

        /// <summary>
        /// Store the 'divide by 2' version of cell size to reduce compute in operations that use this quantity a lot
        /// </summary>
        public double CellSizeOver2;

        /// <summary>
        /// The real world size on the ground of a cell in the grid. This applies to tree with different numbers of levels.
        /// This property is assignable only at the time the sub grid tree is constructed.
        /// </summary>
        public double CellSize { get => cellSize; set => SetCellSize(value); }

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
        /// The sub grid factory to create sub grids for the sub grid tree
        /// </summary>
        private ISubGridFactory SubGridFactory { get; }

        // CreateNewSubGrid creates a new sub grid relevant to the requested level
        // in the tree. This new sub grid is not added into the tree structure -
        // it is unattached until explicitly inserted.
        public virtual ISubGrid CreateNewSubGrid(byte level) => SubGridFactory.GetSubGrid(this, level);

        private int indexOriginOffset;

        /// <summary>
        /// The value of the index origin offset for this sub grid tree
        /// </summary>
        public int IndexOriginOffset => indexOriginOffset; // { get; }

        /// <summary>
        /// Base Sub Grid Tree constructor. Creates a tree with the requested number of levels, 
        /// using the requested cell size for leaf cells and the supplied sub grid factory to create
        /// its leaf and node sub grids
        /// </summary>
        /// <param name="numLevels"></param>
        /// <param name="cellSize"></param>
        /// <param name="subGridFactory"></param>
        public SubGridTree(byte numLevels,
                           double cellSize,
                           ISubGridFactory subGridFactory)
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

            this.numLevels = numLevels;
            SetCellSize(cellSize);
            indexOriginOffset = 1 << (NumBitsInKeys - 1);

            SubGridFactory = subGridFactory ?? throw new ArgumentException("A sub grid factory must be specified", nameof(subGridFactory));

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
            Root = CreateNewSubGrid(1) as INodeSubGrid;
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
                                                                  int IndexOriginOffset,
                                                                  out int CellX, out int CellY)
        {
          // Calculate the cell index in a centered [0,0] origin basis
          // Convert the cell indexes into a bottom left [0,0] basis (ie: to grid cell
          // indexes in the top right quadrant).
            CellX = (int)Math.Floor(X / CellSize) + IndexOriginOffset;
            CellY = (int)Math.Floor(Y / CellSize) + IndexOriginOffset;
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
                                                           out int CellX, out int CellY)
        {
            if (Math.Abs(X) > MaxOrdinate || Math.Abs(Y) > MaxOrdinate)
            {
                CellX = int.MaxValue;
                CellY = int.MaxValue;

                return false;
            }

            CalculateIndexOfCellContainingPosition(X, Y, cellSize, indexOriginOffset, out CellX, out CellY);

            return true;
        }

        /// <summary>
        /// CalculateRegionGridCoverage determines the extent of on-the-ground grid cells that correspond to the given world extent.
        /// </summary>
        /// <param name="worldExtent"></param>
        /// <param name="cellExtent"></param>
        /// <returns></returns>
        public void CalculateRegionGridCoverage(BoundingWorldExtent3D worldExtent, out BoundingIntegerExtent2D cellExtent)
        {
            // First calculate the leaf data cell indexes for the given real world extent
            // If the world coordinates lie outside of the extent covered by the grid, then
            // clamp them to the boundaries of the grid.
            // Note: The clamping of the supplied world extent range means this method can never fail, always
            // returning a clamped cell extent to the underlying sub grid tree cell coordinate range
            CalculateIndexOfCellContainingPosition(
              Range.EnsureRange(worldExtent.MinX, -MaxOrdinate, MaxOrdinate),
              Range.EnsureRange(worldExtent.MinY, -MaxOrdinate, MaxOrdinate),
              out int minX, out int minY);
            CalculateIndexOfCellContainingPosition(
              Range.EnsureRange(worldExtent.MaxX, -MaxOrdinate, MaxOrdinate),
              Range.EnsureRange(worldExtent.MaxY, -MaxOrdinate, MaxOrdinate),
              out int maxX, out int maxY);

            cellExtent = new BoundingIntegerExtent2D((int) minX, (int) minY, (int) maxX, (int) maxY);
        }

        /// <summary>
        /// Returns the maximum world extent that this grid is capable of covering.
        /// </summary>
        /// <returns></returns>
        public BoundingWorldExtent3D FullGridExtent()
        {
            return new BoundingWorldExtent3D(-indexOriginOffset * cellSize,
                                             -indexOriginOffset * cellSize,
                                             indexOriginOffset * cellSize,
                                             indexOriginOffset * cellSize);
        }

        /// <summary>
        /// FullCellExtent returns the total extent of cells within this sub grid tree. 
        /// </summary>
        /// <returns></returns>
        public BoundingIntegerExtent2D FullCellExtent()
        {
            int numCellsAcrossGrid = 1 << (numLevels * SubGridTreeConsts.SubGridIndexBitsPerLevel);

            return new BoundingIntegerExtent2D(0, 0, numCellsAcrossGrid, numCellsAcrossGrid);
        }

        /// <summary>
        /// ScanSubGrids scans all sub grids at a requested level in the tree that
        /// intersect the given real world extent. Each sub grid that exists in the
        /// extent is passed to the OnProcessLeafSubGrid event for processing 
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
            CalculateRegionGridCoverage(extent, out BoundingIntegerExtent2D cellExtent);

            // Given the on-the-ground cell extents we can now ask the sub grids to recursively scan themselves.
            return Root.ScanSubGrids(cellExtent, leafFunctor, nodeFunctor);
        }

        /// <summary>
        /// ScanSubGrids scans all sub grids at a requested level in the tree that
        /// intersect the given cell address space extent. Each sub grid that exists in the
        /// extent is passed to the OnProcessLeafSubGrid event for processing 
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
        /// ScanAllSubGrids scans all sub grids. Each sub grid that exists in the
        /// extent is passed to the OnProcessLeafSubGrid event for processing 
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
        /// ConstructPathToCell constructs all necessary sub grids in all levels in
        /// the tree so that there is a path that can be traversed from the root of the
        /// tree to the leaf sub grid that will contain the cell identified by
        /// CellX and CellY. If PathType is pctCreateLeaf it returns the leaf
        /// sub grid instance into which the caller may place the cell data. If
        /// PathType is pctCreatePathToLeaf it returns the node sub grid instance that
        /// owns the leaf sub grid that contains the cell
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="pathType"></param>
        /// <returns></returns>
        public virtual ISubGrid ConstructPathToCell(int cellX, int cellY,
                                            SubGridPathConstructionType pathType)
        {
            ISubGrid result = null;
            INodeSubGrid subGrid = Root;

            // First descend through the node levels of the tree
            for (byte I = 1; I < numLevels - 1; I++) // Yes, -1 because we choose a sub grid cell from the next level down...
            {
                var testSubGrid = subGrid.GetSubGridContainingCell(cellX, cellY);

                if (testSubGrid != null)
                {
                    // Walk into this cell in the sub grid as the next level down in this path exists
                    subGrid = testSubGrid as INodeSubGrid;
                }
                else
                {
                    if (pathType == SubGridPathConstructionType.ReturnExistingLeafOnly)
                    {
                        // We do not need to descend further - the leaf sub grid we are interested in does not exist.
                        return null;
                    }

                    INodeSubGrid lockSubGrid = subGrid;
                    // Obtain a lock on the node sub grid to create and add the new child node sub grid to it
                    lock (lockSubGrid)
                    {
                        // Check to see if another thread was able to create the sub grid before the
                        // lock was obtained. If so just return it
                        testSubGrid = subGrid.GetSubGridContainingCell(cellX, cellY);

                        if (testSubGrid != null)
                        {
                            // Walk into this cell in the sub grid as the next level down in this path exists
                            subGrid = testSubGrid as INodeSubGrid;
                        }
                        else
                        {
                            // We need to create a new sub grid
                            ISubGrid newSubGrid = CreateNewSubGrid((byte) (I + 1));

                            // ... and add it into this sub grid as the sub grid cell coordinates
                            // returned by GetSubGridContainingCell ...
                             subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridCellX, out byte subGridCellY);
                             subGrid.SetSubGrid(subGridCellX, subGridCellY, newSubGrid);

                            // ... then carry on descending into it
                            subGrid = newSubGrid as INodeSubGrid;
                        }
                    }
                }
            }

            // Now check to see if the cell in the node level directly above the leaf
            // level contains a reference to the appropriate leaf sub grid
            if (subGrid != null)
            {
                // If requested, create the new leaf sub grid, otherwise return the
                // node sub grid stored in SubGrid to the caller
                switch (pathType)
                {
                    case SubGridPathConstructionType.CreateLeaf:
                        // Get the local sub grid index in this sub grid that the leaf sub grid that
                        // contains the cell resides at
                        subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridCellX, out byte subGridCellY);

                        // Check if the leaf sub grid already exists, if not then create it.
                        result = subGrid.GetSubGrid(subGridCellX, subGridCellY);

                        if (result == null)
                        {
                            // Lock the node sub grid model while creating the new child leaf sub grid within it.
                            lock (subGrid)
                            {
                                // Check another thread has not created it before acquisition of the lock
                                result = subGrid.GetSubGrid(subGridCellX, subGridCellY);
                                if (result == null)
                                {
                                   result = CreateNewSubGrid(numLevels);
                                   subGrid.SetSubGrid(subGridCellX, subGridCellY, result);
                                }
                            }
                        }
                        break;

                    case SubGridPathConstructionType.CreatePathToLeaf:
                        result = subGrid;
                        break;

                    case SubGridPathConstructionType.ReturnExistingLeafOnly:

                        // Get the local sub grid index in this sub grid that the leaf sub grid that
                        // contains the cell resides at
                        subGrid.GetSubGridCellIndex(cellX, cellY, out subGridCellX, out subGridCellY);

                        // Return any existing leaf
                        result = subGrid.GetSubGrid(subGridCellX, subGridCellY);
                        break;

                    default:
                      throw new TRexSubGridTreeException($"Unknown SubGridPathConstructionType: {pathType}");
                }
            }

            return result;
        }

        /// <summary>
        /// CountLeafSubGridsInMemory counts the number of leaf sub grids within the tree that currently reside in memory.
        /// </summary>
        /// <returns>The number of leaf sub grids in the tree</returns>
        public int CountLeafSubGridsInMemory()
        {
            int count = 0;

            return ScanAllSubGrids(subGrid => { count++; return true; }) ? count : -1;
        }

        /// <summary>
        /// LocateSubGridContaining attempts to locate a sub grid at the level in the tree
        /// given by Level that contains the on-the-ground cell identified by
        /// CellX and CellY
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public ISubGrid LocateSubGridContaining(int cellX, int cellY, byte level)
        {
            ISubGrid subGrid = Root;

            // Walk down the tree looking for the sub grid that contains the cell with the given X, Y location
            while (subGrid != null && subGrid.Level < level)
            {
                subGrid = ((INodeSubGrid) subGrid).GetSubGridContainingCell(cellX, cellY);
            }
            return subGrid;
        }

        /// <summary>
        /// LocateSubGridContaining attempts to locate a sub grid at the level in the tree,
        /// but defaults to looking at the bottom level CellX and CellY
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <returns></returns>
        public ISubGrid LocateSubGridContaining(int cellX, int cellY) => LocateSubGridContaining(cellX, cellY, numLevels);

        /// <summary>
        /// LocateClosestSubGridContaining behaves much like LocateSubGridContaining()
        /// except that it walks as far through the tree as it can up to the designated
        /// Level to find the requested cell, then returns that sub grid.
        /// The returned node may be a leaf sub grid or a node sub grid
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public ISubGrid LocateClosestSubGridContaining(int cellX, int cellY, byte level)
        {
            // Walk down the tree looking for the sub grid that contains the cell
            // with the given X, Y location. Stop when we reach the leaf sub grid level,
            // or we can't descend any further through the tree, and return that sub grid.

            ISubGrid subGrid = Root;

            while (subGrid.Level < level)
            {
                var testSubGrid = ((INodeSubGrid)subGrid).GetSubGridContainingCell(cellX, cellY);
                if (testSubGrid != null)
                    subGrid = testSubGrid;
                else
                    return subGrid;
            }

            return subGrid;
        }

        /// <summary>
        /// GetCellCenterPosition computes the real world location of the center
        /// of the on-the-ground cell identified by X and Y. X and Y are in the
        /// bottom left origin of the grid. The returned CX, CY values are translated
        /// to the centered origin of the real world coordinate system
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        public void GetCellCenterPosition(int X, int Y, out double cx, out double cy)
        {
            cx = (X - indexOriginOffset) * cellSize + CellSizeOver2;
            cy = (Y - indexOriginOffset) * cellSize + CellSizeOver2;
        }

        /// <summary>
        /// GetCellOriginPosition computes the real world location of the origin
        /// of the on-the-ground cell identified by X and Y. X and Y are in the
        /// bottom left origin of the grid. The returned OX, OY values are translated
        /// to the centered origin of the real world coordinate system
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="ox"></param>
        /// <param name="oy"></param>
        public void GetCellOriginPosition(int X, int Y, out double ox, out double oy)
        {
            ox = (X - indexOriginOffset) * cellSize;
            oy = (Y - indexOriginOffset) * cellSize;
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
        public BoundingWorldExtent3D GetCellExtents(int X, int Y)
        {
            double OriginX = (X - indexOriginOffset) * cellSize;
            double OriginY = (Y - indexOriginOffset) * cellSize;

            return new BoundingWorldExtent3D(OriginX, OriginY, OriginX + cellSize, OriginY + cellSize);
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
      public void GetCellExtents(int X, int Y, ref BoundingWorldExtent3D extents)
      {
        double OriginX = (X - indexOriginOffset) * cellSize;
        double OriginY = (Y - indexOriginOffset) * cellSize;

        extents.MinX = OriginX;
        extents.MinY = OriginY;
        extents.MaxX = OriginX + cellSize;
        extents.MaxY = OriginY + cellSize;
      }

      /// <summary>
      /// CreateUnattachedLeaf Creates an instance of a sub grid leaf node and returns
      /// it to the caller. The newly created sub grid is _not_ attached to this grid.
      /// </summary>
      /// <returns></returns>
      public ILeafSubGrid CreateUnattachedLeaf() => CreateNewSubGrid(numLevels) as ILeafSubGrid;

      /// <summary>
      /// The header string to be written into a serialized sub grid tree
      /// </summary>
      /// <returns></returns>
      public virtual string SerialisedHeaderName() => string.Empty;

      /// <summary>
      /// The header version to be written into a serialized sub grid tree
      /// </summary>
      /// <returns></returns>
      public virtual int SerialisedVersion() => 0;

      /// <summary>
      /// The internal logic to serialise out the content of the sub grid tree using the SubGridTreePersistor
      /// </summary>
      /// <param name="reader"></param>
      private void SerialiseInReader(BinaryReader reader) => SubGridTreePersistor.Read(this, SerialisedHeaderName(), SerialisedVersion(), reader);

      /// <summary>
      /// The internal logic to serialise in the content of the sub grid tree using the SubGridTreePersistor
      /// </summary>
      /// <param name="writer"></param>
      private void SerialiseOutWriter(BinaryWriter writer) => SubGridTreePersistor.Write(this, SerialisedHeaderName(), SerialisedVersion(), writer);

      /// <summary>
      /// Serializes the content of the sub grid tree to a byte array
      /// </summary>
      /// <returns></returns>
      public byte[] ToBytes() => FromToBytes.ToBytes(SerialiseOutWriter);

      /// <summary>
      /// Deserializes the content of the sub grid tree from a byte array
      /// </summary>
      /// <returns></returns>
      public void FromBytes(byte[] bytes) => FromToBytes.FromBytes(bytes, SerialiseInReader);

      /// <summary>
      /// Serializes the content of the sub grid tree to a memory stream
      /// </summary>
      /// <returns></returns>
      public MemoryStream ToStream() => FromToBytes.ToStream(SerialiseOutWriter);

      public void ToStream(Stream stream) => FromToBytes.ToStream(stream, SerialiseOutWriter);

      /// <summary>
      /// Deserializes the content of the sub grid tree from a memory stream
      /// </summary>
      /// <returns></returns>
      public void FromStream(MemoryStream stream) => FromToBytes.FromStream(stream, SerialiseInReader);
    }
}
