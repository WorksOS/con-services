using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.Designs.SVL.DXF
{
  public class DXFPolyLineEntity : DXFEntity
  {
    public bool Closed { get; set; }

    // public bool IsDXFSolid { get; set; }
    public int Thickness { get; set; }
    public List<DXFEntity> Entities { get; private set; } = new List<DXFEntity>();

    public DXFPolyLineEntity(string layer, byte colour, int thickness) : base(layer, colour)
    {
      Thickness = thickness;
    }

    public override bool Is3D()
    {
      if (Entities.Count == 0)
        // If polyline doesn't have any entities default to 2-D
        return false;

      foreach (var entity in Entities)
        if (!entity.Is3D())
          return false;

      return true;
    }

    protected void SaveAsPolyLine(StreamWriter writer, DistanceUnitsType OutputUnits)
    {
      if (Entities.Count == 0)
        return;

      // Get the height of the first vertex in the polyline. If the polyline is not
      // to be written out as a 3D polyline, this height will be the height of all the
      // vertices in the polyline.
      var PolyLineHeight = Entities[0].GetInitialHeight();

      DXFUtils.WriteDXFRecord(writer, 0, "POLYLINE");

      base.SaveToFile(writer, OutputUnits);

      DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFThicknessID, Thickness.ToString());

      DXFUtils.WriteDXFRecord(writer, 6, "CONTINUOUS");

      var PolylineFlags = 0;
      if (Closed)
        PolylineFlags |= 0x01;

      // Determine if the polyline contains any arc entities (ie: intervals with 'bulges')
      var HasArcs = false;
      foreach (var entity in Entities)
      {
        if (entity is DXFArcEntity)
        {
          HasArcs = true;
          break;
        }
      }

      var Output3DVertices = Is3D();

      if (HasArcs && PolyLineHeight != Consts.NullDouble)
        // We have to write out a 30 record to specify the
        // height of all the entities in the polyline.
      {
        DXFUtils.WriteDXFRecord(writer, 30, DXFUtils.NoLocaleFloatToStrF(DXFUtils.DXFDistance(PolyLineHeight, OutputUnits), 6));
        Output3DVertices = false;
      }

      if (Output3DVertices)
        PolylineFlags |= 0x08;
      DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFPolyLineFlagsID, PolylineFlags.ToString());

      DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFEntitiesFollow, "1");
      DXFUtils.WriteXYZToDXF(writer, 0, 0, 0, Consts.NullDouble, OutputUnits);

      for (int I = 0; I < Entities.Count; I++)
      {
        DXFUtils.WriteDXFRecord(writer, 0, "VERTEX");
        DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFLayerNameID, DXFUtils.DXFiseLayerName(Layer));

        if (Entities[I] is DXFLineEntity lineEntity)
          DXFUtils.WriteXYZToDXF(writer, 0, lineEntity.X1, lineEntity.Y1, HasArcs ? Consts.NullDouble : lineEntity.Z1, OutputUnits);

        if (Entities[I] is DXFArcEntity arcEntity)
        {
          DXFUtils.WriteXYZToDXF(writer, 0, arcEntity.X1, arcEntity.Y1, Consts.NullDouble, OutputUnits);

          // Write out the bulge for the arc
          var IncAngle = ArcUtils.CalcIncludedAngle(arcEntity.X1, arcEntity.Y1, arcEntity.X2, arcEntity.Y2, arcEntity.CX, arcEntity.CY, arcEntity.Clockwise);
          var Bulge = Math.Tan(IncAngle / 4);
          DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFArcBulgeID, DXFUtils.NoLocaleFloatToStrF(Bulge, 6));
        }

        if (Output3DVertices)
          DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFVertexFlagsID, "32");
      }

      // Write out last vertex
      DXFUtils.WriteDXFRecord(writer, 0, "VERTEX");

      DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFLayerNameID, DXFUtils.DXFiseLayerName(Layer));
      if (Entities[Entities.Count - 1] is DXFArcEntity arcEntityEnd)
        DXFUtils.WriteXYZToDXF(writer, 0, arcEntityEnd.X2, arcEntityEnd.Y2, Consts.NullDouble, OutputUnits);
      else if (Entities[Entities.Count - 1] is DXFLineEntity lineEntity)
        DXFUtils.WriteXYZToDXF(writer, 0, lineEntity.X2, lineEntity.Y2, HasArcs ? Consts.NullDouble : lineEntity.Z2, OutputUnits);

      if (Output3DVertices)
        DXFUtils.WriteDXFRecord(writer, DXFConsts.DXFVertexFlagsID, "32");

      DXFUtils.WriteDXFRecord(writer, 0, "SEQEND");
    }

    //    Procedure SaveAsSolid(var F : MediaTypeNames.Text;
    //    OutputUnits : DistanceUnitsType);

    public override void SaveToFile(StreamWriter writer, DistanceUnitsType OutputUnits)
    {
      if (Entities.Count == 0)
        return;

//      if (IsDXFSolid)
//      SaveAsSolid(writer, OutputUnits)
//      else
      SaveAsPolyLine(writer, OutputUnits);
    }
//procedure CalculateExtents(var EMinX, EMinY, EMinZ, EMaxX, EMaxY, EMaxZ : Double); Override;

    public override DXFEntityTypes EntityType() => DXFEntityTypes.detPolyLine;

//Procedure ConvertTo2D; Override;
//function EndpointsAreCoincident: Boolean;
  }
}
