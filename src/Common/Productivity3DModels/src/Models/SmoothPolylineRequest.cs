namespace VSS.Productivity3D.Models.Models
{
  public class SmoothPolylineRequest
  {
    public const int NOT_DEFINED = -1;

    public double[,] Coordinates { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of points the returned GeoJSON should include. 
    /// </summary>
    public int MaxPoints { get; set; } = NOT_DEFINED;
  }
}
