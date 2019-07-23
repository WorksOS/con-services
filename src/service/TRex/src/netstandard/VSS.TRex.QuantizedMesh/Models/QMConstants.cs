using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.QuantizedMesh.Models
{
  public static class QMConstants
  {
    public const int LowResolutionTile = 10;   // Tile GridSize
    public const int MidResolutionTile = 50;   // Tile GridSize
    public const int HighResolutionTile = 100;  // Tile GridSize
    public const int LowResolutionLevel = 0;   // 0 to MidResolutionLevel-1
    public const int MidResolutionLevel = 14;  // MidResolutionLevel to HighResolutionLevel-1
    public const int HighResolutionLevel = 19; // HighResolutionLevel to max
  }
}
