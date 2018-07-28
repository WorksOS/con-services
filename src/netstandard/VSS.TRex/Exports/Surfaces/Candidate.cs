using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Exports.Surfaces
{
  /// <summary>
  /// Defines a candidate location and elevation for inclusion into the TIN being decimated from the grid points
  /// </summary>
  public struct Candidate
  {
    public int X, Y;
    public double Z;
    public double Import;

    public Candidate(double import)
    {
      X = 0;
      Y = 0;
      Z = 0;
      Import = import;
    }

    public bool Consider(double import) => import > Import;

    public void Update(int sx, int sy, double sz, double import)
    {
      X = sx;
      Y = sy;
      Z = sz;
      Import = import;
    }
  }
}
