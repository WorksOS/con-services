namespace CCSS.Geometry
{
  internal struct Point
  {
    public double X;
    public double Y;
    public string WKTSubstring => $"{X} {Y}";

    public override bool Equals(object obj)
    {
      var source = (Point)obj;
      return (source.X == X) && (source.Y == Y);
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}
