using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.Designs.SVL.DXF
{
  public class DXFArcEntity : DXFEntity
  {
    public double X1, Y1, Z1;
    public double X2, Y2, Z2;
    public double CX, CY, CZ;
    public int Thickness;
    public bool Clockwise;
    public bool Reversed;
    public bool SingleArcEdgePoint;

    public DXFArcEntity(string layer, int colour, double AX1, double AY1, double AZ1, double AX2, double AY2, double AZ2, double ACX, double ACY, double ACZ,
      bool AClockwise, bool AReversed, bool ASingleArcEdgePoint, int AThickness) : base(layer, colour)
    {
      X1 = AX1;
      Y1 = AY1;
      Z1 = AZ1;
      X2 = AX2;
      Y2 = AY2;
      Z2 = AZ2;
      CX = ACX;
      CY = ACY;
      CZ = ACZ;
      Clockwise = AClockwise;
      Reversed = AReversed;
      SingleArcEdgePoint = ASingleArcEdgePoint;
    }

    public override void SaveToFile(StreamWriter writer, DistanceUnitsType OutputUnits)
    {
//      var
//      Radius : Double;
      //    start_angle,
      //  end_angle: Double;
      //IncAngle: Double;
      //  ItsACircle: Boolean;

      GetStartEndAnglesRadius(out double start_angle, out double end_angle, out double radius);

      //  if (fClockwise and not fReversed) or (not fClockwise and fReversed) then
      //  if fClockwise then
      //    swap_f(start_angle, end_angle);

      double IncAngle = ArcUtils.CalcIncludedAngle(X1, Y1, X2, Y2, CX, CY, Clockwise);
      bool ItsACircle = (start_angle == end_angle) && SingleArcEdgePoint;

      if (ItsACircle)
        DXFUtils.WriteDXFRecord(writer, 0, "CIRCLE");
      else
        DXFUtils.WriteDXFRecord(writer, 0, "ARC");

      base.SaveToFile(writer, OutputUnits);

      DXFUtils.WriteXYZToDXF(writer, 0, CX, CY, CZ, OutputUnits);
      DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFArcRadiusID, DXFUtils.NoLocaleFloatToStrF(DXFUtils.DXFDistance(radius, OutputUnits), 6));

      if (!ItsACircle)
      {
        start_angle = start_angle / (Math.PI / 180);
        end_angle = end_angle / (Math.PI / 180);

        if (IncAngle < 0)
          MinMax.Swap(ref start_angle, ref end_angle);
        if (start_angle < 0)
          start_angle = start_angle + 360;
        if (end_angle < start_angle)
          end_angle = end_angle + 360;

        // Write the two angles to the DXF file
        DXFUtils.WriteDXFAngle(writer, DXFConsts.DXFArcStartAngleID, start_angle);
        DXFUtils.WriteDXFAngle(writer, DXFConsts.DXFArcEndAngleID, end_angle);
      }

      DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFThicknessID, Thickness.ToString());

    }
    //    procedure CalculateExtents(var EMinX, EMinY, EMinZ, EMaxX, EMaxY, EMaxZ : Double); Override;

    public override DXFEntityTypes EntityType() => DXFEntityTypes.detArc;

    // Procedure ConvertTo2D; Override;
    public override bool Is3D() => Z1 != Consts.NullDouble && Z2 != Consts.NullDouble && CZ != Consts.NullDouble;

    public void GetStartEndAnglesRadius(out double startAng, out double endAng, out double radius)
    {
      double relX = X1 - CX;
      double relY = Y1 - CY;
      double endX = X2 - CX;
      double endY = Y2 - CY;
      radius = MathUtilities.Hypot(relX, relY);
      startAng = Math.Atan2(relY, relX);
      endAng = Math.Atan2(endY, endX);
    }

    public override double GetInitialHeight() => Z1;
  }
}
