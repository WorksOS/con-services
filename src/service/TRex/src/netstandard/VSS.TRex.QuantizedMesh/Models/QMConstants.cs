using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.QuantizedMesh.Models
{
  public static class QMConstants
  {
    // Always make gridsize an odd number for best results
    public const int FlatResolutionGridSize = 3;    // Tile GridSize
    public const int MidResolutionGridSize  = 66;   // Tile GridSize
    public const int HighResolutionGridSize = 66;   // Tile GridSize
    public const int DemoResolutionGridSize = 5;    // Tile GridSize
    public const int TileValueRange = 32768;        // 0 to 32767

    public const int MidResolutionLevel  = 16; // MidResolutionLevel to HighResolutionLevel-1
    public const int HighResolutionLevel = 20; // HighResolutionLevel to max

    public const int NoGridSize = 0;
    public const float SealLevelElev = 330; // render at sealevel
    public const float DemoBaseHgt = 300;
    public const float Mederian = 300;

  }
}
