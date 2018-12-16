using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// </summary>
  public class ProfileRequest : RequestBase
  {
    /// <summary>
    /// The project to perform the request against
    /// </summary>
    public long? projectID { get; set; }

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// The type of profile to be generated.
    /// </summary>
    public ProductionDataType profileType { get; set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    public FilterResult filter { get; set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// Value may be null.
    /// </summary>
    public long? filterID { get; set; }

    /// <summary>
    /// The descriptor for an alignment centerline design to be used as the geometry along which the profile is generated
    /// Value may be null.
    /// </summary>
    public DesignDescriptor alignmentDesign { get; set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coorinates are expressed in terms of the grid coordinate system used by the project. Values are expressed in meters.
    /// Value may be null.
    /// </summary>
    public ProfileGridPoints gridPoints { get; set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coorinates are expressed in terms of the WGS84 lat/lon coordinates. Values are expressed in radians.
    /// Value may be null.
    /// </summary>
    public ProfileLLPoints wgs84Points { get; set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to start computing the profile from. Values are expressed in meters.
    /// </summary>
    public double? startStation { get; set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to finish computing the profile at. Values are expressed in meters.
    /// </summary>
    public double? endStation { get; set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }

    /// <summary>
    /// Return all analysed layers and cell passes along with the summary cell based results of the profile query
    /// </summary>
    public bool returnAllPassesAndLayers { get; set; }
  }
}
