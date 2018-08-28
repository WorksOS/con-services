namespace VSS.TRex.Pipelines.Interfaces
{
  public enum PipelineProcessorPipelineStyle
  {
    /// <summary>
    /// Uses a derivative of SubGridPipelineProgressive to orchestrate the subgrid query pipeline
    /// </summary>
    DefaultProgressive,

    /// <summary>
    /// Uses a derivative of SubGridPipelineAggregative to orchestrate the subgrid query pipeline
    /// </summary>
    DefaultAggregative
  }
}
