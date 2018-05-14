using System;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SubGridTrees.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    public interface ISubGridTree
    {
        /// <summary>
        /// Internal numeric identifier for the sub grid tree. All internal operations will refer to the sub grid
        /// tree using this identitifer. 
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// External identifier (GUID) for the subgrid tree. The instance may be tagged with this ID as an 
        /// association to the primary numeric identifier.
        /// </summary>
        Guid ExternalID { get; set; }

        /// <summary>
        /// The number of levels defined in this subgrid tree. 
        /// A 6 level tree typically defines leaf cells as relating to on-the-ground cell in the real world
        /// coordinate system (eg: cells tracking passes made by construction machines)
        /// A 5 level tree typically defines leaf cells that represent some aspect of the subgrids in the 
        /// 6th layer of the tree containing on-the-groun leaf cells (eg: subgrid existence map)
        /// This property is assignable only at the time the subgrid tree is constructed.
        /// </summary>
        byte NumLevels { get; }

        /// <summary>
        /// The real world size on the ground of a cell in the grid. This applies to tree with different numbers of levels.
        /// This property is mutable at any time as it does not modify any internal storage concerns, but it will change the 
        /// calculated answers to queries as CellSize relates the spread of cells across the real world coordinate system the
        /// data stored in the sub grid tree was collected.
        /// </summary>
        double CellSize { get; set; }

        /// <summary>
        /// The value of the index origin offset for this sub grid tree
        /// </summary>
        uint IndexOriginOffset { get; }

        /// <summary>
        /// Root is the top level subgrid in a subgrid tree. All other subgrids are children or descendents from
        /// this node. Root is an INodeSubGrid interface, a descendent from ISubGrid. Root is automatically created when the SubGridTree is created.
        /// </summary>
        INodeSubGrid Root { get; set; }

        /// <summary>
        /// Clears all content from the subgrid tree and resets the root node to empty
        /// </summary>
        void Clear();

        /// <summary>
        /// ScanSubgrids scans all subgrids at a requested level in the tree that
        /// intersect the given real world extent. Each subgrid that exists in the
        /// extent is passed to the OnProcessLeafSubgrid event for processing 
        /// </summary>
        /// <param name="extent"></param>
        /// <param name="leafFunctor"></param>
        /// <param name="nodeFunctor"></param>
        /// <returns></returns>
        bool ScanSubGrids(BoundingWorldExtent3D extent,
                                 Func<ISubGrid, bool> leafFunctor = null,
                                 Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

        /// <summary>
        /// ScanSubgrids scans all subgrids at a requested level in the tree that
        /// intersect the given cell address space extent. Each subgrid that exists in the
        /// extent is passed to the OnProcessLeafSubgrid event for processing 
        /// </summary>
        /// <param name="extent"></param>
        /// <param name="leafFunctor"></param>
        /// <param name="nodeFunctor"></param>
        /// <returns></returns>
        bool ScanSubGrids(BoundingIntegerExtent2D extent,
                          Func<ISubGrid, bool> leafFunctor = null,
                          Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

        /// <summary>
        /// ScanSubgrids scans all subgrids. Each subgrid that exists in the
        /// extent is passed to the OnProcessLeafSubgrid event for processing 
        /// </summary>
        /// <param name="leafFunctor"></param>
        /// <param name="nodeFunctor"></param>
        /// <returns></returns>
        bool ScanAllSubGrids(Func<ISubGrid, bool> leafFunctor = null,
                             Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

        /// <summary>
        /// CountLeafSubgridsInMemory counts the number of leaf subgrids within the tree that currently reside in memory.
        /// </summary>
        /// <returns>The number of leaf subgrids in the tree</returns>
        int CountLeafSubgridsInMemory();

        /// <summary>
        /// FullGridExtent returns the maximum world extent that this grid is capable of covering.
        /// </summary>
        /// <returns></returns>
        BoundingWorldExtent3D FullGridExtent();

        /// <summary>
        /// FullCellExtent returns the total extent of cells within this subgridtree. 
        /// </summary>
        /// <returns></returns>
        BoundingIntegerExtent2D FullCellExtent();

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
        ISubGrid ConstructPathToCell(uint cellX, uint cellY, SubGridPathConstructionType pathType);

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
        bool CalculateIndexOfCellContainingPosition(double X, double Y,
                                                    out uint CellX, out uint CellY);

        /// <summary>
        /// LocateSubGridContaining attempts to locate a subgrid at the level in the tree
        /// given by Level that contains the on-the-ground cell identified by
        /// CellX and CellY
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        ISubGrid LocateSubGridContaining(uint cellX, uint cellY, byte level);

        /// <summary>
        /// LocateSubGridContaining attempts to locate a subgrid at the level in the tree,
        /// but defaults to looking at the bottom lavel
        /// CellX and CellY
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <returns></returns>
        ISubGrid LocateSubGridContaining(uint cellX, uint cellY);

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
        ISubGrid LocateClosestSubGridContaining(uint cellX, uint cellY, byte level);

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
        void GetCellCenterPosition(uint X, uint Y, out double cx, out double cy);

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
        void GetCellOriginPosition(uint X, uint Y, out double ox, out double oy);

        /// <summary>
        /// GetCellExtentsconputes the real world extents of the OTG cell identified
        /// by X and Y. X and Y are in the bottom left origin of the grid.
        /// The returned extents are translated to the centered origin of the real
        /// world coordinate system
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        BoundingWorldExtent3D GetCellExtents(uint X, uint Y);

        /// <summary>
        // CreateUnattachedLeaf Creates an instance of a subgrid leaf node and returns
        // it to the caller. The newly created subgrid is _not_ attached to this grid.
        /// </summary>
        /// <returns></returns>
        ILeafSubGrid CreateUnattachedLeaf();

        /// <summary>
        /// CalculateRegionGridCoverage determines the extent of on-the-ground grid cells that correspond to the given world extent.
        /// </summary>
        /// <param name="worldExtent"></param>
        /// <param name="cellExtent"></param>
        /// <returns></returns>
        bool CalculateRegionGridCoverage(BoundingWorldExtent3D worldExtent, out BoundingIntegerExtent2D cellExtent);
    }
}
