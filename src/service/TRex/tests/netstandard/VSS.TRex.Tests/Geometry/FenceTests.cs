using System.Collections.Generic;
using FluentAssertions;
using VSS.TRex.Geometry;
using VSS.TRex.Common;
using Xunit;

namespace VSS.TRex.Tests.Geometry
{
        public class FenceTests
    {
        private Fence makeSimpleRectangleFence()
        {
            Fence fence = new Fence();
            fence.SetRectangleFence(0, 0, 100, 100);

            return fence;
        }

        private Fence makeSimpleTriangleFence()
        {
          var fence = new Fence
          {
            Points = new List<FencePoint>
            {
              new FencePoint(0, 0),
              new FencePoint(0, 100),
              new FencePoint(100, 0)
            }
          };

          fence.UpdateExtents();

          return fence;
        }

        [Fact()]
        public void Test_FenceTests_Fence()
        {
            Fence fence = new Fence();

            Assert.False(fence.IsRectangle, "IsRectangle not initialised to false");
            Assert.False(fence.HasVertices, "New fence has vertices");
            Assert.True(fence.MinX > fence.MaxX && fence.MinY > fence.MaxY,
                "Fence extents are not reveresed");

            Fence fence2 = new Fence(0, 0, 1, 1);
            Assert.True(fence2.NumVertices == 4 && fence2.Area() == 1.0);
        }

        [Fact()]
        public void Test_FenceTests_Fence_FromBoundingExtent()
        {
          Fence fence = new Fence(new BoundingWorldExtent3D(100, 100, 200, 200));

          fence.IsRectangle.Should().BeTrue();
          fence.MinX.Should().Be(100);
          fence.MinY.Should().Be(100);
          fence.MaxX.Should().Be(200);
          fence.MaxY.Should().Be(200);
        }

        [Fact()]
        public void Test_FenceTests_IncludesPoint_Rectangular()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.True(fence.IncludesPoint(50, 50), "Point (50, 50) not included");
            Assert.False(fence.IncludesPoint(150, 150), "Point (150, 150) is included");
            Assert.False(fence.IncludesPoint(-1, 0), "Point (-1, 0) is included");
            Assert.True(fence.IncludesPoint(50, 100), "Point (50, 100) is not  included");
            Assert.True(fence.IncludesPoint(50, 0), "Point (50, 0) is not  included");
        }

        [Fact()]
        public void Test_FenceTests_IncludesPoint_Rectangular_AsFreeFormPolygon()
        {
          var fence = new Fence
          {
            Points = new List<FencePoint>
            {
              new FencePoint(0, 0),
              new FencePoint(0, 100),
              new FencePoint(100, 100),
              new FencePoint(100, 0)
            }
          };

          fence.IsRectangle = false;
          fence.UpdateExtents();
          
          Assert.True(fence.IncludesPoint(50, 50), "Point (50, 50) not included");
          Assert.False(fence.IncludesPoint(150, 150), "Point (150, 150) is included");
          Assert.False(fence.IncludesPoint(-1, 0), "Point (-1, 0) is included");
          Assert.True(fence.IncludesPoint(50, 100), "Point (50, 100) is not included");
        }

        [Fact()]
        public void Test_FenceTests_IncludesPoint_TooFewPoints()
        {
          var fence = new Fence()
          {
            Points = new List<FencePoint>
            {
              new FencePoint(0, 0),
              new FencePoint(100, 0)
            }
          };
          fence.UpdateExtents();

          fence.IncludesPoint(50, 0).Should().BeFalse("Because the fence has ony two vertices");
        }

        [Theory]
        [InlineData(new double[] { 0, 0, 0, 100, 100, 100, 100, 0 }, 50, 0 + 1E-100, true)] // This one is normal as a self test

        [InlineData(new double[] { 0, 0, 0, 0, 0, 0 }, 0, 0, true)]
        [InlineData(new double[] { 0, 0, 100, 0, 0, 0 }, 0, 0, true)]
        [InlineData(new double[] { 0, 0, 100, 0, 0, 0 }, 100, 0, true)]
      
        [InlineData(new double[] { 0, 0, 0, 0, 0, 0 }, 0, 0 + 1E-20, false)]
        [InlineData(new double[] { 0, 0, 100, 0, 0, 0 }, 0, 0 + 1E-20, false)]
        [InlineData(new double[] { 0, 0, 100, 0, 0, 0 }, 100, 0 + 1E-20, false)]
      
