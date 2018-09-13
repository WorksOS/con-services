using System;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines.Interfaces
{
    /// <summary>
    /// Interface for pipeline state and control for tasks and requests to access
    /// </summary>
    public interface ISubGridPipelineBase
    {
        /// <summary>
        /// The type of grid data to be seleted from the data model
        /// </summary>
        GridDataType GridDataType { get; set; }

        ITask PipelineTask { get; set; }

        /// <summary>
        /// Notes if the underlyinf query needs to include surveyed surface information in its results
        /// </summary>
        bool IncludeSurveyedSurfaceInformation { get; set; }

        /// <summary>
        /// The request descriptor ID for this request
        /// </summary>
        Guid RequestDescriptor { get; set; }

        /// <summary>
        /// A restriction on the cells that are returned via the query that intersects with the spatial seelction filtering and criteria
        /// </summary>
        BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; }

        AreaControlSet AreaControlSet { get; set; }

        /// <summary>
        /// Advise the pipeline processing has been aborted
        /// </summary>
        void Abort();

        /// <summary>
        /// Determine if the pipeline has been aborted
        /// </summary>
        bool Aborted { get; }

        /// <summary>
        /// Determine if the pipeline was proactively terminated
        /// </summary>
        bool Terminated { get; set;  }

        /// <summary>
        /// Advise the pipeline that all processing activities have been completed
        /// </summary>
        bool PipelineCompleted { get; set; }

        /// <summary>
        /// Date model the pipeline is operating on
        /// </summary>
        Guid DataModelID { get; set; }

        /// <summary>
        /// Advise the client of the pipeline that a single subgrid has been processed
        /// </summary>
        void SubgridProcessed();

        /// <summary>
        /// Advise the client of the pipeline that a group of numProcessed subgrids has been processed
        /// </summary>
        void SubgridsProcessed(long numProcessed);

        /// <summary>
        /// The set of filter the pipeline requestas are operating under
        /// </summary>
        IFilterSet FilterSet { get; set; }

        /// <summary>
        /// Map of all subgrids requiring infromation be requested from them
        /// </summary>
        ISubGridTreeBitMask OverallExistenceMap { get; set; }
      
        /// <summary>
        /// Map of all subgrids that specifically require production data to be requested for them
        /// </summary>
        ISubGridTreeBitMask ProdDataExistenceMap { get; set; }
      
        /// <summary>
        /// Map of all subgrids that require elevation data to be extracted from a design surface
        /// </summary>
        ISubGridTreeBitMask DesignSubgridOverlayMap { get; set; }

        /// <summary>
        /// Initiates processing of the pipeline
        /// </summary>
        bool Initiate();

        /// <summary>
        /// Wait for the pipeline to completes operations, or abort at expiration of time to live timeout
        /// </summary>
        void WaitForCompletion();

        IRequestAnalyser RequestAnalyser { get; set; }
    }
}
