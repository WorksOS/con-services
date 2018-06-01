using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Executors.Tasks.Interfaces;

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
    /// Indicates if the pipeline was aborted due to a TTL timeout
    /// </summary>
    bool AbortedDueToTimeout { get; set; }

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
