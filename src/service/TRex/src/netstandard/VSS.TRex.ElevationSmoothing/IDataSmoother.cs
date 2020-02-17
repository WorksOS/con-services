namespace VSS.TRex.DataSmoothing
{
  public interface IDataSmoother
  {
    /// <summary>
    /// The additional border of values around the context to be smooted to provide consistent smotthing of the
    /// cells around the edges of 2D arrays of source values.
    /// </summary>
    int AdditionalBorderSize { get; }
  }
}
