using System.IO;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.SVL
{
  public class NFFLineworkSmoothedPolyLineVertexEntity : NFFLineworkPolyLineVertexEntity
  {
    public double TrailingAzimuth;

    // smoothing coefficients
    public double Alpha;
    public double Beta;

    public NFFLineworkSmoothedPolyLineVertexEntity()
    {
      //fVertexRedundant = False;

      Alpha = Consts.NullDouble;
      Beta = Consts.NullDouble;
      TrailingAzimuth = Consts.NullDouble;
    }

    //  private fVertexRedundant: Boolean;

    public NFFLineworkSmoothedPolyLineVertexEntity(NFFLineworkEntity AParent,
    double AX, double AY, double AZ, double AChainage, double ATrailingAzimuth) : base(AParent, AX, AY, AZ, AChainage)
    {
      TrailingAzimuth = ATrailingAzimuth;
    }

    public new NFFLineworkSmoothedPolyLineVertexEntity Clone() 
    {
      var Result = new NFFLineworkSmoothedPolyLineVertexEntity();
      Result.Assign(this);
      return Result;
    }

    public override void Assign(NFFLineworkPolyLineVertexEntity VertexEntity)
    {
      base.Assign(VertexEntity);

      TrailingAzimuth = (VertexEntity as NFFLineworkSmoothedPolyLineVertexEntity).TrailingAzimuth;
      Alpha = (VertexEntity as NFFLineworkSmoothedPolyLineVertexEntity).Alpha;
      Beta = (VertexEntity as NFFLineworkSmoothedPolyLineVertexEntity).Beta;
    }

    // SaveToNFFStream/LoadFromNFFStream implement GENERIC save/load functionality
    // NOT NFF save/load functionality
  //  procedure SaveToStream(Stream : TStream);
  public override void LoadFromStream(BinaryReader reader)
  {
    base.LoadFromStream(reader);

    Alpha = reader.ReadDouble();
    Beta = reader.ReadDouble();

    Chainage = reader.ReadDouble();

    TrailingAzimuth = reader.ReadDouble();
  }

  // SwapCoefficients swaps the sens of direction through this vertex
    //  procedure SwapCoefficients;

    // Function IsSameAs(const Other : NFFLineworkSmoothedPolyLineVertexEntity) : Boolean;
  }
}
