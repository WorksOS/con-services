using System;
using System.IO;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL.DXF
{
  public class ExportToDXF : SVLExporterBase
  {
    private DXFFile DXF;

    private void ExportNFFSmoothedPolyLineEntityToDXF(NFFLineworkSmoothedPolyLineEntity Data)
    {
      // Iterate over each pair of points
      var StartPt = Data.Vertices.First();
      var vertices = new AddVertexCallback();

      for (var I = 0; I < Data.Vertices.Count; I++)
      {
        var EndPt = Data.Vertices[I];

        vertices.VertexCount = 0;
        NFFUtils.DecomposeSmoothPolyLineSegmentToPolyLine(StartPt, EndPt,
          1.0 /* Min length*/, 100 /*Max segment length */, 1000 /*Max number of segments*/,
          vertices.AddVertex);
        if (vertices.VertexCount > 2)
        {
          var DXFPolyline = new DXFPolyLineEntity("B", kAlignmentCenterLineColor, kAlignmentCenterLineThickness)
          {
            Closed = false
          };
          DXF.Entities.Add(DXFPolyline);
          for (var PtIdx = 0; PtIdx < vertices.VertexCount - 1; PtIdx++)
            DXFPolyline.Entities.Add(new DXFLineEntity("B", kAlignmentCenterLineColor,
              vertices.Vertices[PtIdx].X,
              vertices.Vertices[PtIdx].Y,
              Consts.NullDouble,
              vertices.Vertices[PtIdx + 1].X,
              vertices.Vertices[PtIdx + 1].Y,
              Consts.NullDouble,
              kAlignmentCenterLineThickness));
        }
        else // Render a straight line for the curve
        {
          DXF.Entities.Add(new DXFLineEntity("B", kAlignmentCenterLineColor,
            StartPt.X, StartPt.Y, Consts.NullDouble,
            EndPt.X, EndPt.Y, Consts.NullDouble,
            kAlignmentCenterLineThickness));
        }

        // Swap
        StartPt = EndPt;
      }
    }

    private void AddEntityToDXF(NFFLineworkEntity nffEntity)
    {
      switch (nffEntity.ElementType)
      {
        case NFFLineWorkElementType.kNFFLineWorkLineElement:
          var lineEntity = nffEntity as NFFLineworkEntity;
          // TODO : Not yet supported DXF.Entities.Add(new DXFLineEntity("B", kAlignmentCenterLineColor, lineEntity. X1, Y1, Z1, X2, Y2, Z2, kAlignmentCenterLineThickness));
          break;

        case NFFLineWorkElementType.kNFFLineWorkPolyLineElement:
        case NFFLineWorkElementType.kNFFLineWorkPolygonElement:
          var nffPolyLine = nffEntity as NFFLineworkPolyLineEntity;
          var DXFPolyline = new DXFPolyLineEntity("B", kAlignmentCenterLineColor, kAlignmentCenterLineThickness);

          DXFPolyline.Closed = nffEntity.ElementType == NFFLineWorkElementType.kNFFLineWorkPolygonElement;
          DXF.Entities.Add(DXFPolyline);

          for (var PtIdx = 0; PtIdx < nffPolyLine.Vertices.Count - 1; PtIdx++)
            DXFPolyline.Entities.Add(new DXFLineEntity("B", kAlignmentCenterLineColor,
              nffPolyLine.Vertices[PtIdx].X, nffPolyLine.Vertices[PtIdx].Y, nffPolyLine.Vertices[PtIdx].Z,
              nffPolyLine.Vertices[PtIdx + 1].X, nffPolyLine.Vertices[PtIdx + 1].Y, nffPolyLine.Vertices[PtIdx + 1].Z,
              kAlignmentCenterLineThickness));
          break;

        case NFFLineWorkElementType.kNFFLineWorkSmoothedPolyLineElement:
          ExportNFFSmoothedPolyLineEntityToDXF((NFFLineworkSmoothedPolyLineEntity)nffEntity);
          break;

        case NFFLineWorkElementType.kNFFLineWorkArcElement:
          var nffArc = nffEntity as NFFLineworkArcEntity;
          double cz;

          if (nffArc.Z1 == Consts.NullDouble || nffArc.Z2 == Consts.NullDouble)
            cz = Consts.NullDouble;
          else
            cz = (nffArc.Z1 + nffArc.Z2) / 2;

          DXF.Entities.Add(new DXFArcEntity("B", kAlignmentCenterLineColor,
            nffArc.X1, nffArc.Y1, nffArc.Z1, nffArc.X2, nffArc.Y2, nffArc.Z2, nffArc.CX, nffArc.CY, cz,
            true, false, false,
            kAlignmentCenterLineThickness));

          break;
      }
    }

    public bool ConstructSVLCenterlineDXFAlignment(NFFGuidableAlignmentEntity alignment,
      out DesignProfilerRequestResult calcResult, out MemoryStream ms)
    {
      ms = null;
      if ((calcResult = Validate(alignment)) != DesignProfilerRequestResult.OK)
        return false;

      DXF = new DXFFile();
      DXF.Layers.Add("B");

      // Run through the entities in the alignment and add them to the DXF file
      for (var I = 0; I < alignment.Entities.Count; I++)
        AddEntityToDXF(alignment.Entities[I]);

      // Construct the stationing text entities along the alignment
      var StationIncrement = AlignmentLabelingInterval;
      var CurrentStation = alignment.StartStation;
      while (CurrentStation <= alignment.EndStation + 0.001)
      {
        alignment.ComputeXY(CurrentStation, 0, out var X, out var Y);
        var Orientation = AzimuthAt(alignment, CurrentStation);

        DXF.Entities.Add(new DXFTextEntity("B",
          kAlignmentCenterLineColor,
          X, Y, Consts.NullDouble,
          $"{CurrentStation / UnitUtils.DistToMeters(Units):F2}",
          Orientation - (Math.PI / 2),
          2,
          "Arial",
          //[],
          //0, 
          0, 0));

        if (CurrentStation + StationIncrement <= alignment.EndStation)
          CurrentStation = CurrentStation + StationIncrement;
        else if (CurrentStation > alignment.EndStation - 0.001)
          break;
        else
          CurrentStation = alignment.EndStation;
      }

      if (DXF.Entities.Count > 0)
      {
        ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        DXF.SaveToFile(writer);
      }

      calcResult = DesignProfilerRequestResult.OK;

      return true;
    }
  }
}
