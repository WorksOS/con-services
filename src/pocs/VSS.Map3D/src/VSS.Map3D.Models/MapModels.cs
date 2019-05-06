using System;
using System.Globalization;

namespace VSS.Map3D.Models
{

  public struct BoundingRect2
  {
    public double North;
    public double South;
    public double East;
    public double West;
    public string ToDisplay()
    {
      return $"({Math.Round(West, 7)},{Math.Round(South, 7)}) - ({Math.Round(East, 7)},{Math.Round(North,7)})";
    }

    public BoundingRect2(double west, double south, double east, double north)
    {
      West = west;
      South = south;
      East = east;
      North = north;
    }
  }

}
