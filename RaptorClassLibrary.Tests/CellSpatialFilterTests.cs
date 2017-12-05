using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Designs;

namespace VSS.VisionLink.Raptor.Filters.Tests
{
    [TestClass()]
    public class CellSpatialFilterTests
    {
        [TestMethod()]
        public void Test_CellSpatialFilter_Creation()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            Assert.IsTrue(!filter.IsAlignmentMask && !filter.IsDesignMask && !filter.IsPositional && !filter.IsSpatial && !filter.IsSquare,
            "Cell spatial filter not created correctly");

            Assert.IsTrue(filter.LeftOffset == Consts.NullDouble && filter.RightOffset == Consts.NullDouble && 
                          filter.StartStation == Consts.NullDouble && filter.EndStation == Consts.NullDouble &&
                          filter.Fence.IsNull() && filter.AlignmentFence.IsNull() && 
                          filter.PositionX == Consts.NullDouble && filter.PositionY == Consts.NullDouble &&
                          filter.PositionRadius == Consts.NullDouble && filter.HasAlignmentDesignMask() == false &&
                          filter.HasSpatialOrPostionalFilters == false,// &&
//                          filter.OverrideSpatialCellRestriction == new BoundingIntegerExtent2D(),
            "Cell spatial filter not created correctly");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_ActiveFiltersString()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            Assert.IsTrue(filter.ActiveFiltersString() == "Spatial:False, Positional:False, DesignMask:False, AlignmentMask:False",
                "Incorrect filter.ActiveFiltersString() result: '{0}'", filter.ActiveFiltersString());
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_Clear()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.AlignmentFence.SetExtents(0, 0, 1, 1);
            filter.EndStation = 1000;
            filter.StartStation = 100;
            filter.LeftOffset = -1;
            filter.RightOffset = 1;
            filter.ReferenceDesign = new DesignDescriptor(1, "A", "B", "C", "D", 11);
            filter.Fence.SetExtents(1, 1, 2, 2);
            filter.PositionRadius = 10;
            filter.PositionX = 10;
            filter.PositionY = 10;

            filter.Clear();

            Assert.IsTrue(filter.AlignmentFence.IsNull() &&
                          filter.Fence.IsNull() &&
                          filter.EndStation == Consts.NullDouble &&
                          filter.StartStation == Consts.NullDouble &&
                          filter.LeftOffset == Consts.NullDouble &&
                          filter.RightOffset == Consts.NullDouble &&
                          filter.ReferenceDesign.IsNull &&
                          filter.PositionRadius == Consts.NullDouble &&
                          filter.PositionX == Consts.NullDouble &&
                          filter.PositionY == Consts.NullDouble,
                          "Filter.Clear did not clear all expected fields");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_SetAlignmentMask()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsAlignmentMask = true;
            filter.AlignmentFence = new Fence(0, 0, 1, 1);
            filter.ReferenceDesign = DesignDescriptor.Null();
            filter.ReferenceDesign.FileName = "c:\\dir\\file.ext";
            filter.StartStation = 100;
            filter.EndStation = 1000;
            filter.LeftOffset = -1;
            filter.RightOffset = 1;

            Assert.IsTrue(filter.IsAlignmentMask && !filter.AlignmentFence.IsNull() &&
                filter.StartStation == 100 && filter.EndStation == 1000 && filter.LeftOffset == -1 && filter.RightOffset == 1 &&
                !filter.ReferenceDesign.IsNull,
                "Alignment mask not initialised correctly");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_SetDesignMask()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsDesignMask = true;
            filter.StartStation = 100;
            filter.EndStation = 1000;
            filter.LeftOffset = -1;
            filter.RightOffset = 1;
            filter.ReferenceDesign.FileName = "Test.ttm";

            Assert.IsTrue(filter.IsDesignMask && 
                filter.StartStation == 100 && filter.EndStation == 1000 && filter.LeftOffset == -1 && filter.RightOffset == 1 &&
                !filter.ReferenceDesign.IsNull,
                "Alignment mask not initialised correctly");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_SetPositional()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsPositional = true;
            filter.PositionX = 100;
            filter.PositionY = 200;
            filter.PositionRadius = 300;
            filter.IsSquare = true;

            Assert.IsTrue(filter.IsPositional && filter.PositionX == 100 && filter.PositionY == 200 && filter.PositionRadius == 300 && filter.IsSquare,
                "Positional filter not initialised correctly");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_SetSpatial()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsSpatial = true;
            filter.Fence = new Fence(0, 0, 1, 1);

            Assert.IsTrue(filter.IsSpatial && filter.Fence.NumVertices == 4 && filter.Fence.Area() == 1,
                "Positional filter not initialised correctly");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_ClearAlignmentMask()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsAlignmentMask = true;
            filter.AlignmentFence = new Fence(0, 0, 1, 1);

            filter.ClearAlignmentMask();
            Assert.IsTrue(!filter.IsAlignmentMask && filter.AlignmentFence.IsNull(), "Alignment mask not cleared correctly");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_ClearDesignMask()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsDesignMask = true;
            filter.ReferenceDesign.FileName = "Test.ttm";

