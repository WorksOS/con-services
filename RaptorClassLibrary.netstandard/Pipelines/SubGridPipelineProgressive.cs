﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Pipelines
{
    /// <summary>
    /// Defines a generic class that decorates progressive pipeline semantics with the desired argument and request response
    /// </summary>
    /// <typeparam name="TSubGridsRequestArgument"></typeparam>
    /// <typeparam name="TSubGridRequestsResponse"></typeparam>
    public class SubGridPipelineProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridPipelineBase<TSubGridsRequestArgument, TSubGridRequestsResponse,
        SubGridRequestsProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>>
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        /// <param name="AID"></param>
        /// <param name="task"></param>
        public SubGridPipelineProgressive(int AID, PipelinedSubGridTask task) : base(AID, task)
        {

        }
    }
}
