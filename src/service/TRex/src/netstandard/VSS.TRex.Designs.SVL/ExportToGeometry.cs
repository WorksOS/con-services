using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.Designs.SVL
{
  public class ExportToGeometry : SVLExporterBase
  {
    /// <summary>
    /// This is a global accumulator for vertices in sets of consecutive series of elements in an SVL alignment
    /// </summary>
    public AddVertexCallback WorkingVertices = new AddVertexCallback();

    /// <summary>
    /// Accumulator for labels created along the alignment
    /// </summary>
    public List<AlignmentGeometryResponseLabel> Labels = new List<AlignmentGeometryResponseLabel>();

    /// <summary>
    /// The collection of arc elements along the alignment
    /// </summary>
    public List<AlignmentGeometryResponseArc> Arcs = new List<AlignmentGeometryResponseArc>();

    /// <summary>
    /// The collection of all series of vertices
    /// </summary>
    public List<AddVertexCallback> Vertices = new List<AddVertexCallback>();

    /// <summary>
    /// The overall calculation result from the operation
    /// </summary>
    public DesignProfilerRequestResult CalcResult { get; private set; }

    private void MoveWorkingVerticesToVertices()
    {
      if (WorkingVertices.VertexCount == 0)
        return;

      Vertices.Add(WorkingVertices);
      WorkingVertices = new AddVertexCallback();
    }

    private void ExportNFFSmoothedPolyLineEntityToGeometry(NFFLineworkSmoothedPolyLineEntity data)
    {
      // Iterate over each pair of points decomposing each interval in turn
      var StartPt = data.Vertices.First();

      for (var I = 0; I < data.Vertices.Count; I++)
      {
        var EndPt = data.Vertices[I];

        NFFUtils.DecomposeSmoothPolyLineSegmentToPolyLine(StartPt, EndPt,
          1.0 /* Min length*/, 100 /*Max segment length */, 1000 /*Max number of segments*/,
          WorkingVertices.AddVertex);

        StartPt = EndPt; // Swap
      }
    }

    private void AddEntityToGeometry(NFFLineworkEntity nffEntity)
    {
      switch (nffEntity.ElementType)
      {
        case NFFLineWorkElementType.kNFFLineWorkPolyLineElement:
          var nffPolyLine = nffEntity as NFFLineworkPolyLineEntity;

          for (var PtIdx = 0; PtIdx < nffPolyLine.Vertices.Count; PtIdx++)
          {
            WorkingVertices.AddVertex(nffPolyLine.Vertices[PtIdx].X,
              nffPolyLine.Vertices[PtIdx].Y,
              nffPolyLine.Vertices[PtIdx].Z,
              nffPolyLine.Vertices[PtIdx].Chainage,
              DecompositionVertexLocation.Intermediate);
          }

          break;

        case NFFLineWorkElementType.kNFFLineWorkSmoothedPolyLineElement:
          ExportNFFSmoothedPolyLineEntityToGeometry((NFFLineworkSmoothedPolyLineEntity) nffEntity);
          break;

        case NFFLineWorkElementType.kNFFLineWorkArcElement:
          MoveWorkingVerticesToVertices();

          var nffArc = nffEntity as NFFLineworkArcEntity;
          double cz;

          if (nffArc.Z1 == Consts.NullDouble || nffArc.Z2 == Consts.NullDouble)
            cz = Consts.NullDouble;
          else
            cz = (nffArc.Z1 + nffArc.Z2) / 2;

          Arcs.Add(new AlignmentGeometryResponseArc(
            nffArc.X1, nffArc.Y1, nffArc.Z1,
            nffArc.X2, nffArc.Y2, nffArc.Z2,
            nffArc.CX, nffArc.CY, cz,
            nffArc.WasClockWise));
          break;
      }
    }

    public bool ConstructSVLCenterlineAlignmentGeometry(NFFGuidableAlignmentEntity alignment)
    {
      if ((CalcResult = Validate(alignment)) != DesignProfilerRequestResult.OK)
        return false;

      // Run through the entities in the alignment and add them to the geometry
      for (var I = 0; I < alignment.Entities.Count; I++)
        AddEntityToGeometry(alignment.Entities[I]);
      MoveWorkingVerticesToVertices();

      // Construct the stationing text entities along the alignment
      var StationIncrement = AlignmentLabelingInterval;
      var CurrentStation = alignment.StartStation;
      while (CurrentStation <= alignment.EndStation + 0.001)
      {
        alignment.ComputeXY(CurrentStation, 0, out var X, out var Y);
        var Orientation = AzimuthAt(alignment, CurrentStation);

        // Create an instance of the response label with the lat/lon coordinate set to the Y/X grid coordinates
        // which will be converted later by the caller
        Labels.Add(new AlignmentGeometryResponseLabel(CurrentStation, Y, X, Orientation - Math.PI / 2));

        if (CurrentStation + StationIncrement <= alignment.EndStation)
          CurrentStation = CurrentStation + StationIncrement;
        else if (CurrentStation > alignment.EndStation - 0.001)
          break;
        else
          CurrentStation = alignment.EndStation;
      }

      foreach (var vertices in Vertices)
        vertices.FillInStationValues();

      CalcResult = DesignProfilerRequestResult.OK;
      return true;
    }
  }
}
