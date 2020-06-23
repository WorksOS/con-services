using System;

namespace VSS.TRex.SiteModels.GridFabric.Requests
{
  [Flags]
  public enum DeleteSiteModelSelectivity
  {
    None = 0x0,

    /// <summary>
    /// Identifies all data held in the data model produced by processing TAG files. This represents spatial data, event data,
    /// machines, machine designs, proofing runs and the existence map
    /// </summary>
    TagFileDerivedData = 0x1,

    /// <summary>
    /// Identifies the site model metadata record
    /// </summary>
    SiteModelMetadata = 0x2,

    /// <summary>
    /// Identifies the coordinate system added to the project
    /// </summary>
    CoordinateSystem = 0x4,

    /// <summary>
    /// Identifies the list of designs added to the project
    /// </summary>
    Designs = 0x8,

    /// <summary>
    /// Identifies the list of alignments added to the project
    /// </summary>
    Alignments = 0x10,

    /// <summary>
    /// Identifies the list of surveyed surfaces added to the project
    /// </summary>
    SurveyedSurfaces = 0x20,

    /// <summary>
    /// Identifies the core site model record representing the project in TRex
    /// </summary>
    SiteModel = 0x40,

    /// <summary>
    /// Identifies all data added to a date model that did not originate from TAG file processing.
    /// This includes certain types of events (overrides) that are user generated versus recorded by a machine
    /// </summary>
    NonTagFileDerivedData = CoordinateSystem | SiteModelMetadata | Designs | Alignments | SurveyedSurfaces,

    /// <summary>
    /// Remove everything from the data model
    /// </summary>
    All = TagFileDerivedData | NonTagFileDerivedData | SiteModelMetadata | SiteModel
  }
}
