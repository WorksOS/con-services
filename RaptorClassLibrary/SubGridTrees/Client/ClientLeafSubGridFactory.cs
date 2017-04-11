using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Client
{
    /// <summary>
    /// Factory responsible for creating concrete 'gred data' specific client sub grid leaf instances
    /// </summary>
    public class ClientLeafSubGridFactory : IClientLeafSubgridFactory
    {
        /// <summary>
        /// Simple array to hold client leaf subgrid type map
        /// </summary>
        private static Type[] typeMap = null;

        /// <summary>
        /// Static class constructor to initialise types
        /// </summary>
        static ClientLeafSubGridFactory()
        {
            typeMap = new Type[]
            {
                null, // All = $00000000;
                null, // CCV = $00000001;
                null, // Height = $00000002;
                null, // Latency = $00000003;
                null, // PassCount = $00000004;
                null, // Frequency = $00000005;
                null, // Amplitude = $00000006;
                null, // Moisture = $00000007;
                null, // Temperature = $00000008;
                null, // RMV = $00000009;
                null, // CCVPercent = $0000000B;
                null, // GPSMode = $0000000A;
                null, // SimpleVolumeOverlay = $0000000C;
                null, // HeightAndTime = $0000000D;
                null, // CompositeHeights = $0000000E;
                null, // MDP = $0000000F;
                null, // MDPPercent = $00000010;
                null, //  CellProfile = $00000011;
                null, //   CellPasses = $00000012;
                null, //   MachineSpeed = $00000013;
                null, //  CCVPercentChange = $00000014;
                null, //  MachineSpeedTarget = $00000015;
                null, // CCVPercentChangeIgnoredTopNullValue = $0000016
                null, // CCA = $0000017
                null  // CCAPerccent = = $0000018
            };
        }

        /// <summary>
        /// Register a type implementing IClientLeafSubGrid against a grid data type for the factory to 
        /// create on demand
        /// </summary>
        /// <param name="gridDataType"></param>
        /// <param name="type"></param>
        public void RegisterClientLeafSubGridType(GridDataType gridDataType, Type type)
        {
            // Check that the type being passed in meets the requirement for 
            // implementing the IClienLeafSubGrid interface
            if (!(typeof(IClientLeafSubGrid).IsAssignableFrom(type)))
            {
                throw new ArgumentException("ClientLeafSubGridFactory requires a types that implements IClientLeafSubGrid", "type");
            }

            if ((int)gridDataType > typeMap.Length)
            {
                throw new ArgumentException("Unknown grid data type in RegisterClientLeafSubgridType", "gridDataType");
            }

            typeMap[(int)gridDataType] = type;
        }

        /// <summary>
        /// Construct a concrete instance of a subgrid implementing the IClientLeafSubGrid interface based
        /// on the role it should play according to the grid data type requested. All aspects of leaf ownership
        /// by a subgrid tree, parentage, level, cellsize, indexoriginoffset are delegated responsibilities
        /// of the caller or a derived factory class
        /// </summary>
        /// <param name="gridDataType"></param>
        /// <returns>An appropriate instance derived from ClientLeafSubgrid</returns>
        public IClientLeafSubGrid GetSubGrid(GridDataType gridDataType)
        {
            return (IClientLeafSubGrid)Activator.CreateInstance
                (
                typeMap[(int)gridDataType], // IClientLeafSubGrid type
                null, // Subgrid tree owner
                null, // Subgrid parent
                SubGridTree.SubGridTreeLevels, // Level, default to standard tree levels
                0.0, // Cell Size
                SubGridTree.DefaultIndexOriginOffset // IndexOfiginOffset, default to tree default value
                );
        }
    }
}
