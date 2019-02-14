using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Pipelines.Interfaces
{
  public interface IRequestAnalyser
  {
    /// <summary>
    /// The pipeline that has initiated this request analysis
    /// </summary>
    ISubGridPipelineBase Pipeline { get; set; }

    /// <summary>
    /// The resulting bitmap sub grid tree mask of all sub grids containing production data that need to be requested
    /// </summary>
    ISubGridTreeBitMask ProdDataMask { get; set; }

    /// <summary>
    /// The resulting bitmap sub grid tree mask of all sub grids containing production data that need to be requested
    /// </summary>
    ISubGridTreeBitMask SurveyedSurfaceOnlyMask { get; set; }

    /// <summary>
    /// Indicates if only a single page of sub grid requests will be processed
    /// </summary>
    bool SubmitSinglePageOfRequests { get; set; }

    /// <summary>
    /// The number of sub grids present in a requested page of sub grids
    /// </summary>
    int SinglePageRequestSize { get; set; }

    /// <summary>
    /// The page number of the page of sub grids to be requested
    /// </summary>
    int SinglePageRequestNumber { get; set; }

    /// <summary>
    /// The spatial extents derived from the parameters when building the pipeline
    /// </summary>
    BoundingWorldExtent3D WorldExtents { get; set; }

    /// <summary>
    /// The executor method for the analyser
    /// </summary>
    /// <returns></returns>
    bool Execute();

    /// <summary>
    /// Counts the number of sub grids that will be submitted to the processing engine given the request parameters
    /// supplied to the request analyser.
    /// </summary>
    /// <returns></returns>
    long CountOfSubGridsThatWillBeSubmitted();

    long TotalNumberOfSubGridsAnalysed { get; set; }
    long TotalNumberOfSubGridsToRequest { get; set; }
    long TotalNumberOfCandidateSubGrids { get; set; }
  }
}
