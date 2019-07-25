using System.IO;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFLineworkSmoothedPolyLineVertexEntity : TNFFLineworkPolyLineVertexEntity
  {
    public double TrailingAzimuth;

    // smoothing coefficients
    public double Alpha;
    public double Beta;

    public TNFFLineworkSmoothedPolyLineVertexEntity()
    {
      //fVertexRedundant = False;

      Alpha = Consts.NullDouble;
      Beta = Consts.NullDouble;
      TrailingAzimuth = Consts.NullDouble;
    }

    //  private fVertexRedundant: Boolean;

    public TNFFLineworkSmoothedPolyLineVertexEntity(TNFFLineworkEntity AParent,
    double AX, double AY, double AZ, double AChainage, double ATrailingAzimuth) : base(AParent, AX, AY, AZ, AChainage)
    {
      TrailingAzimuth = ATrailingAzimuth;
    }

    public TNFFLineworkSmoothedPolyLineVertexEntity Clone() 
    {
      var Result = new TNFFLineworkSmoothedPolyLineVertexEntity();
      Result.Assign(this);
      return Result;
    }

    public override void Assign(TNFFLineworkPolyLineVertexEntity VertexEntity)
    {
      base.Assign(VertexEntity);

      TrailingAzimuth = (VertexEntity as TNFFLineworkSmoothedPolyLineVertexEntity).TrailingAzimuth;
      Alpha = (VertexEntity as TNFFLineworkSmoothedPolyLineVertexEntity).Alpha;
      Beta = (VertexEntity as TNFFLineworkSmoothedPolyLineVertexEntity).Beta;
    }

    // SaveToNFFStream/LoadFromNFFStream implement GENERIC save/load functionality
    // NOT NFF save/load functionality
  //  procedure SaveToStream(Stream : TStream);
  public void LoadFromStream(BinaryReader reader)
  {
    base.LoadFromStream(reader);

    Alpha = reader.ReadDouble();
    Beta = reader.ReadDouble();

    Chainage = reader.ReadDouble();

    TrailingAzimuth = reader.ReadDouble();
  }

  // SwapCoefficients swaps the sens of direction through this vertex
    //  procedure SwapCoefficients;

    // Function IsSameAs(const Other : TNFFLineworkSmoothedPolyLineVertexEntity) : Boolean;
  }
}
