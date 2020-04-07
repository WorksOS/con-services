namespace VSS.TRex.Pipelines.Interfaces
{
  public enum PipelineProcessorPipelineStyle
  {
    /// <summary>
    /// Uses a derivative of SubGridPipelineProgressive to orchestrate the sub grid query pipeline
    /// </summary>
    DefaultProgressive,

    /// <summary>
    /// Uses a derivative of SubGridPipelineAggregative to orchestrate the sub grid query pipeline
    /// </summary>
    DefaultAggregative,

    /// <summary>
    /// An aggregating sub grid pipeline processor oriented towards progressive volume calculations that
    /// require custom parameters in the sub grids request argument
    /// </summary>
    ProgressiveVolumes
  }
}
