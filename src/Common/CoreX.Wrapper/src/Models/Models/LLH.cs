namespace CoreX.Models
{
  public struct LLH
  {
    public double Latitude;
    public double Longitude;
    public double Height;

    public override string ToString() => $"Lat: {Latitude}, Lon: {Longitude}, Height: {Height}";
  }
}
