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
    public DistanceUnitsType Units { get; set; } = DistanceUnitsType.metres;
    public double AlignmentLabelingInterval { get; set; } = 10; // Default to 10 meters

    private const int kAlignmentCenterLineColor = 1; // Red
    private const int kAlignmentCenterLineThickness = 2;

    private DXFFile DXF;

    private void ExportNFFSmoothedPolyLineEntityToDXF(NFFLineworkSmoothedPolyLineEntity Data)
    {
      // Iterate over each pair of points
      var StartPt = Data.Vertices.First();
      var vertices = new AddVertexCallback();

      for (int I = 0; I < Data.Vertices.Count; I++)
      {
        var EndPt = Data.Vertices[I];

        vertices.VertexCount = 0;
        NFFUtils.DecomposeSmoothPolylineSegmentToPolyLine(StartPt, EndPt,
          1.0 /* Min length*/, 100 /*Max segment length */, 1000 /*Max number of segments*/,
          vertices.AddVertex);
        if (vertices.VertexCount > 2)
        {
          var DXFPolyline = new DXFPolyLineEntity("B", kAlignmentCenterLineColor, kAlignmentCenterLineThickness);
          DXFPolyline.Closed = false;
          DXF.Entities.Add(DXFPolyline);
          for (int PtIdx = 0; PtIdx < vertices.VertexCount - 1; PtIdx++)
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

    private void AddEntityToDXF(NFFLineworkEntity NFFEntity)
    {
      switch (NFFEntity.ElementType)
      {
        case NFFLineWorkElementType.kNFFLineWorkLineElement:
          var lineEntity = NFFEntity as NFFLineworkEntity;
          // TODO : Not yet supported DXF.Entities.Add(new DXFLineEntity("B", kAlignmentCenterLineColor, lineEntity. X1, Y1, Z1, X2, Y2, Z2, kAlignmentCenterLineThickness));
          break;

        case NFFLineWorkElementType.kNFFLineWorkPolyLineElement:
        case NFFLineWorkElementType.kNFFLineWorkPolygonElement:
          var nffPolyLine = NFFEntity as NFFLineworkPolyLineEntity;
          var DXFPolyline = new DXFPolyLineEntity("B", kAlignmentCenterLineColor, kAlignmentCenterLineThickness);

          DXFPolyline.Closed = NFFEntity.ElementType == NFFLineWorkElementType.kNFFLineWorkPolygonElement;
          DXF.Entities.Add(DXFPolyline);

          for (int PtIdx = 0; PtIdx < nffPolyLine.Vertices.Count - 1; PtIdx++)
            DXFPolyline.Entities.Add(new DXFLineEntity("B", kAlignmentCenterLineColor,
              nffPolyLine.Vertices[PtIdx].X, nffPolyLine.Vertices[PtIdx].Y, nffPolyLine.Vertices[PtIdx].Z,
              nffPolyLine.Vertices[PtIdx + 1].X, nffPolyLine.Vertices[PtIdx + 1].Y, nffPolyLine.Vertices[PtIdx + 1].Z,
              kAlignmentCenterLineThickness));
          break;

        case NFFLineWorkElementType.kNFFLineWorkSmoothedPolyLineElement:
          ExportNFFSmoothedPolyLineEntityToDXF((NFFLineworkSmoothedPolyLineEntity)NFFEntity);
          break;

        case NFFLineWorkElementType.kNFFLineWorkArcElement:
          var nffArc = NFFEntity as NFFLineworkArcEntity;
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

    double AzimuthAt(NFFGuidableAlignmentEntity Alignment, double Stn)
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
        GeometryUtils.RectToPolar(Y1, X1, Y2, X2, out double result, out _);
        return result;
      }

      return Consts.NullDouble;
    }

    public bool ConstructSVLCenterlineDXFAlignment(NFFGuidableAlignmentEntity Alignment,
      out DesignProfilerRequestResult CalcResult, out MemoryStream MS)
    {
      // Todo InterlockedIncrement64(DesignProfilerRequestStats.NumAlignmentCenterlinesComputed);
      MS = null;
      CalcResult = DesignProfilerRequestResult.UnknownError;

      DXF = new DXFFile();

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
      double StationIncrement = AlignmentLabelingInterval;
      double CurrentStation = Alignment.StartStation;
      while (CurrentStation <= Alignment.EndStation + 0.001)
      {
        Alignment.ComputeXY(CurrentStation, 0, out double X, out double Y);
        var Orientation = AzimuthAt(Alignment, CurrentStation);

        DXF.Entities.Add(new DXFTextEntity("B",
          kAlignmentCenterLineColor,
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
