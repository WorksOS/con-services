using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Tasks.Interfaces;

namespace VSS.TRex.Pipelines.Interfaces
{
  public interface IPipelineProcessor
  {
    /// <summary>
    /// Records if the pipeline was aborted before completing operations
    /// </summary>
    bool PipelineAborted { get; set; }

    /// <summary>
    /// The task to be fitted to the pipelien to mediate subgrid retrieval and procesing
    /// </summary>
    ITask Task { get; set; }

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
    /// If this request involves a relationship with a design then ensure the existance map
    /// for the design is loaded in to memory to allow the request pipeline to confine
    /// subgrid requests that overlay the actual design
    /// </summary>
    bool RequestRequiresAccessToDesignFileExistanceMap { get; set; }

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
  }
}
