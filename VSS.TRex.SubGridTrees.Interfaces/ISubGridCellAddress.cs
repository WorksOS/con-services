using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  public interface ISubGridCellAddress
  {
    /// <summary>
    /// The X ordinate of the cell address
    /// </summary>
    uint X { get; set; }

    /// <summary>
    /// The Y ordinate of the cell address
    /// </summary>
    uint Y { get; set; }

    /// <summary>
    /// Specifies if production data is being requested with respect to this cell address
    /// </summary>
    bool ProdDataRequested { get; set; }

    /// <summary>
    /// Specifies if surveyed surface elevation data is being requested with respect to this cell address
    /// </summary>
    bool SurveyedSurfaceDataRequested { get; set; }

    /// <summary>
    /// Constructs a single long quantity that encodes both the X & Y elements of the subgrid address.
    /// X occupies the high 32 bits, Y the lower 32 bits
    /// </summary>
    long ToNormalisedInt64 { get; }

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
    long ToNormalisedSubgridOriginInt64 { get; }

    /// <summary>
    /// Constructs a descriptor from a cell address that skips and interleaves alternate bits from each of the 
    /// X and Y components of the cell address.
    /// </summary>
    uint ToSkipInterleavedDescriptor { get; }

    /// <summary>
    /// Constructs a descriptor from a cell address that skips and interleaves alternate bits from each of the 
    /// X and Y components of the cell address with the variation that the cell address is restricted to the address
    /// of the parent subgrid that contains it. All cell addresses within that subgrid will return the same normalised 
    /// origin descriptor.
    /// </summary>
    uint ToSkipInterleavedSubgridOriginDescriptor { get; }

    /// <summary>
    /// Determine if this subgrid cell address is the same as another subgrid cell address.
    /// The comparison examines only the location of the address, not the data request flags also contained within it
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool Equals(ISubGridCellAddress other);

    /// <summary>
    /// Constructs a spatial division descriptor from a cell address that skips and interleaves alternate bits from each of the 
    /// X and Y components of the cell address with the variation that the cell address is restricted to the address
    /// of the parent subgrid that contains it. All cell addresses within that subgrid will return the same normalised 
    /// origin descriptor.
    /// </summary>
    uint ToSpatialDivisionDescriptor(uint numSpatialDivisions);

    /// <summary>
    /// Constructs a spatial partition descriptor from a cell address that skips and interleaves alternate bits from each of the 
    /// X and Y components of the cell address with the variation that the cell address is restricted to the address
    /// of the parent subgrid that contains it. All cell addresses within that subgrid will return the same normalised 
    /// origin descriptor.
    /// </summary>
    uint ToSpatialPartitionDescriptor();

    /// <summary>
    /// Produce a human readable form of the cell address information
    /// </summary>
    /// <returns></returns>
    string ToString();

    /// <summary>
    /// Sets the state of a cell address struct
    /// </summary>
    /// <param name="AX"></param>
    /// <param name="AY"></param>
    /// <param name="AProdDataRequested"></param>
    /// <param name="ASurveyedSurfaceDataRequested"></param>
    void Set(uint AX, uint AY, bool AProdDataRequested, bool ASurveyedSurfaceDataRequested);
  }
}