        [InlineData(new double[] { 0, 0, 0, 0, 0, 0 }, 0, 0 - 1E-20, false)]
        [InlineData(new double[] { 0, 0, 100, 0, 0, 0 }, 0, 0 - 1E-20, false)]
        [InlineData(new double[] { 0, 0, 100, 0, 0, 0 }, 100, 0 - 1E-20, false)]
      
        [InlineData(new double[] { 0, 0, 0, -1, 0, 0, 0, 0, 0, 0}, 0, 0 + 1E-20, false)]
        [InlineData(new double[] { 0, 0, 0, -1, 0, 0, 100, 0, 0, 0 }, 0, 0 + 1E-20, false)]
        [InlineData(new double[] { 0, 0, 0, -1, 0, 0, 100, 0, 0, 0 }, 100, 0 + 1E-20, false)]
        [InlineData(new double[] { 0, 0, 0, -1, 0, 0, 100, 0, 0, 0, 100, 0 }, 0, 0 + 1E-20, false)]
      
        [InlineData(new double[] { 0, 0, 0, -1, 0, 0, 0, 0, 0, 0 }, 0, 0 - 1E-20, false)]
        [InlineData(new double[] { 0, 0, 0, -1, 0, 0, 100, 0, 0, 0 }, 0, 0 - 1E-20, false)]
        [InlineData(new double[] { 0, 0, 0, -1, 0, 0, 100, 0, 0, 0 }, 100, 0 - 1E-20, false)]
        [InlineData(new double[] { 0, 0, 0, -1, 0, 0, 100, 0, 0, 0, 100, 0 }, 0, 0 - 1E-20, false)]

        public void Test_FenceTests_IncludesPoint_PathologicalCases(double [] coords, double probeX, double probeY, bool result)
        {
          var vertices = new List<FencePoint>();
          for (int i = 0; i < coords.Length / 2; i++)
             vertices.Add(new FencePoint(coords[i * 2], coords[i * 2 + 1]));

          var fence = new Fence
          {
            Points = vertices
          };
          fence.UpdateExtents();

          fence.IncludesPoint(probeX, probeY).Should().Be(result);
        }

        [Fact()]
        public void Test_FenceTests_IncludesPoint_Triangular()
        {
          var fence = makeSimpleTriangleFence();

          fence.IncludesPoint(-1, -1).Should().BeFalse();
          fence.IncludesPoint(100, 100).Should().BeFalse();

          // Vertex inclusions are a little ambiguous due to line scan intersection algorithm
          fence.IncludesPoint(0, 0).Should().BeTrue();
          fence.IncludesPoint(100, 0).Should().BeTrue();
          fence.IncludesPoint(0, 100).Should().BeTrue();

          fence.IncludesPoint(1E-10, 1E-10).Should().BeTrue();
          fence.IncludesPoint(100 - 1E-10, 1E-11).Should().BeTrue();
          fence.IncludesPoint(1E-11, 100 - 1E-10).Should().BeTrue();
        }

        [Fact()]
        public void Test_FenceTests_IncludesPoint_FailWithTooFewPoints()
        {
            var fence = new Fence();

            fence.Points.Add(new FencePoint(0, 0));
            fence.Points.Add(new FencePoint(100, 0));

            fence.IncludesPoint(10, 0).Should().BeFalse();
        }

