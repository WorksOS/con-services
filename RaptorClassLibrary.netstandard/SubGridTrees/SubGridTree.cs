using System;
using System.Diagnostics;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Types;
using VSS.VisionLink.Raptor.Utilities;

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
acess mechanisms chiefly revolve around sub-grids, rather than actual cells
themselves, apart from cell X/Y location references used to locate subgrids.
*/

namespace VSS.VisionLink.Raptor
{
    /// <summary>
    /// Base class for implementation of sub grid trees that defines various parameters and constants related to them
    /// </summary>
    public class SubGridTree : ISubGridTree
    {
        /// <summary>
        /// We use 6 levels in our subgrid tree (Root + 4 intermediary + 1 on-the-ground)
        /// </summary>
        public const byte SubGridTreeLevels = 6;

        /// <summary>
        /// The default cell size used when creating a new site model
        /// </summary>
        public const double DefaultCellSize = 0.34; // Units: meters

        /// <summary>
        /// Denotes how many bits of an on-the-ground cell X or Y index reference each level in the subgrid tree represents
        /// </summary>
        public const byte SubGridIndexBitsPerLevel = 5;

        /// <summary>
        /// The number of cells that are in a sub grid X and Y dimension (square grid)
        /// </summary>
        public const byte SubGridTreeDimension = 1 << SubGridIndexBitsPerLevel;

        /// <summary>
        /// The number of cells that are in a sub grid X and Y dimension (square grid), minus 1
        /// </summary>
        public const byte SubGridTreeDimensionMinus1 = (1 << SubGridIndexBitsPerLevel) - 1;

        /// <summary>
        /// The number of cells contained within a subgrid
        /// </summary>
        public const int SubGridTreeCellsPerSubgrid = SubGridTreeDimension * SubGridTreeDimension;

        /// <summary>
        // The number of cells on-the-ground a single cell in the
        // root a sub grid tree node spans in the X and Y dimensions. Each level down
        // will represent a smaller fraction of these on-the-ground cells given by
        // (1/ kSubGridTreeDimension)
        /// </summary>
        public const uint RootSubGridCellSize = 1 << ((SubGridTreeLevels - 1) * SubGridIndexBitsPerLevel);

        /// <summary>
        /// The mask required to extract the portion of a subgrid key relating to a single level in the tree
        /// This is 0b00011111 (ie: five bits of an X/Y component of the key)
        /// </summary>
        public const byte SubGridLocalKeyMask = 0x1F;

        /// <summary>
        /// The number of cells (representing node subgrids, leaf subgrids or on-the-ground cells) that
        /// are represented within a subgrid
        /// </summary>
        public const uint CellsPerSubgrid = SubGridTreeDimension * SubGridTreeDimension;

        /****************************** Internal members **************************/

        /// <summary>
        /// The number of bits in the X and Y cell index keys that are used by the grid. The used bits are always the LSB bits.
        /// </summary>
        private byte NumBitsInKeys => (byte)(NumLevels * SubGridIndexBitsPerLevel);

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
        public long ID { get; set; }

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
        /// The default cell index offset to translate the
        /// centered world coordinate origin into the bottom left grid origin
        /// This may also be thought of as the distance in on-the-ground cells from the
        /// bottom left hand corner (origin) of the grid, to the cell whose bottom
        /// left hand corner is at the exact center of the grid. This offset is
        /// applied to both the X and Y dimensions of the grid.
        /// </summary>
        public static uint DefaultIndexOriginOffset => 1 << ((SubGridTreeLevels * SubGridIndexBitsPerLevel) - 1);

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
            if (numLevels < 1 || numLevels > SubGridTreeLevels)
            {
                // Invalid number of tree levels
                throw new ArgumentException(string.Format("Number of levels must be between 1 and {0}", SubGridTreeLevels), "numLevels");
            }

            if (cellSize < 0.01 || cellSize > 1000000)
            {
                // Invalid cell size
                throw new ArgumentException("CellSize must be between 0.01 and 1000000", "cellSize");
            }

            NumLevels = numLevels;
            SetCellSize(cellSize);
            IndexOriginOffset = (uint)1 << (NumBitsInKeys - 1);

            SubgridFactory = subGridfactory ?? throw new ArgumentException("A subgrid factory must be specified", "subGridfactory");

            // Construct the root node for the tree
            InitialiseRoot();

            // Not implementing memory size tracking for now
            // FTotalInMemorySize = 0;
        }

