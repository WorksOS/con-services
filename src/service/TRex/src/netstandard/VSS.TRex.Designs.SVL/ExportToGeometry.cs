using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL.DXF;
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.Designs.SVL
{
  public class ExportToGeometry : SVLExporterBase
  {
    /// <summary>
    /// This is a global accumulator for vertices in a consecutive series of elements in an SVL alignment
    /// </summary>
    public AddVertexCallback Vertices = new AddVertexCallback();

    public List<AlignmentGeometryResponseLabel> Labels = new List<AlignmentGeometryResponseLabel>();

    public DesignProfilerRequestResult CalcResult { get; private set; }

    private void ExportNFFSmoothedPolyLineEntityToGeometry(NFFLineworkSmoothedPolyLineEntity data)
    {
      // Iterate over each pair of points decomposing each interval in turn
      var StartPt = data.Vertices.First();

      for (var I = 0; I < data.Vertices.Count; I++)
      {
        var EndPt = data.Vertices[I];

        NFFUtils.DecomposeSmoothPolyLineSegmentToPolyLine(StartPt, EndPt,
          1.0 /* Min length*/, 100 /*Max segment length */, 1000 /*Max number of segments*/,
          Vertices.AddVertex);

        StartPt = EndPt; // Swap
      }
    }

    private void AddEntityToDXF(NFFLineworkEntity nffEntity)
    {
      switch (nffEntity.ElementType)
      {
        case NFFLineWorkElementType.kNFFLineWorkPolyLineElement:
          var nffPolyLine = nffEntity as NFFLineworkPolyLineEntity;

          for (var PtIdx = 0; PtIdx < nffPolyLine.Vertices.Count; PtIdx++)
          {
            Vertices.AddVertex(nffPolyLine.Vertices[PtIdx].X, 
                               nffPolyLine.Vertices[PtIdx].Y, 
                               nffPolyLine.Vertices[PtIdx].Z,
                               nffPolyLine.Vertices[PtIdx].Chainage,
                               DecompositionVertexLocation.Intermediate);
          }
          break;

        case NFFLineWorkElementType.kNFFLineWorkSmoothedPolyLineElement:
          ExportNFFSmoothedPolyLineEntityToGeometry((NFFLineworkSmoothedPolyLineEntity)nffEntity);
          break;

        case NFFLineWorkElementType.kNFFLineWorkArcElement:
          /* TODO: Implement arcs in SVL alignment geometry
          var nffArc = nffEntity as NFFLineworkArcEntity;
          double cz;

          if (nffArc.Z1 == Consts.NullDouble || nffArc.Z2 == Consts.NullDouble)
            cz = Consts.NullDouble;
          else
            cz = (nffArc.Z1 + nffArc.Z2) / 2;

          Entities.Add(new DXFArcEntity(
            nffArc.X1, nffArc.Y1, nffArc.Z1, nffArc.X2, nffArc.Y2, nffArc.Z2, nffArc.CX, nffArc.CY, cz));
          */   
          break;
      }
    }

    public bool ConstructSVLCenterlineAlignmentGeometry(NFFGuidableAlignmentEntity alignment)
    {
      if ((CalcResult = Validate(alignment)) != DesignProfilerRequestResult.OK)
        return false;

      // Run through the entities in the alignment and add them to the DXF file
      for (var I = 0; I < alignment.Entities.Count; I++)
        AddEntityToDXF(alignment.Entities[I]);

      // Decorate the vertices that 

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

      CalcResult = DesignProfilerRequestResult.OK;
      return true;
    }
  }
}