            filter.ClearDesignMask();

            Assert.IsTrue(!filter.IsDesignMask && filter.ReferenceDesign.IsNull,
                "Design mask not cleared correctly");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_ClearPositional()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsPositional = true;
            filter.PositionX = 100;
            filter.PositionY = 200;
            filter.PositionRadius = 300;
            filter.IsSquare = true;

            filter.ClearPositional();

            Assert.IsTrue(!filter.IsPositional && filter.PositionX == Consts.NullDouble && filter.PositionY == Consts.NullDouble && filter.PositionRadius == Consts.NullDouble && !filter.IsSquare,
                "Positional filter not cleared correctly");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_ClearSpatial()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            filter.IsSpatial = true;
            filter.Fence = new Fence(0, 0, 1, 1);

            filter.ClearSpatial();

            Assert.IsTrue(!filter.IsSpatial && filter.Fence.IsNull(),  "Spatial filter not cleared correctly");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_HasAlignmentDesignMask()
        {
            CellSpatialFilter filter = new CellSpatialFilter();

            Assert.IsFalse(filter.HasSpatialOrPostionalFilters, "Default fence does not have spatial or positional filter");
            filter.IsSpatial = true;

            Assert.IsTrue(filter.HasSpatialOrPostionalFilters, "Default fence does not have spatial or positional filter");
            filter.ClearSpatial();
            Assert.IsFalse(filter.HasSpatialOrPostionalFilters, "Default fence has spatial or positional filter");

            filter.IsPositional = true;

            Assert.IsTrue(filter.HasSpatialOrPostionalFilters, "Default fence does not have spatial or positional filter");
            filter.ClearPositional();
            Assert.IsFalse(filter.HasSpatialOrPostionalFilters, "Default fence has spatial or positional filter");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_IsCellInSelection_Polygon()
        {
            CellSpatialFilter filter = new CellSpatialFilter();
            filter.IsSpatial = true;
            filter.Fence = new Fence(0, 0, 1, 1);

            Assert.IsTrue(filter.IsCellInSelection(0.5, 0.5), "Cell location is IN the fence not OUT of it");
            Assert.IsFalse(filter.IsCellInSelection(10, 10), "Cell location is OUT of the fence not IN it");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_IsCellInSelection_Positional_PointRadiusCircular()
        {
            CellSpatialFilter filter = new CellSpatialFilter();
            filter.IsPositional = true;
            filter.PositionX = 0.0;
            filter.PositionY = 0.0;
            filter.PositionRadius = 1;
            filter.IsSquare = false;

            Assert.IsTrue(filter.IsCellInSelection(0.5, 0.5), "Cell location is IN the fence not OUT of it");
            Assert.IsTrue(filter.IsCellInSelection(0.0, 0.99999), "Cell location is IN the fence not OUT of it");
            Assert.IsTrue(filter.IsCellInSelection(0.0, -0.99999), "Cell location is IN the fence not OUT of it");
            Assert.IsTrue(filter.IsCellInSelection(0.99999, 0.0), "Cell location is IN the fence not OUT of it");
            Assert.IsTrue(filter.IsCellInSelection(-0.99999, 0.0), "Cell location is IN the fence not OUT of it");
            Assert.IsFalse(filter.IsCellInSelection(10, 10), "Cell location is OUT of the fence not IN it");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_IsCellInSelection_Positional_PointRadiusSquare()
        {
            CellSpatialFilter filter = new CellSpatialFilter();
            filter.IsPositional = true;
            filter.PositionX = 0.0;
            filter.PositionY = 0.0;
            filter.PositionRadius = 1.0;
            filter.IsSquare = true;

            Assert.IsTrue(filter.IsCellInSelection(0.5, 0.5), "Cell location is IN the fence not OUT of it");
            Assert.IsTrue(filter.IsCellInSelection(0.99909, 0.99999), "Cell location is IN the fence not OUT of it");
            Assert.IsTrue(filter.IsCellInSelection(0.99909, -0.99999), "Cell location is IN the fence not OUT of it");
            Assert.IsTrue(filter.IsCellInSelection(0.99999, 0.99909), "Cell location is IN the fence not OUT of it");
            Assert.IsTrue(filter.IsCellInSelection(-0.99999, 0.99909), "Cell location is IN the fence not OUT of it");
            Assert.IsFalse(filter.IsCellInSelection(10, 10), "Cell location is OUT of the fence not IN it");
        }

        [TestMethod()]
        public void Test_CellSpatialFilter_IsPositionInSelection()
        {
            CellSpatialFilter filter = new CellSpatialFilter();
            filter.IsSpatial = true;
            filter.Fence = new Fence();
            filter.Fence.SetExtents(0, 0, 1, 1);
            filter.Fence.UpdateExtents();

            Assert.IsTrue(filter.IsPositionInSelection(0.5, 0.5), "Cell location is IN the fence not OUT of it");
            Assert.IsFalse(filter.IsCellInSelection(10, 10), "Cell location is OUT of the fence not IN it");
        }
    }
}
