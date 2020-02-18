namespace VSS.TRex.DataSmoothing
{
  public interface IDataSmoother
  {
    /// <summary>
    /// The additional border of values around the context to be smoothed to provide consistent smoothing of the
    /// cells around the edges of 2D arrays of source values.
    /// </summary>
    int AdditionalBorderSize { get; }
  }
}