        [Fact()]
        public void Test_FenceTests_IncludesLine()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.True(fence.IncludesLine(50, 50, 75, 75), "Line (50, 50, 75, 75) not included");
            Assert.True(fence.IncludesLine(50, 50, 175, 75), "Line (50, 50, 175, 75) not included");
            Assert.True(fence.IncludesLine(-100, -100, 200, 200), "Line (-100, -100, 200, 200) not included");
            Assert.False(fence.IncludesLine(-100, -100, 0, 200), "Line (-100, -100, 0, 200) included");
        }

        [Fact()]
        public void Test_FenceTests_IncludesExtent_Rectangular()
        {
          var fence = makeSimpleRectangleFence();

          fence.IncludesExtent(new BoundingWorldExtent3D(1, 1, 99, 99)).Should().BeTrue();
          fence.IncludesExtent(new BoundingWorldExtent3D(0, 0, 100, 100)).Should().BeTrue(); 

          fence.IncludesExtent(new BoundingWorldExtent3D(-1, -1, 101, 101)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(-1, -1, 1, 1)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(99, 99, 101, 101)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(-1, 99, 1, 101)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(99, -1, 101, 1)).Should().BeFalse();
        }

        [Fact()]
        public void Test_FenceTests_IncludesExtent_Triangular()
        {
          var fence = makeSimpleTriangleFence();

          fence.IncludesExtent(new BoundingWorldExtent3D(1, 1, 49, 49)).Should().BeTrue();
          fence.IncludesExtent(new BoundingWorldExtent3D(0, 0, 50, 50)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(0, 0, 50 - 1E-10, 50 - 1E-10)).Should().BeFalse();

          fence.IncludesExtent(new BoundingWorldExtent3D(50 + 1E-10, 50 + 1E-10, 100, 100)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(-1, -1, 101, 101)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(-1, -1, 1, 1)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(99, 99, 101, 101)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(-1, 99, 1, 101)).Should().BeFalse();
          fence.IncludesExtent(new BoundingWorldExtent3D(99, -1, 101, 1)).Should().BeFalse();
        }

        [Fact()]
        public void Test_FenceTests_IncludesExtent_ComplexBoundaryCross_NoPointInclusion()
        {
          // No fence vertices within the extent being tested
          var fence = new Fence
          {
            Points = new List<FencePoint>
            {
              new FencePoint(0, 0),
              new FencePoint(0, 100),
              new FencePoint(25, 100),
              new FencePoint(35, -100),
              new FencePoint(70, -100),
              new FencePoint(75, 100),
              new FencePoint(100, 100),
              new FencePoint(100, 0)
            }
          };

          fence.UpdateExtents();

          fence.IncludesExtent(new BoundingWorldExtent3D(10, 10, 90, 90)).Should().BeFalse();
        }

        [Fact()]
        public void Test_FenceTests_IncludesExtent_ComplexBoundaryCross_WithPointInclusion()
        {
          // With fence vertices within the extent being tested
          var fence = new Fence
          {
            Points = new List<FencePoint>
            {
              new FencePoint(0, 0),
              new FencePoint(0, 100),
              new FencePoint(25, 100),
              new FencePoint(35, 50),
              new FencePoint(70, 50),
              new FencePoint(75, 100),
              new FencePoint(100, 100),
              new FencePoint(100, 0)
            }
          };

          fence.UpdateExtents();

          fence.IncludesExtent(new BoundingWorldExtent3D(10, 10, 90, 90)).Should().BeFalse();
        }

        [Fact()]
        public void Test_FenceTests_IntersectsExtent_Rectanguler()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.True(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX, fence.MinY, fence.MaxX, fence.MaxY)), 
                          "Extents do not overlap");
            Assert.True(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX - 50, fence.MinY - 50, fence.MaxX - 50, fence.MaxY - 50)),
                          "Extents do not overlap");
            Assert.True(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX - 100, fence.MinY - 100, fence.MaxX - 100, fence.MaxY - 100)),
                          "Extents do not overlap");
            Assert.True(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX + 100, fence.MinY + 100, fence.MaxX + 100, fence.MaxY + 100)),
                          "Extents do not overlap");

            Assert.False(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX - 101, fence.MinY - 101, fence.MaxX - 101, fence.MaxY - 101)),
                           "Extents not overlap");
            Assert.False(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX + 101, fence.MinY + 101, fence.MaxX + 101, fence.MaxY + 101)),
                           "Extents not overlap");
        }

        
        [Fact()]
        public void Test_FenceTests_IntersectsExtent_Triangular()
        {
            Fence fence = makeSimpleTriangleFence();

            fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX, fence.MinY, fence.MaxX, fence.MaxY)).Should().BeTrue();

            fence.IntersectsExtent(new BoundingWorldExtent3D(50, 50, 100, 100)).Should().BeTrue();
            fence.IntersectsExtent(new BoundingWorldExtent3D(50.1, 50.1, 100, 100)).Should().BeFalse();

            fence.IntersectsExtent(new BoundingWorldExtent3D(-1000, -1000, 1000, 1000)).Should().BeTrue();
            fence.IntersectsExtent(new BoundingWorldExtent3D(0, 0, 1, 1)).Should().BeTrue();
            fence.IntersectsExtent(new BoundingWorldExtent3D(0, 49, 1, 51)).Should().BeTrue();
            fence.IntersectsExtent(new BoundingWorldExtent3D(49, 0, 51, 1)).Should().BeTrue();

            fence.IntersectsExtent(new BoundingWorldExtent3D(-100, 10, 100, 20)).Should().BeTrue();
        }

        [Fact()]
        public void Test_FenceTests_Initialise()
        {
            Fence fence = makeSimpleRectangleFence();
            fence.Initialise();

            Assert.False(fence.IsRectangle, "IsRectangle not initialised to false");
            Assert.False(fence.HasVertices, "New fence has vertices");
            Assert.True(fence.MinX > fence.MaxX && fence.MinY > fence.MaxY,
                "Fence extents are not reveresed");
        }

        [Fact()]
        public void Test_FenceTests_Clear()
        {
            Fence fence = makeSimpleRectangleFence();
            fence.Clear();

            Assert.False(fence.IsRectangle, "IsRectangle not initialised to false");
            Assert.False(fence.HasVertices, "New fence has vertices");
            Assert.True(fence.MinX > fence.MaxX && fence.MinY > fence.MaxY,
                "Fence extents are not reveresed");        
        }

        [Fact()]
        public void Test_FenceTests_IsSquare()
        {
            Fence fence = makeSimpleRectangleFence();
            Assert.True(fence.IsSquare, "Rectangle is not square");

            fence.Points[0].SetXY(-100, 100); // Not square any more
            fence.UpdateExtents();

            Assert.False(fence.IsSquare, "Rectangle is square");
        }

        [Fact()]
        public void Test_FenceTests_GetExtents()
        {
            Fence fence = makeSimpleRectangleFence();

            fence.GetExtents(out double minx, out double miny, out double maxx, out double maxy);

            Assert.True(minx == 0 && miny == 0 && maxx == 100 && maxy == 100,
                "Extracted extents are incorrect");
        }

        [Fact()]
        public void Test_FenceTests_SetExtents()
        {
            Fence fence = new Fence();
            fence.SetExtents(0, 0, 100, 100);

            Assert.Equal(4, fence.NumVertices);

            Assert.True(fence.Points[0].X == 0 && fence.Points[0].Y == 0 &&
                fence.Points[1].X == 0 && fence.Points[1].Y == 100 &&
                fence.Points[2].X == 100 && fence.Points[2].Y == 100 &&
                fence.Points[3].X == 100 && fence.Points[3].Y == 0,
                "Fence point vertices incorrect");

            Assert.True(fence.IsRectangle);
        }

        [Fact()]
        public void Test_FenceTests_HasVertices()
        {
            Fence fence = new Fence();

            Assert.False(fence.HasVertices, "HasVertices incorrect");

            fence.SetExtents(0, 0, 100, 100);

            Assert.True(fence.HasVertices && fence.NumVertices == 4, "HasVertices incorrect");
        }

        [Fact()]
        public void Test_FenceTests_NumVertices()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.Equal(4, fence.NumVertices);
        }

        [Fact()]
        public void Test_FenceTests_Area_NoPointsInFence()
        {
          Fence fence = new Fence();

          Assert.Equal(0, fence.Area());
        }

        [Fact()]
        public void Test_FenceTests_Area()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.Equal(10000, fence.Area());
        }

        [Fact()]
        public void Test_FenceTests_UpdateExtents()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.True(fence.MinX == 0 && fence.MinY == 0 && fence.MaxX == 100 && fence.MaxY == 100,
                "Extents are incorrect");
        }

        [Fact()]
        public void Test_FenceTests_Assign()
        {
            Fence fenceSource = makeSimpleRectangleFence();
            Fence fence = new Fence();

            fence.Assign(fenceSource);

            Assert.Equal(4, fence.NumVertices);

            Assert.True(fence.Points[0].X == 0 && fence.Points[0].Y == 0 &&
                fence.Points[1].X == 0 && fence.Points[1].Y == 100 &&
                fence.Points[2].X == 100 && fence.Points[2].Y == 100 &&
                fence.Points[3].X == 100 && fence.Points[3].Y == 0,
                "Fence point vertices incorrect");
        }

        [Fact()]
        public void Test_FenceTests_SetRectangleFence()
        {
            Fence fence = new Fence();
            fence.SetRectangleFence(0, 0, 100, 100);

            Assert.Equal(4, fence.NumVertices);

            Assert.True(fence.Points[0].X == 0 && fence.Points[0].Y == 0 &&
                fence.Points[1].X == 0 && fence.Points[1].Y == 100 &&
                fence.Points[2].X == 100 && fence.Points[2].Y == 100 &&
                fence.Points[3].X == 100 && fence.Points[3].Y == 0,
                "Fence point vertices incorrect");

            Assert.True(fence.IsRectangle);
        }

        [Fact()]
        public void Test_FenceTests_IsNull()
        {
            Fence fence = new Fence();
            fence.SetRectangleFence(0, 0, 100, 100);

            Assert.False(fence.IsNull(), "Fence is null when it is not");

            fence[0].X = Consts.NullDouble;

            Assert.True(fence.IsNull(), "Fence is not null when it is");
        }

        [Fact()]
        public void Test_FenceTests_IsNull_Empty()
        {
          var fence = new Fence();
          fence.IsNull().Should().Be(true);
        }
  }
}
