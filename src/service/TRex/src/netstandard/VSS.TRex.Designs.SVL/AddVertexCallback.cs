using System.Collections.Generic;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.Designs.SVL
{
  public class AddVertexCallback
  {
    public List<AlignmentGeometryVertex> Vertices { get; } = new List<AlignmentGeometryVertex>();
    public int VertexCount { get; set; }

    private void Add(AlignmentGeometryVertex vertex)
    {
      if (VertexCount == 0 || !Vertices[VertexCount - 1].Equals(vertex))
      {
        if (VertexCount >= Vertices.Count)
        {
          Vertices.Add(vertex);
        }
        else
        {
          Vertices[VertexCount] = vertex;
        }

        VertexCount++;
      }
    }

    public void AddVertex(double vx, double vy, DecompositionVertexLocation vertexLocation)
    {
      Add(new AlignmentGeometryVertex(vx, vy, Consts.NullDouble, Consts.NullDouble));
    }

    public void AddVertex(double vx, double vy, double vz, double vstn, DecompositionVertexLocation vertexLocation)
    {
      Add(new AlignmentGeometryVertex(vx, vy, vz, vstn));
    }

    public void FillInStationValues()
    {
      // Fill in the station values for intermediate vertices.
      // For any vertices lacking stationing, compute it by determining the bracketing points with known stationing
      // and distributing the traversed station range across the sum of the traversed geometric distances between all
      // intermediary vertices

      var index = 0;
      var prevKnownStationIndex = -1;
      var thisKnownStationIndex = -1;
      var geometryTraversed = Consts.NullDouble;

        while (index < VertexCount)
        {
          if (Vertices[index].Station != Consts.NullDouble)
          {
            if (prevKnownStationIndex == -1)
              prevKnownStationIndex = index;
            else if (thisKnownStationIndex == -1)
              thisKnownStationIndex = index;
          }

          if (prevKnownStationIndex != -1)
          {
            if (geometryTraversed == Consts.NullDouble)
            {
              geometryTraversed = 0.0;
            }
            else
            {
              var prevVertex = Vertices[index - 1];
              var thisVertex = Vertices[index];
              geometryTraversed += MathUtilities.Hypot(thisVertex.X - prevVertex.X, thisVertex.Y - prevVertex.Y);
            }
          }

          if (prevKnownStationIndex != -1 && thisKnownStationIndex != -1 && thisKnownStationIndex - prevKnownStationIndex > 1)
          {
            var stationTraversed = Vertices[thisKnownStationIndex].Station - Vertices[prevKnownStationIndex].Station;

            for (var i = prevKnownStationIndex + 1; i < thisKnownStationIndex - 1; i++)
            {
              var prevVertex = Vertices[i - 1];
              var thisVertex = Vertices[i];
              var thisGeometryTraversed = MathUtilities.Hypot(thisVertex.X - prevVertex.X, thisVertex.Y - prevVertex.Y);

              thisVertex.Station = prevVertex.Station + (thisGeometryTraversed / geometryTraversed) * stationTraversed;
            }

            prevKnownStationIndex = thisKnownStationIndex;
            thisKnownStationIndex = -1;
            geometryTraversed = Consts.NullDouble;
          }

          index++;
        }
    }
  }
}
