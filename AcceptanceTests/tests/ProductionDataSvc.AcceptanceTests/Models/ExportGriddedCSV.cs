using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The representation of a pass counts request
  /// </summary>
  public class ExportGriddedCSVRequest : RequestBase
  {
    /// <summary>
    /// The project to process the CS definition file into.
    /// </summary>
    public long projectId { get; set; }

    /// <summary>
    /// An identifier from the caller. 
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// Determines if the coordinates of the points in the emitted CSV file are in Northing and Easting coordinates or 
    /// in Station and Offset coordinates with respect to a road design centerline supplied as a part of the request.
    /// </summary>
    public GriddedCSVReportType reportType { get; set; }

    /// <summary>
    /// Sets the custom caller identifier.
    /// </summary>
    public string callerId { get; set; }

    /// <summary>
    /// Sets the design file to be used for cut/fill or station/offset calculations
    /// </summary>
    public DesignDescriptor designFile { get; set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    public FilterResult filter { get; set; }

    /// <summary>
    /// The filter ID to be used in the request.
    /// May be null.
    /// </summary>
    public long filterID { get; set; }

    /// <summary>
    /// A collection of parameters and configuration information relating to analysis and determination of material layers.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }

    /// <summary>
    /// The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.
    /// </summary>
    public double interval { get; set; }

    /// <summary>
    /// Include the measured elevation at the sampled location
    /// </summary>
    public bool reportElevation { get; set; }

    /// <summary>
    /// Include the calculated cut-fill between the elevation at the sampled location and the design elevation at the same location
    /// </summary>
    public bool reportCutFill { get; set; }

    /// <summary>
    /// Include the measured CMV at the sampled location
    /// </summary>
    public bool reportCMV { get; set; }

    /// <summary>
    /// Include the measured MDP at the sampled location
    /// </summary>
    public bool reportMDP { get; set; }

    /// <summary>
    /// Include the calculated pass count at the sampled location
    /// </summary>
    public bool reportPassCount { get; set; }

    /// <summary>
    /// Include the measured temperature at the sampled location
    /// </summary>
    public bool reportTemperature { get; set; }

    /// <summary>
    /// *** Currently unclear, related to alignment based gridded CSV exports
    /// </summary>
    public GriddedCSVReportOption reportOption { get; set; }

    /// <summary>
    /// The Northing ordinate of the location to start gridding from
    /// </summary>
    public double startNorthing { get; set; }

    /// <summary>
    /// The Easting ordinate of the location to start gridding from
    /// </summary>
    public double startEasting { get; set; }

    /// <summary>
    /// The Northing ordinate of the location to end gridding at
    /// </summary>
    public double endNorthing { get; set; }

    /// <summary>
    /// The Easting ordinate of the location to end gridding at
    /// </summary>
    public double endEasting { get; set; }

    /// <summary>
    /// The orientation of the grid, expressed in radians
    /// </summary>
    public double direction { get; set; }

    /// <summary>
    /// If set to false, the returned file content is an ANSI text representation of the CSV file. If set to true, the returned file content is a ZIP conpressed archive containing a single file with the name 'asbuilt.csv'.
    /// </summary>
    public bool compress { get; set; }
  }

  public enum GriddedCSVReportType
  {
    Gridded = 1,
    Alignment = 2,
  }

  public enum GriddedCSVReportOption
  {
    Direction = 0,
    EndPoint = 1,
    Automatic = 2
  }
}
