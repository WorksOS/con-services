using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Executors.Tasks.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.GridFabric.Requests
{
    /// <summary>
    /// The SubGridRequests GridFabric class sends a request to the grid for a collection of subgrids to be processed according 
    /// to relevant filters other parameters. The grid fabric responds with responses as the servers in the fabric compute them, sending
    /// them to the TRex node identified by the TRexNodeId property
    /// </summary>
    public abstract class SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse> : CacheComputePoolRequest<TSubGridsRequestArgument, TSubGridRequestsResponse> 
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Task is the business logic that will handle the response to the subgrids request
        /// </summary>
        public ITask Task;

        /// <summary>
        /// The request argument to be passed to target of the request
        /// </summary>
        public TSubGridsRequestArgument arg;

        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public Guid SiteModelID { get; set; } = Guid.Empty;

        /// <summary>
        /// The request ID assigned to the activity requiring these subgrids to be requested. This ID is used to funnel 
        /// traffic from the processing cluster into the correct processing context
        /// </summary>
        public Guid RequestID { get; set; } = Guid.Empty;

        /// <summary>
        /// The identifier of the TRex Node that is issuing the request for subgrids and which wants to receive the processed
        /// subgrid responses
        /// </summary>
        public string TRexNodeId { get; set; } = string.Empty;

        /// <summary>
        /// The type of grid data to be retrieved from the subgrid requests
        /// </summary>
        public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// A subgrid bit mask tree identifying all the production data subgrids that require processing
        /// </summary>
        public SubGridTreeSubGridExistenceBitMask ProdDataMask { get; set; }

        /// <summary>
        /// A subgrid bit mask tree identifying all the surveyd surface subgrids that require processing
        /// </summary>
        public SubGridTreeSubGridExistenceBitMask SurveyedSurfaceOnlyMask { get; set; }

        /// <summary>
        /// The set of filters to be applied to the subgrids being processed
        /// </summary>
        public FilterSet Filters { get; set; }

        /// <summary>
        /// Denotes whether results of these requests should include any surveyed surfaces in the site model
        /// </summary>
        public bool IncludeSurveyedSurfaceInformation { get; set; }

        /// <summary>
        /// The design to be used in cases of cut/fill subgrid requests
        /// </summary>
        public Guid CutFillDesignID { get; set; } = Guid.Empty;

        /// <summary>
        /// No arg constructor that establishes this request as a cache compute request. 
        /// of subgrid processing is returned as a set of partitioned results from the Broadcast() invocation.
        /// </summary>
        public SubGridRequestsBase()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="task"></param>
        /// <param name="siteModelID"></param>
        /// <param name="requestID"></param>
        /// <param name="trexNodeId"></param>
        /// <param name="requestedGridDataType"></param>
        /// <param name="includeSurveyedSurfaceInformation"></param>
        /// <param name="prodDataMask"></param>
        /// <param name="surveyedSurfaceOnlyMask"></param>
        /// <param name="filters"></param>
        /// <param name="cutFillDesignID"></param>
        public SubGridRequestsBase(ITask task,
                                   Guid siteModelID, 
                                   Guid requestID, 
                                   string trexNodeId, 
                                   GridDataType requestedGridDataType, 
                                   bool includeSurveyedSurfaceInformation,
                                   SubGridTreeSubGridExistenceBitMask prodDataMask,
                                   SubGridTreeSubGridExistenceBitMask surveyedSurfaceOnlyMask,
                                   FilterSet filters,
                                   Guid cutFillDesignID) : this()
        {
            Task = task;
            SiteModelID = siteModelID;
            RequestID = requestID;
            TRexNodeId = trexNodeId;
            RequestedGridDataType = requestedGridDataType;
            IncludeSurveyedSurfaceInformation = includeSurveyedSurfaceInformation;
            ProdDataMask = prodDataMask;
            SurveyedSurfaceOnlyMask = surveyedSurfaceOnlyMask;
            Filters = filters;
            CutFillDesignID = cutFillDesignID;
        }

        /// <summary>
        /// Unpacks elements of the request argument that are represented as byte arrays in the Ignite request
        /// </summary>
        /// <returns></returns>
        protected void PrepareArgument()
        {
            Log.LogInformation($"Preparing argument with TRexNodeId = {TRexNodeId}");

            using (MemoryStream ProdDataMS = new MemoryStream(), SurveyedSurfaceMS = new MemoryStream())
            {
                using (BinaryWriter ProdDataWriter = new BinaryWriter(ProdDataMS), SurveyedSurfaceWriter = new BinaryWriter(SurveyedSurfaceMS))
                {
                    SubGridTreePersistor.Write(ProdDataMask, ProdDataWriter);
                    SubGridTreePersistor.Write(SurveyedSurfaceOnlyMask, SurveyedSurfaceWriter);

                    arg = new TSubGridsRequestArgument()
                    {
                        SiteModelID = SiteModelID,
                        RequestID = RequestID,
                        GridDataType = RequestedGridDataType,
                        IncludeSurveyedSurfaceInformation = IncludeSurveyedSurfaceInformation,
                        ProdDataMaskBytes = ProdDataMS.ToArray(),
                        SurveyedSurfaceOnlyMaskBytes = SurveyedSurfaceMS.ToArray(),
                        Filters = Filters,
                        MessageTopic = string.Format("SubGridRequest:{0}", RequestID),
                        TRexNodeID = TRexNodeId,
                        CutFillDesignID = CutFillDesignID
                    };
                }
            }
        }

        protected void CheckArguments()
        {
            // Make sure things look kosher
            if (ProdDataMask == null || SurveyedSurfaceOnlyMask == null || Filters == null || RequestID == Guid.Empty)
            {
                if (ProdDataMask == null)
                    throw new ArgumentException("ProdDataMask not initialised");
                if (SurveyedSurfaceOnlyMask == null)
                    throw new ArgumentException("SurveyedSurfaceOnlyMask not initialised");
                if (Filters == null)
                    throw new ArgumentException("Filters not initialised");
                if (RequestID == Guid.Empty)
                    throw new ArgumentException("RequestID not initialised");
            }
        }

        /// <summary>
        /// Executes a request for a number of subgrids to be processed according to filters and other
        /// parameters
        /// </summary>
        /// <returns></returns>
        public abstract ICollection<TSubGridRequestsResponse> Execute();
    }
}
