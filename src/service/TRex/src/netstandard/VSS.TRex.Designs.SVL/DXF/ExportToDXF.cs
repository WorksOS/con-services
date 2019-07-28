using System;
using System.IO;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.Designs.SVL.DXF
{
  public class ExportToDXF
  {
    public distance_units_type Units { get; set; } = distance_units_type.metres;
    public double AlignmentLabellingInterval { get; set; } = 10; // Default to 10 meters

    private const int kAlignmentCenterlineColor = 1; // Red
    private const int kAlignmentCenterlineThickness = 2;

    private void ExportNFFSmoothedPolylineEntityToDXF(NFFLineworkSmoothedPolyLineEntity Data,
      DXFFile DXF)
    {
      // Iterate over each pair of points
      var StartPt = Data.Vertices.First();
      var vertices = new AddVertexCallbackClass();

      for (int I = 0; I < Data.Vertices.Count; I++)
      {
        var EndPt = Data.Vertices[I];

        vertices.VertexCount = 0;
        NFFUtils.DecomposeSmoothPolylineSegmentToPolyLine(StartPt, EndPt,
          1.0 /* Min length*/, 100 /*Max segment length */, 1000 /*Max number of segments*/,
          vertices.AddVertex);
        if (vertices.VertexCount > 2)
        {
          var DXFPolyline = new DXFPolyLineEntity("B", kAlignmentCenterlineColor, kAlignmentCenterlineThickness);
          DXFPolyline.Closed = false;
          DXF.Entities.Add(DXFPolyline);
          for (int PtIdx = 0; PtIdx < vertices.VertexCount - 1; PtIdx++)
            DXFPolyline.Entities.Add(new DXFLineEntity("B", kAlignmentCenterlineColor,
              vertices.Vertices[PtIdx].X,
              vertices.Vertices[PtIdx].Y,
              Consts.NullDouble,
              vertices.Vertices[PtIdx + 1].X,
              vertices.Vertices[PtIdx + 1].Y,
              Consts.NullDouble,
              kAlignmentCenterlineThickness));
        }
        else // Render a straight line for the curve
        {
          DXF.Entities.Add(new DXFLineEntity("B", kAlignmentCenterlineColor,
            StartPt.X, StartPt.Y, Consts.NullDouble,
            EndPt.X, EndPt.Y, Consts.NullDouble,
            kAlignmentCenterlineThickness));
        }

        // Swap
        StartPt = EndPt;
      }
    }


    public bool ConstructSVLCenterlineDXFAlignment(NFFGuidableAlignmentEntity Alignment,
      out DesignProfilerRequestResult CalcResult, out MemoryStream MS)
    {
      DXFFile DXF = new DXFFile();

      void AddEntityToDXF(NFFLineworkEntity NFFEntity)
      {
        switch (NFFEntity.ElementType)
        {
          case NFFLineWorkElementType.kNFFLineWorkLineElement:
            var lineEntity = NFFEntity as NFFLineworkEntity;
            // TODO : Not yet supported DXF.Entities.Add(new DXFLineEntity("B", kAlignmentCenterlineColor, lineEntity. X1, Y1, Z1, X2, Y2, Z2, kAlignmentCenterlineThickness));
            break;

          case NFFLineWorkElementType.kNFFLineWorkPolyLineElement:
          case NFFLineWorkElementType.kNFFLineWorkPolygonElement:
            var nffPolyLine = NFFEntity as NFFLineworkPolyLineEntity;
            var DXFPolyline = new DXFPolyLineEntity("B", kAlignmentCenterlineColor, kAlignmentCenterlineThickness);
            DXFPolyline.Closed = NFFEntity.ElementType == NFFLineWorkElementType.kNFFLineWorkPolygonElement;
            DXF.Entities.Add(DXFPolyline);
            for (int PtIdx = 0; PtIdx < nffPolyLine.Vertices.Count - 1; PtIdx++)
              DXFPolyline.Entities.Add(new DXFLineEntity("B", kAlignmentCenterlineColor,
                nffPolyLine.Vertices[PtIdx].X, nffPolyLine.Vertices[PtIdx].Y, nffPolyLine.Vertices[PtIdx].Z,
                nffPolyLine.Vertices[PtIdx + 1].X, nffPolyLine.Vertices[PtIdx + 1].Y, nffPolyLine.Vertices[PtIdx + 1].Z,
                kAlignmentCenterlineThickness));
            break;

          case NFFLineWorkElementType.kNFFLineWorkSmoothedPolyLineElement:
            ExportNFFSmoothedPolylineEntityToDXF((NFFLineworkSmoothedPolyLineEntity) NFFEntity, DXF);
            break;

          case NFFLineWorkElementType.kNFFLineWorkArcElement:
            var nffArc = NFFEntity as NFFLineworkArcEntity;
            double cz;
            if (nffArc.Z1 == Consts.NullDouble || nffArc.Z2 == Consts.NullDouble)
              cz = Consts.NullDouble;
            else
              cz = (nffArc.Z1 + nffArc.Z2) / 2;

            DXF.Entities.Add(new DXFArcEntity("B", kAlignmentCenterlineColor,
              nffArc.X1, nffArc.Y1, nffArc.Z1, nffArc.X2, nffArc.Y2, nffArc.Z2, nffArc.CX, nffArc.CY, cz,
              true, false, false,
              kAlignmentCenterlineThickness));

            break;
        }
      }

      double AzimuthAt(double Stn)
      {
        double TestStn1, TestStn2;

        if (Stn < Alignment.StartStation + 0.001)
          TestStn1 = Alignment.StartStation;
        else
          TestStn1 = Stn - 0.001;

        if (Stn > (Alignment.EndStation - 0.001))
          TestStn2 = Alignment.EndStation;
        else
          TestStn2 = Stn + 0.001;

        Alignment.ComputeXY(TestStn1, 0, out double X1, out double Y1);
        Alignment.ComputeXY(TestStn2, 0, out double X2, out double Y2);

        if (X1 != Consts.NullDouble && Y1 != Consts.NullDouble && X2 != Consts.NullDouble && Y2 != Consts.NullDouble)
        {
          GeometryUtils.rect_to_polar(Y1, X1, Y2, X2, out double result, out double Dist);
          return result;
        }

        return Consts.NullDouble;
      }

      // Todo InterlockedIncrement64(DesignProfilerRequestStats.NumAlignmentCenterlinesComputed);
      MS = null;
      CalcResult = DesignProfilerRequestResult.UnknownError;

      if (Alignment.Entities.Count == 0)
      {
        CalcResult = DesignProfilerRequestResult.AlignmentContainsNoElements;
        return false;
      }

      if (Alignment.StartStation == Consts.NullDouble || Alignment.EndStation == Consts.NullDouble)
      {
        CalcResult = DesignProfilerRequestResult.AlignmentContainsNoStationing;
        return false;
      }

      if (Alignment.StartStation >= Alignment.EndStation)
      {
        CalcResult = DesignProfilerRequestResult.AlignmentContainsInvalidStationing;
        return false;
      }

      DXF = new DXFFile();
      DXF.Layers.Add("B");

      // Run through the entities in the alignment and add them to the DXF file
      for (int I = 0; I < Alignment.Entities.Count; I++)
        AddEntityToDXF(Alignment.Entities[I]);

      // Construct the stationing text entities along the alignment
      double StationIncrement = AlignmentLabellingInterval;
      double CurrentStation = Alignment.StartStation;
      while (CurrentStation <= Alignment.EndStation + 0.001)
      {
        Alignment.ComputeXY(CurrentStation, 0, out double X, out double Y);
        double Orientation = AzimuthAt(CurrentStation);

        DXF.Entities.Add(new DXFTextEntity("B",
          kAlignmentCenterlineColor,
          X, Y, Consts.NullDouble,
          $"{CurrentStation / DXFUtils.DistToMetres(Units):F2}",
          Orientation - (Math.PI / 2),
          2,
          "Arial",
          //[],
          //0, 
          0, 0));

        if (CurrentStation + StationIncrement <= Alignment.EndStation)
          CurrentStation = CurrentStation + StationIncrement;
        else if (CurrentStation > Alignment.EndStation - 0.001)
          break;
        else
          CurrentStation = Alignment.EndStation;
      }

      if (DXF.Entities.Count > 0)
      {
        MS = new MemoryStream();
        using (var writer = new StreamWriter(MS))
        {
          DXF.SaveToFile(writer);
        }
      }

      CalcResult = DesignProfilerRequestResult.OK;

      return true;
    }
  }
}
