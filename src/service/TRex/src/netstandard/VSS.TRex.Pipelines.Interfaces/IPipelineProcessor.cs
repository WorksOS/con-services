using VSS.TRex.Common.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines.Interfaces
{
  public interface IPipelineProcessor
  {
    /// <summary>
    /// Reference to the site model incolved in the request
    /// </summary>
    ISiteModel SiteModel { get; set; }

    /// <summary>
    /// Records if the pipeline was aborted before completing operations
    /// </summary>
    bool PipelineAborted { get; set; }

    /// <summary>
    /// The task to be fitted to the pipelien to mediate subgrid retrieval and procesing
    /// </summary>
    ITRexTask Task { get; set; }

    /// <summary>
    /// The pipe lien used to retrive subgrids from the cluster compute layer
    /// </summary>
    ISubGridPipelineBase Pipeline { get; set; }

    /// <summary>
    /// The request analyser used to determine the subgrids to be sent to the cluster compute layer
    /// </summary>
    IRequestAnalyser RequestAnalyser { get; set; }

    /// <summary>
    /// The respons esupplied to the pipeline processor
    /// </summary>
    ISubGridsPipelinedReponseBase Response { get; set; }

    /// <summary>
    /// Indicates if the pipeline was aborted due to a TTL timeout
    /// </summary>
    bool AbortedDueToTimeout { get; set; }

    /// <summary>
    /// Indicates if the pipeline requests require the inclusion of surveyed surface information
    /// </summary>
    bool RequireSurveyedSurfaceInformation { get; set; }

    /// <summary>
    /// If this request involves a relationship with a design then ensure the existence map
    /// for the design is loaded in to memory to allow the request pipeline to confine
    /// subgrid requests that overlay the actual design
    /// </summary>
    bool RequestRequiresAccessToDesignFileExistenceMap { get; set; }

    /// <summary>
    /// Any override world coordinate spatial extent imposed by the client context.
    /// For example, this might be the rectangular border of a tile being requested
    /// </summary>
    BoundingWorldExtent3D OverrideSpatialExtents { get; set; }

    /// <summary>
    /// A restriction on the cells that are returned via the query that intersects with the spatial seelction filtering and criteria
    /// </summary>
    BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; }

    /// <summary>
    /// Builds the pipeline configured per the supplied state ready to exesute the request
    /// </summary>
    /// <returns></returns>
    bool Build();

    /// <summary>
    /// Performing all processing activities to retrieve subgrids
    /// </summary>
    void Process();

    /// <summary>
    /// The spatial extents derived from the parameters when building the pipeline
    /// </summary>
    BoundingWorldExtent3D SpatialExtents { get; set; }

    /// <summary>
    /// Grid data type to be processed and/or returned by the query (eg: Height, CutFill etc)
    /// </summary>
    GridDataType GridDataType { get; set; }

    ISubGridTreeBitMask ProdDataExistenceMap { get; set; }
    ISubGridTreeBitMask OverallExistenceMap { get; set; }
    ISubGridTreeBitMask DesignSubgridOverlayMap { get; set; }
  }
}
