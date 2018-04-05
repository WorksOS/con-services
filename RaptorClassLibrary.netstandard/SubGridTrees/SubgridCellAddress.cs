using System;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    /// <summary>
    /// Identifies a cell address or location within a subgrid tree
    /// </summary>
    public struct SubGridCellAddress : IEquatable<SubGridCellAddress>
    {
        /// <summary>
        /// Storage for bit flags used for elements such as ProdDataRequested and SurveyedSurfaceDataRequested
        /// </summary>
        private byte DataRequestFlags;

        /// <summary>
        /// The X ordinate of the cell address
        /// </summary>
        public uint X { get; set; }

        /// <summary>
        /// The Y ordinate of the cell address
        /// </summary>
        public uint Y { get; set; }

        /// <summary>
        /// Specifies if production data is being requested with respect to this cell address
        /// </summary>
        public bool ProdDataRequested
        {
            get { return BitFlagHelper.IsBitOn(DataRequestFlags, 0); }
            set { BitFlagHelper.SetBit(ref DataRequestFlags, 0, value); }
        }

        /// <summary>
        /// Specifies if surveyed surface elevation data is being requested with respect to this cell address
        /// </summary>
        public bool SurveyedSurfaceDataRequested
        {
            get { return BitFlagHelper.IsBitOn(DataRequestFlags, 1); }
            set { BitFlagHelper.SetBit(ref DataRequestFlags, 1, value); }
        }

        public SubGridCellAddress(uint AX, uint AY)
        {
            X = AX;
            Y = AY;
            DataRequestFlags = 0;
        }

        public SubGridCellAddress(uint AX, uint AY, bool AProdDataRequested, bool ASurveyedSurfaceDataRequested) : this(AX, AY)
        {
            ProdDataRequested = AProdDataRequested;
            SurveyedSurfaceDataRequested = ASurveyedSurfaceDataRequested;
        }

        /// <summary>
        /// Determine if this subgrid cell address is the same as another subgrid cell address.
        /// The comparison examines only the location of the address, not the data request flags also contained within it
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SubGridCellAddress other) => (X == other.X) && (Y == other.Y);

        /// <summary>
        /// Constructs a single long quantity that encodes both the X & Y elements of the subgrid address.
        /// X occupies the high 32 bits, Y the lower 32 bits
        /// </summary>
        public long ToNormalisedInt64 => ((long)X << 32) | Y;

        /// <summary>
        /// Construct a 'normalised' Int64 descriptor for the cell address in terms of the
        /// leaf subgrid layer that contains the on-the-ground cells that the cell address 
        /// is specified. The resulting normalised address identifies the subgrid parent that contains 
        /// that cell address. All cell addresses within that subgrid will return the same normalised address.
        /// Note: This cell address is expected to be in terms of the cell at the origin
        /// of a subgrid of on the ground cells. The result is an int64 with the subgrid
        /// origin X and Y transformed into cell-relative indices in the next higher layer in
        /// the subgrid tree then expressed as a normalised number in the least significant
        /// number of bits in the resultant Int64 return type.
        /// </summary>
        public long ToNormalisedSubgridOriginInt64 => ((X >> SubGridTree.SubGridIndexBitsPerLevel) << ((SubGridTree.SubGridTreeLevels - 1) * SubGridTree.SubGridIndexBitsPerLevel)) | (Y >> SubGridTree.SubGridIndexBitsPerLevel);

        /// <summary>
        /// Constructs a descriptor from a cell address that skips and interleaves alternate bits from each of the 
        /// X and Y components of the cell address.
        /// </summary>
        public uint ToSkipInterleavedDescriptor => (X & 0xAAAAAAAA) | (Y & 0x55555555);

        /// <summary>
        /// Constructs a spatial division descriptor from a cell address that skips and interleaves alternate bits from each of the 
        /// X and Y components of the cell address with the variation that the cell address is restricted to the address
        /// of the parent subgrid that contains it. All cell addresses within that subgrid will return the same normalised 
        /// origin descriptor.
        /// </summary>
        public static uint ToSpatialDivisionDescriptor(uint X, uint Y, uint numSpatialDivisions) => (((X >> SubGridTree.SubGridIndexBitsPerLevel) & 0xAAAAAAAA) | ((Y >> SubGridTree.SubGridIndexBitsPerLevel) & 0x55555555)) % numSpatialDivisions;

        /// <summary>
        /// Constructs a spatial division descriptor from a cell address that skips and interleaves alternate bits from each of the 
        /// X and Y components of the cell address with the variation that the cell address is restricted to the address
        /// of the parent subgrid that contains it. All cell addresses within that subgrid will return the same normalised 
        /// origin descriptor.
        /// </summary>
        public uint ToSpatialDivisionDescriptor(uint numSpatialDivisions) => (((X >> SubGridTree.SubGridIndexBitsPerLevel) & 0xAAAAAAAA) | ((Y >> SubGridTree.SubGridIndexBitsPerLevel) & 0x55555555)) % numSpatialDivisions;

        /// <summary>
        /// Constructs a descriptor from a cell address that skips and interleaves alternate bits from each of the 
        /// X and Y components of the cell address with the variation that the cell address is restricted to the address
        /// of the parent subgrid that contains it. All cell addresses within that subgrid will return the same normalised 
        /// origin descriptor.
        /// </summary>
        public uint ToSkipInterleavedSubgridOriginDescriptor => ((X >> SubGridTree.SubGridIndexBitsPerLevel) & 0xAAAAAAAA) | ((Y >> SubGridTree.SubGridIndexBitsPerLevel) & 0x55555555);

        /// <summary>
        /// Produce a human readable form of the cell address information
        /// </summary>
        /// <returns></returns>
        public override string ToString() => string.Format("{0}:{1}", X, Y);

        /// <summary>
        /// Sets the state of a cell address struct
        /// </summary>
        /// <param name="AX"></param>
        /// <param name="AY"></param>
        /// <param name="AProdDataRequested"></param>
        /// <param name="ASurveyedSurfaceDataRequested"></param>
        public void Set(uint AX, uint AY, bool AProdDataRequested, bool ASurveyedSurfaceDataRequested)
        {
            X = AX;
            Y = AY;
            ProdDataRequested = AProdDataRequested;
            SurveyedSurfaceDataRequested = ASurveyedSurfaceDataRequested;
        }
    }
}
