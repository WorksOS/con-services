using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
    /// <summary>
    /// ClientLeafSubGrid is a local base class for sub grid tree
    /// leaf subgrid derivatives. This class defines support for assigning filtered
    /// values to cells in the grid, and also adds a cache map to it. The cache
    /// map records which cells in the subgrid contain information that has been
    /// retrieved from the server.
    /// </summary>
    public abstract class ClientLeafSubGrid : SubGrid, IClientLeafSubGrid
    {
        /// <summary>
        /// Enumeration indicating type of grid data held in this client leaf sub grid
        /// </summary>
        protected GridDataType _gridDataType;

        /// <summary>
        /// Enumeration indicating type of grid data held in this client leaf sub grid
        /// </summary>
        public GridDataType GridDataType { get { return _gridDataType; } } 

        /// <summary>
        /// Cellsize is a copy of the cell size from the parent subgrid. It is replicated here
        /// to remove SubGridTree binding in other processing contexts
        /// </summary>
        public double CellSize { get; set; }

        /// <summary>
        /// IndexOriginOffset is a copy of the IndexOriginOffset from the parent subgrid. It is replicated here
        ///to remove SubGridTree binding in other processing contexts
        /// </summary>
        public uint IndexOriginOffset { get; set; }

        /// <summary>
        /// Is data extraction limited to the top identified layer of materials in each cell
        /// </summary>
        public bool TopLayerOnly { get; set; }

        public abstract void FillWithTestPattern();

        public abstract bool LeafContentEquals(IClientLeafSubGrid other);

        /// <summary>
        /// The requested display mode driving the request of these subgrids of data
        /// </summary>
        public DisplayMode ProfileDisplayMode { get; set; }

        /// <summary>
        /// A map of flags indicating which grid data types are supported by the intermediary subgrid result cache
        /// </summary>
        private static bool[] SupportsAssignationFromCachedPreProcessedClientSubgrid = // GridDataType
        {
          false, // All = $00000000;
          true,  // CCV = $00000001;
          false, // Height = $00000002;
          false, // Latency = $00000003;
          true,  // PassCount = $00000004;
          false, // Frequency = $00000005;
          false, // Amplitude = $00000006;
          false, // Moisture = $00000007;
          true,  // Temperature = $00000008;
          false, // RMV = $00000009;
          true,  // CCVPercent = $0000000B;
          false, // GPSMode = $0000000A;
          false, // SimpleVolumeOverlay = $0000000C;
          false, // HeightAndTime = $0000000D;
          false, // CompositeHeights = $0000000E;
          true,  // MDP = $0000000F;
          true,  // MDPPercent = $00000010;
          false, //  CellProfile = $00000011;
          false, //   CellPasses = $00000012;
          false, //   MachineSpeed = $00000013;
          false, //  CCVPercentChange = $00000014;
          false, //  MachineSpeedTarget = $00000015;
          false, // CCVPercentChangeIgnoredTopNullValue = $0000016
          true,  // CCA = $0000017
          true   // CCAPerccent = = $0000018
        };

        /// <summary>
        /// Existence map of where we know Prod Data exists 
        /// </summary>
        public SubGridTreeBitmapSubGridBits ProdDataMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

        /// <summary>
        /// Existence map of cells matching current filter settings
        /// </summary>
        public SubGridTreeBitmapSubGridBits FilterMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

        /// <summary>
        /// Constructor the the base client subgrid. This decorates the standard (owner, parent, level)
        /// constructor from the base with the cellsize and indexoriginoffset parameters from the subgridtree
        /// this leaf is derived from.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        /// <param name="cellSize"></param>
        /// <param name="indexOriginOffset"></param>
        public ClientLeafSubGrid(ISubGridTree owner,
            ISubGrid parent,
            byte level,
            double cellSize,
            uint indexOriginOffset) : base(owner, parent, level)
        {
            CellSize = cellSize;
            IndexOriginOffset = indexOriginOffset;

            _gridDataType = GridDataType.All; // Default to 'all', descendant specialized classes will set appropriately

            TopLayerOnly = false;
            ProfileDisplayMode = DisplayMode.Height;
        }

        /// <summary>
        /// Assign the result of filtering a cell (based on filtering and other criteria) into a cell in this client leaf subgrid
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="Context"></param>
        public virtual void AssignFilteredValue(byte CellX, byte CellY, FilteredValueAssignmentContext Context)
        {
            Debug.Assert(false, "{0}.AssignFilteredValue may not be called directly. No need to assign value for entire cell pass", MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        /// <summary>
        /// Determine if the value propsed for assignation to a cell in this client leaf subgrid is null with respect
        /// to the nullability criteria of that client leaf subgrid
        /// </summary>
        /// <param name="filteredValue"></param>
        /// <returns></returns>
        public virtual bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue)
        {
            Debug.Assert(false, "{0}AssignableFilteredValueIsNull may not be called directly. Not valid to check nullness against entire cell pass", MethodBase.GetCurrentMethod().DeclaringType.Name);
            return false;
        }

        /// <summary>
        /// The set of population control flags this client wants enabled in the course of servicing requests
        /// </summary>
        public PopulationControlFlags EventPopulationFlags { get; set; } = PopulationControlFlags.None;

        public virtual bool WantsLiftProcessingResults() => false;

        /// <summary>
        /// Calculate the world origin coordinate location for this client leaf sub grid.
        /// This uses the local cell size and index orifin offset information to perform the 
        /// calculation locally without the need for a reference sub grid tree.
        /// </summary>
        /// <param name="WorldOriginX"></param>
        /// <param name="WorldOriginY"></param>
        public override void CalculateWorldOrigin(out double WorldOriginX,
                                                  out double WorldOriginY)
        {
            WorldOriginX = (OriginX - IndexOriginOffset) * CellSize;
            WorldOriginY = (OriginY - IndexOriginOffset) * CellSize;
        }

        /// <summary>
        /// Calculate the world coordinate extents for this client leaf sub grid.
        /// This uses the local cell size and index orifin offset information to perform the 
        /// calculation locally without the need for a reference sub grid tree.
        /// </summary>
        /// <returns></returns>
        public BoundingWorldExtent3D CalculateWorldExtent()
        {
            double WOx = (OriginX - IndexOriginOffset) * CellSize;
            double WOy = (OriginY - IndexOriginOffset) * CellSize;

            return new BoundingWorldExtent3D(WOx, WOy,
                                             WOx + SubGridTreeConsts.SubGridTreeDimension * CellSize,
                                             WOy + SubGridTreeConsts.SubGridTreeDimension * CellSize);
        }

        /// <summary>
        /// Assign cell information from a previously cached result held in the general subgrid result cache
        /// using the supplied map to control which cells from the caches subgrid should be copied into this
        /// client leaf sub grid
        /// </summary>
        /// <param name="source"></param>
        /// <param name="map"></param>
        public virtual void AssignFromCachedPreProcessedClientSubgrid(ISubGrid source,
                                                                      SubGridTreeBitmapSubGridBits map)
        {
            Debug.Assert(false, "{0}.AssignFromCachedPreProcessedClientSubgrid does not support assignation", MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        /// <summary>
        /// Assigns the state of one client leaf sub grid to this client leaf subgrid
        /// Note: The cell values are explicitly NOT copied in this operation
        /// </summary>
        /// <param name="source"></param>
        public void Assign(ClientLeafSubGrid source)
        {
            Level = source.Level;
            OriginX = source.OriginX;
            OriginY = source.OriginY;

            // Grid data type is never assigned from one client grid to another...
            //GridDataType = source.GridDataType;

            CellSize = source.CellSize;
            IndexOriginOffset = source.IndexOriginOffset;
            ProdDataMap.Assign(source.ProdDataMap);
            FilterMap.Assign(source.FilterMap);
        }

        /// <summary>
        /// Dumps the contents of this client leaf subgrid into the log in a human readable form
        /// </summary>
        /// <param name="title"></param>
        public virtual void DumpToLog(string title)
        {
            throw new NotImplementedException("TICSubGridTreeLeafSubGridBase.DumpToLog not implemented in " + GetType().Name);
        }

/*
        /// <summary>
        /// Write the contents of leaf sub grid using the supplied formatter
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public override void Write(BinaryFormatter formatter, Stream stream)
        {
            formatter.Serialize(stream, OriginX);
            formatter.Serialize(stream, OriginY);
            formatter.Serialize(stream, Level);
            formatter.Serialize(stream, GridDataType);
            formatter.Serialize(stream, CellSize);
            formatter.Serialize(stream, IndexOriginOffset);

            // Construct the map representing those cells that contain values that
            // should be serialised
            // Unsure if this is needed (current gen used it to control which values were written into the stream)
            ProdDataMap.ForEach(CellHasValue);

            // Write the map to the stream for deserialisation
            formatter.Serialize(stream, ProdDataMap);
            formatter.Serialize(stream, FilterMap);
        }

        /// <summary>
        /// Fill the contents of the leaf sub grid reading the binary representation using the provided formatter
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public override void Read(BinaryFormatter formatter, Stream stream)
        {
            OriginX = (uint)formatter.Deserialize(stream);
            OriginY = (uint)formatter.Deserialize(stream);
            Level = (byte)formatter.Deserialize(stream);

            if ((GridDataType)formatter.Deserialize(stream) != GridDataType)
            {
                Debug.Assert(false, "GridDataType in stream does not match GridDataType of local subgrid instance");
            }

            CellSize = (double)formatter.Deserialize(stream);
            IndexOriginOffset = (uint)formatter.Deserialize(stream);

            ProdDataMap = (SubGridTreeBitmapSubGridBits)formatter.Deserialize(stream);
            FilterMap = (SubGridTreeBitmapSubGridBits)formatter.Deserialize(stream);
        }
*/

        // procedure WriteToStream(const Stream: TStream); override;
        // procedure ReadFromStream(const Stream: TStream); override;

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public override void Write(BinaryWriter writer, byte [] buffer)
        {
            base.Write(writer, buffer);

            writer.Write((int)GridDataType);
            writer.Write(CellSize);
            writer.Write(IndexOriginOffset);

            ProdDataMap.Write(writer, buffer);
            FilterMap.Write(writer, buffer);
        }

        /// <summary>
        /// Fill the items array by reading the binary representation using the provided reader. 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="buffer"></param>
        public override void Read(BinaryReader reader, byte [] buffer)
        {
            base.Read(reader, buffer);

            if ((GridDataType)reader.ReadInt32() != GridDataType)
            {
                Debug.Assert(false, "GridDataType in stream does not match GridDataType of local subgrid instance");
            }

            CellSize = reader.ReadDouble();
            IndexOriginOffset = reader.ReadUInt32();

            ProdDataMap.Read(reader, buffer);
            FilterMap.Read(reader, buffer);
        }
    }
}
