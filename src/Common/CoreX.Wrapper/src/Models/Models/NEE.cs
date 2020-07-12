namespace CoreX.Models
{
  public struct NEE
  {
    public double North;
    public double East;
    public double Elevation;

    public override string ToString() => $"North: {North}, East: {East}, Elevation: {Elevation} | ";
  }
}
