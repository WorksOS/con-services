using System.IO;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.SVL.DXF
{
  public class DXFLineEntity : DXFEntity
  {
    public double X1, Y1, Z1;
    public double X2, Y2, Z2;
    public int Thickness;

    public DXFLineEntity(string layer, int colour, double AX1, double AY1, double AZ1, double AX2, double AY2, double AZ2,
      int AThickness) : base(layer, colour)
    {
      X1 = AX1;
      Y1 = AY1;
      Z1 = AZ1;
      X2 = AX2;
      Y2 = AY2;
      Z2 = AZ2;
    }

    public override void SaveToFile(StreamWriter writer, DistanceUnitsType OutputUnits)
    {
      DXFUtils.WriteDXFRecord(writer, 0, "LINE");

      base.SaveToFile(writer, OutputUnits);

      DXFUtils.WriteXYZToDXF(writer, 0, X1, Y1, Z1, OutputUnits);
      DXFUtils.WriteXYZToDXF(writer, 1, X2, Y2, Z2, OutputUnits);

      DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFThicknessID, Thickness.ToString());

    }

    //   procedure CalculateExtents(var EMinX, EMinY, EMinZ, EMaxX, EMaxY, EMaxZ : Double); Override;
    public override DXFEntityTypes EntityType() => DXFEntityTypes.detLine;

    //  Procedure ConvertTo2D; Override;
    public override bool Is3D() => Z1 != Consts.NullDouble && Z2 != Consts.NullDouble;

    public override double GetInitialHeight() => Z1;
  }
}
