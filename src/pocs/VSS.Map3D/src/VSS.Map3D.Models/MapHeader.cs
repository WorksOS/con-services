using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.Map3D.Models
{
  public struct MapHeader
  {
    /// <summary>
    /// Full path and file name to source if required
    /// </summary>
    public string FullName;
    /// <summary>
    /// Size of a squared grid of vertices
    /// </summary>
    public int GridSize;
    /// <summary>
    /// Size of a tile width. West to East
    /// </summary>
    public int Width;
    /// <summary>
    /// Size of a tile height. North to South
    /// </summary>
    public int Height;
    public float MinElevation;
    public float MaxElevation;

  }
}
