namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class PointLL
  {
    /// <summary>
    /// Gets or sets the latitude
    /// </summary>
    public double latitude { get; set; }
    /// <summary>
    /// Gets or sets the longitude
    /// </summary>
    public double longitude { get; set; }
    public PointLL(double latitude, double longitude)
    {
      this.latitude = latitude;
      this.longitude = longitude;
    }
  }
}
