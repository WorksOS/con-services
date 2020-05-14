using VSS.Productivity3D.Models.Models.Files;
using VSS.TRex.Geometry;

namespace VSS.TRex.Files.DXF
{
  public class PolyLineBoundary
  {
    public Fence Boundary = new Fence();
    public DXFLineWorkBoundaryType Type = DXFLineWorkBoundaryType.GenericBoundary;
    public string Name = "";
  }
}
