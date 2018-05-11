using System;
using System.IO;
using VSS.Velociraptor.DesignProfiling;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;
using VSS.Velociraptor.DesignProfiling.GridFabric.Requests;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.Utilities.Interfaces;

namespace VSS.VisionLink.Raptor.Designs.Storage
{
    /// <summary>
    /// Represents the information known about a design
    /// </summary>
    [Serializable]
    public class Design : IEquatable<Design>, IBinaryReaderWriter
    {
        DesignDescriptor FDesignDescriptor;
        BoundingWorldExtent3D FExtents;

        /// <summary>
        /// Singleton request used by all designs. This request encapsulates the Ignite reference which
        /// is relatively slow to initialise when making many calls.
        /// </summary>
        private static DesignElevationPatchRequest request = new DesignElevationPatchRequest();

        /// <summary>
        /// Binary serialization logic
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(ID);
            FDesignDescriptor.Write(writer);
            FExtents.Write(writer);
        }

        /// <summary>
        /// Binary serialization logic
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

        /// <summary>
        /// Binary deserialization logic
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {
            ID = reader.ReadInt64();
            FDesignDescriptor.Read(reader);
            FExtents.Read(reader);
        }

        /// <summary>
        /// The intenal identifier of the design
        /// </summary>
        public long ID { get; private set; } = long.MinValue;

        /// <summary>
        /// The full design descriptior representing the design
        /// </summary>
        public DesignDescriptor DesignDescriptor { get { return FDesignDescriptor; } }

        /// <summary>
        /// The rectangular bounding extents of the design in grid coordiantes
        /// </summary>
        public BoundingWorldExtent3D Extents { get { return FExtents; } }

        /// <summary>
        /// No-arg constructor
        /// </summary>
        public Design()
        {
        }

        /// <summary>
        /// Constructor accepting a Binary Reader instance from which to instantiate itself
        /// </summary>
        /// <param name="reader"></param>
        public Design(BinaryReader reader)
        {
            Read(reader);
        }

        /// <summary>
        /// Constructor accepting full design state
        /// </summary>
        /// <param name="AID"></param>
        /// <param name="ADesignDescriptor"></param>
        /// <param name="AExtents"></param>
        public Design(long AID,
                      DesignDescriptor ADesignDescriptor,
                      BoundingWorldExtent3D AExtents)
        {
            ID = AID;
            FDesignDescriptor = ADesignDescriptor;
            FExtents = AExtents;
        }

        /// <summary>
        /// Produces a deep clone of the design
        /// </summary>
        /// <returns></returns>
        public Design Clone() => new Design(ID, FDesignDescriptor, FExtents);

        /// <summary>
        /// ToString() for Design
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"ID:{ID}, DesignID:{FDesignDescriptor.DesignID}; {FDesignDescriptor.FileSpace};{FDesignDescriptor.Folder};{FDesignDescriptor.FileName} {FDesignDescriptor.Offset:F3} [{FExtents}]";
        }

        /// <summary>
        /// Determine if two designs are equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Design other)
        {
            return (ID == other.ID) &&
                   FDesignDescriptor.Equals(other.DesignDescriptor) &&
                   (FExtents.Equals(other.Extents));
        }

        /// <summary>
        /// Calculates an elevation subgrid for a desginatec subgrid on this design
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="originCellAddress"></param>
        /// <param name="cellSize"></param>
        /// <param name="designHeights"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public bool GetDesignHeights(Guid siteModelID,
                                     SubGridCellAddress originCellAddress,
                                     double cellSize,
                                     out ClientHeightLeafSubGrid designHeights,
                                     out DesignProfilerRequestResult errorCode)
        {
            // Query the DesignProfiler service to get the patch of elevations calculated
            errorCode = DesignProfilerRequestResult.OK;
            designHeights = null;

            try
            {
                designHeights = request.Execute(new CalculateDesignElevationPatchArgument()
                {
                    CellSize = cellSize,
                    DesignDescriptor = FDesignDescriptor,
                    OriginX = originCellAddress.X,
                    OriginY = originCellAddress.Y,
                    // ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled),
                    SiteModelID = siteModelID
                });
            }
            catch
            {
                errorCode = DesignProfilerRequestResult.UnknownError;
            }

            return errorCode == DesignProfilerRequestResult.OK;
        }
    }
}
