using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        long FID = long.MinValue;
        DesignDescriptor FDesignDescriptor;
        BoundingWorldExtent3D FExtents;

        public void Write(BinaryWriter writer)
        {
            writer.Write(FID);
            FDesignDescriptor.Write(writer);
            FExtents.Write(writer);
        }

        public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

        public void Read(BinaryReader reader)
        {
            FID = reader.ReadInt64();
            FDesignDescriptor.Read(reader);
            FExtents.Read(reader);
        }

        public long ID { get { return FID; } }
        public DesignDescriptor DesignDescriptor { get { return FDesignDescriptor; } }
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
        public Design(BinaryReader reader) : base()
        {
            Read(reader);
        }

        /// <summary>
        /// Constructor accepting full design state
        /// </summary>
        /// <param name="AID"></param>
        /// <param name="ADesignDescriptor"></param>
        /// <param name="AAsAtDate"></param>
        public Design(long AID,
                      DesignDescriptor ADesignDescriptor,
                      BoundingWorldExtent3D AExtents) : base()
        {
            FID = AID;
            FDesignDescriptor = ADesignDescriptor;
            FExtents = AExtents;
        }

        /// <summary>
        /// Produces a deep clone of the design
        /// </summary>
        /// <returns></returns>
        public Design Clone() => new Design(FID, FDesignDescriptor, FExtents);

        /// <summary>
        /// ToString() for Design
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("ID:{0}, DesignID:{1}; {2};{3};{4} {5:F3} [{6}]",
                            FID,
                             FDesignDescriptor.DesignID,
                             FDesignDescriptor.FileSpace, FDesignDescriptor.Folder, FDesignDescriptor.FileName,
                             FDesignDescriptor.Offset,
                             FExtents);
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

        public bool GetDesignHeights(long siteModelID,
                                     SubGridCellAddress originCellAddress,
                                     double cellSize,
                                     out ClientHeightLeafSubGrid designHeights,
                                     out DesignProfilerRequestResult errorCode)
        {
            return GetDesignHeights(DesignDescriptor, siteModelID, originCellAddress, cellSize, out designHeights, out errorCode);
        }

        public static bool GetDesignHeights(DesignDescriptor DesignDescriptor,
                                            long siteModelID,
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
                DesignElevationPatchRequest request = new DesignElevationPatchRequest();

                designHeights = request.Execute(new CalculateDesignElevationPatchArgument()
                {
                    CellSize = cellSize,
                    DesignDescriptor = DesignDescriptor,
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
