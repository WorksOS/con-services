﻿using System;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Pipelines;
using VSS.VisionLink.Raptor.Pipelines.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks.Interfaces
{
    public interface ITask  //<TSubGridRequestor> where TSubGridRequestor : SubGridRequestsBase, new()
    {
        void Cancel();

        bool TransferResponse(object response);

        GridDataType GridDataType { get; set; }

        string RaptorNodeID { get; set; }

        ISubGridPipelineBase PipeLine { get; set; }
    }
}