        /// <summary>
        /// Base Sub Grid Tree constructor. Creates a tree with the requested numner of levels, 
        /// using the requested cell size for leaf cells and th esupplied subgrid factory to create
        /// its leaf and node subgrids.
        /// persistedStreamHeader and persistedStreamVersion are used to annotate persistent serialisations
        /// of the information held in the tree.
        /// </summary>
        /// <param name="numLevels"></param>
        /// <param name="cellSize"></param>
        /// <param name="subGridfactory"></param>
        /// <param name="persistedStreamHeader"></param>
        /// <param name="persistedStreamVersion"></param>
        public SubGridTree(byte numLevels,
                           double cellSize,
                           ISubGridFactory subGridfactory,
                           string persistedStreamHeader,
                           int persistedStreamVersion) : this(numLevels, cellSize, subGridfactory)
        {
            //            PersistedStreamHeader = persistedStreamHeader;
            //            PersistedStreamVersion = persistedStreamVersion;
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
            CellX = (uint)Math.Floor(X / CellSize);
            CellY = (uint)Math.Floor(Y / CellSize);

            // Convert the cell indexes into a bottom left [0,0] basis (ie: to grid cell
            // indexes in the top right quadrant).

            CellX += IndexOriginOffset;
            CellY += IndexOriginOffset;
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
            if ((Math.Abs(X) > MaxOrdinate) || (Math.Abs(Y) > MaxOrdinate))
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
                if (CalculateIndexOfCellContainingPosition(Range.EnsureRange(worldExtent.MinX, -MaxOrdinate, MaxOrdinate),
                                                         Range.EnsureRange(worldExtent.MinY, -MaxOrdinate, MaxOrdinate),
                                                         out uint minX,
                                                         out uint minY) &&
                         CalculateIndexOfCellContainingPosition(Range.EnsureRange(worldExtent.MaxX, -MaxOrdinate, MaxOrdinate),
                                                      Range.EnsureRange(worldExtent.MaxY, -MaxOrdinate, MaxOrdinate),
                                                      out uint maxX,
                                                      out uint maxY))
                {
                    cellExtent = new BoundingIntegerExtent2D((int)minX, (int)minY, (int)maxX, (int)maxY);
                    return true;
                }
                else
                {
                    cellExtent = new BoundingIntegerExtent2D();
                    return false;
                }
            }
            catch (Exception)
            {
                throw; // For now, just throw it until we have logging sorted out
                // SIGLogMessage.PublishNoODS(Self, Format('Exception in SubGridTree.CalculateRegionGridCoverage: %s', [E.Message]), slmcDebug);
                // Something bad happened...
                //                return false;
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
            int numCellsAcrossGrid = 1 << (NumLevels * SubGridIndexBitsPerLevel);

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
                // TODO: Reinstate logging once it is set up
                // The extents requested lie at least partially ouside the tree. Ignore this request.
                //  SIGLogMessage.PublishNoODS(Self, Format('TSubGridTree.ScanSubGrids could not convert %s in a cell extent',
                //                                                    [Extent.AsText]), slmcWarning);
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
            ISubGrid newSubGrid;

            ISubGrid result = null;
            INodeSubGrid subGrid = Root;

            // First descend through the node levels of the tree
            for (byte I = 1; I < NumLevels - 1; I++) // Yes, -1 because we choose a subgrid cell from the next level down...
            {
                if (subGrid.GetSubGridContainingCell(cellX, cellY,
                                                     out subGridCellX, out subGridCellY))
                {
                    // Walk into this cell in the subgrid as the next level down in this
                    // path exists
                    subGrid = subGrid.GetSubGrid(subGridCellX, subGridCellY) as INodeSubGrid;
                }
                else
                {
                    if (pathType == SubGridPathConstructionType.ReturnExistingLeafOnly)
                    {
                        // We do not need to descend further - the leaf subgrid we are interested in does not exist.
                        break;
                    }

                    // We need to create a new subgrid
                    newSubGrid = CreateNewSubgrid((byte)(I + 1));

                    // ... and add it into this subgrid as the subgrid cell coordinates
                    // returned by GetSubGridContainingCell ...
                    subGrid.SetSubGrid(subGridCellX, subGridCellY, newSubGrid);

                    // ... then carry on descending into it
                    subGrid = newSubGrid as INodeSubGrid;
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
                        //Debug.Assert(subGrid is INodeSubGrid);

                        // Get the local subgrid index in this subgrid that the leaf subgrid that
                        // contains the cell resides at
                        subGrid.GetSubGridCellIndex(cellX, cellY, out subGridCellX, out subGridCellY);

                        // Check if the leaf subgrid already exists, if not then create it.
                        result = subGrid.GetSubGrid(subGridCellX, subGridCellY);

                        if (result == null)
                        {
                            result = CreateNewSubgrid(NumLevels);
                            subGrid.SetSubGrid(subGridCellX, subGridCellY, result);
                        }
                        break;

                    case SubGridPathConstructionType.CreatePathToLeaf:
                        result = subGrid;
                        break;

                    case SubGridPathConstructionType.ReturnExistingLeafOnly:
                        // Debug.Assert(subGrid is INodeSubGrid);

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
                {
                    subGrid = subGrid.GetSubGrid(subGridCellX, subGridCellY);
                }
                else
                {
                    return null;
                }
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
                //with TSubGridTreeNodeSubGrid(Result) do
                if (((INodeSubGrid)subGrid).GetSubGridContainingCell(cellX, cellY, out byte subGridCellX, out byte subGridCellY))
                {
                    subGrid = subGrid.GetSubGrid(subGridCellX, subGridCellY);
                }
                else
                {
                    return subGrid;
                }
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
        /// GetCellExtentsconputes the real world extents of the OTG cell identified
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
        // CreateUnattachedLeaf Creates an instance of a subgrid leaf node and returns
        // it to the caller. The newly created subgrid is _not_ attached to this grid.
        /// </summary>
        /// <returns></returns>
        public ILeafSubGrid CreateUnattachedLeaf() => CreateNewSubgrid(NumLevels) as ILeafSubGrid;
    }
}
