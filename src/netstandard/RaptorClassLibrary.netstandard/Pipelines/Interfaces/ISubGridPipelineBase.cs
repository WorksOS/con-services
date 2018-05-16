using System;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Pipelines.Interfaces
{
    /// <summary>
    /// Interface for pipeline state and control for tasks and requests to access
    /// </summary>
    public interface ISubGridPipelineBase
    {
        /// <summary>
        /// Advise the pipelien processing has been aborted
        /// </summary>
        void Abort();

        /// <summary>
        /// Determine if the pipeline has been aborted
        /// </summary>
        bool Aborted { get; }

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
        /// The set of filtere the pipeline requestas are operating under
        /// </summary>
        FilterSet FilterSet { get; set; }

        /// <summary>
        /// Map of all subgrids requiring infromation be requested from them
        /// </summary>
        SubGridTreeSubGridExistenceBitMask OverallExistenceMap { get; set; }

        /// <summary>
        /// Map of all subgrids that specifically require production data to be requested for them
        /// </summary>
        SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap { get; set; }

        /// <summary>
        /// Map of all subgrids that require elevation data to be extracted from a design surface
        /// </summary>
        SubGridTreeSubGridExistenceBitMask DesignSubgridOverlayMap { get; set; }
        
    }
}
