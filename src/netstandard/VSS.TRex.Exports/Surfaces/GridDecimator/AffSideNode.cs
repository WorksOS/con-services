using VSS.TRex.Designs.TTM;

namespace VSS.TRex.Exports.Surfaces.GridDecimator
{
  public struct AffSideNode
  {
    public TriVertex point; // the start point of the polygon side
    public Triangle tri; //affected triangle that the side belongs to 

    public Triangle side; // triangle on the other side of the line 

    public int Next; // Index of next AffSideNode element in array
  }
}
